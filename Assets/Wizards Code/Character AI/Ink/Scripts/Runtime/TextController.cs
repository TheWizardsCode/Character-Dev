using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using WizardsCode.Character;
using Random = UnityEngine.Random;

namespace WizardsCode.Ink
{
    public class TextController : MonoBehaviour
    {
        #region Inspector Fields
        [SerializeField]
        [FormerlySerializedAs("The GUI component that will display the speakers name.")]
        TextMeshProUGUI m_SpeakersName;

        [SerializeField, Tooltip("The GUI component that will display the current text of this chunk.")]
        TextMeshProUGUI m_CurrentText;
        [SerializeField, Tooltip("The GUI component that will display the previos text of this chunk. If this is set then each time new text is sent to the story text the current content will be moved to this component, unless clear on new text is true.")]
        TextMeshProUGUI m_PreviousText;
        [SerializeField, Tooltip("Should the story text in the UI be cleared every time we add new text? Set to true if your UI does not handle scrolling well.")]
        bool m_ClearOnNewText = false;

        [SerializeField]
        [FormerlySerializedAs("Whether or not sounds should be played.")]
        bool m_PlaySpeakingSounds = true;
        private Coroutine revealCo;
        [SerializeField]
        [FormerlySerializedAs("An array of sounds that will be played while narration or speech is occuring")]
        AudioClip[] m_SpeechSounds;

        [SerializeField]
        [FormerlySerializedAs("An array of sounds that will be played when punctuation is detected.")]
        AudioClip[] m_PunctuationSounds;

        [SerializeField]
        [FormerlySerializedAs("The audio source for the speech sounds.")]
        AudioSource m_AudioSource_Speech;

        [SerializeField]
        [FormerlySerializedAs("The audio source for the punctuation sounds.")]
        AudioSource m_AudioSourcePunctuation;

        [SerializeField]
        [FormerlySerializedAs("RUNTIME ONLY: The delay between characters being printed. If set to 0 all characters will be printed at once." +
            " Note that the minimum time between characters is the duration of a single frame (at 60fps that is 0.0167s)," +
            " any setting below this will have the effect of displaying one character per frame." +
            " In the editor a value of 0 will always be used, regardless of the setting here.")]
        internal float m_SecondsBetweenPrintingChars = 0.01f;
        #endregion


        float _targetScale = 1.0f;
        float _closeEnough = 0.01f; // small enough to be invisible
        float _prettySmall = 0.1f; // small enough to be able to detect we're aiming for small
        private BaseActorController m_ActiveSpeaker;
        private ScrollRect scrollRect;

        /// <summary>
        /// Test to see if the controller is actively displaying text. If the process has completed then this will return true.
        /// </summary>
        public bool isFinished
        {
            get;
            private set;
        }

        #region Lifecycle Events
        void Start()
        {
            isFinished = true;
            m_CurrentText.maxVisibleCharacters = 0;
            ClearText();
            scrollRect = GetComponentInChildren<ScrollRect>();
#if UNITY_EDITOR
            //m_SecondsBetweenPrintingChars = 0;
#endif
        }

        void OnEnable()
        {
            if (revealCo != null)
            {
                revealCo = StartCoroutine(RevealChars());
            }
        }

        private void OnDisable()
        {
            if (revealCo != null)
            {
                StopCoroutine(revealCo);
            }
        }
        #endregion

        /** reveal chars, once per pass */
        IEnumerator RevealChars()
        {
            isFinished = false;
            float lastTime = Time.realtimeSinceStartup;

            while (m_CurrentText.maxVisibleCharacters < m_CurrentText.text.Length)
            {
                if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Mouse0) || Input.GetKeyDown(KeyCode.Mouse1))
                {
                    m_CurrentText.maxVisibleCharacters = m_CurrentText.text.Length;
                    break;
                }

                m_CurrentText.maxVisibleCharacters += Mathf.RoundToInt((Time.realtimeSinceStartup - lastTime) / m_SecondsBetweenPrintingChars);
                if (m_PlaySpeakingSounds && m_CurrentText.text.Length > 0)
                {
                    if (m_CurrentText.text.Length < m_CurrentText.maxVisibleCharacters)
                    {
                        ProduceSpeechSound(m_CurrentText.text.ToCharArray()[m_CurrentText.maxVisibleCharacters - 1]);
                    } else
                    {
                        ProduceSpeechSound(m_CurrentText.text.ToCharArray()[m_CurrentText.text.Length - 1]);
                    }
                }

                scrollRect.verticalNormalizedPosition = 0;

                lastTime = Time.realtimeSinceStartup;
                yield return new WaitForSeconds(m_SecondsBetweenPrintingChars);
            }

            isFinished = true;
        }

        /** produce a very short sound based on what type of char is passed in. */
        void ProduceSpeechSound(char c)
        {
            if (char.IsPunctuation(c) && !m_AudioSourcePunctuation.isPlaying)
            {
                m_AudioSource_Speech.Stop();

                if (m_PunctuationSounds != null && m_PunctuationSounds.Length > 0)
                {
                    m_AudioSourcePunctuation.clip = m_PunctuationSounds[Random.Range(0, m_PunctuationSounds.Length)];
                    m_AudioSourcePunctuation.Play();
                }
            }
            else if (char.IsLetter(c) && !m_AudioSource_Speech.isPlaying)
            {
                m_AudioSourcePunctuation.Stop();
                if (m_SpeechSounds != null && m_SpeechSounds.Length > 0)
                {
                    m_AudioSource_Speech.clip = m_SpeechSounds[Random.Range(0, m_SpeechSounds.Length)];
                    m_AudioSource_Speech.Play();
                }
            }
        }
        
        /** convenience method, detects if _trget_ scale is small */
        public bool IsHidden()
        {
            return _targetScale < _prettySmall;
        }

        /// <summary>
        /// Add text to the current text element, clearing the existing text if the speaker has changed.
        /// If clearOnNewText is true clear the existing text regardless of speaker change status. 
        /// If a previousText component is set then any existing text will first be mved to that.
        /// </summary>
        /// <param name="speaker">The curent speaker. Set to null if this is descriptive text or an unnamed narrator.</param>
        /// <param name="text">The text to display. This will be displayed one character at a time based on the default delay between characters (set in the inspector, use `SetText(speaker, text, playSounds, delay)` to override.</param>
        public void AddText(BaseActorController speaker, string text)
        {
            if (m_ActiveSpeaker != speaker)
            {
                ClearText();
                m_CurrentText.maxVisibleCharacters = 0;
            }
            m_ActiveSpeaker = speaker;

            int displayedCharacters = m_CurrentText.maxVisibleCharacters;
            if (m_ActiveSpeaker)
            {
                SetText(speaker, $"{m_CurrentText.text}\n{speaker.displayName}> {text}");
            } else
            {
                SetText(speaker, $"{m_CurrentText.text}\n{text}");
            }

            if (m_SecondsBetweenPrintingChars > 0)
            {
                m_CurrentText.maxVisibleCharacters = displayedCharacters;
            }
        }

        public void SetText(BaseActorController speaker, string text)
        {
            SetText(speaker, text, m_PlaySpeakingSounds);
        }

        /// <summary>
        /// Set the text to a specific value. Any content in the currentText element be added to the previousText and then removed from currentText.
        /// </summary>
        /// <param name="speaker">The curent speaker. Set to null if this is descriptive text or an unnamed narrator.</param>
        /// <param name="text">The text to display. This will be displayed one character at a time based on the default delay between characters (set in the inspector, use `SetText(speaker, text, playSounds, delay)` to override.</param>
        /// <param name="bPlaySpeakingSounds">Override the default setting (in the inspector) for playing speaking sounds.</param>
        public void SetText(BaseActorController speaker, string text, bool bPlaySpeakingSounds)
        {
            if (m_ClearOnNewText)
            {
                ClearText();
                m_CurrentText.maxVisibleCharacters = 0;
            }
            
            m_PlaySpeakingSounds = bPlaySpeakingSounds;

            m_ActiveSpeaker = speaker;
            if (speaker)
            {
                m_SpeakersName.text = speaker.displayName;
                // FIXME: shouldn't be navigating the tree like this, make the speakers name element the root object and discover the text object beneath
                m_SpeakersName.transform.parent.gameObject.SetActive(true);
            } else
            {
                // FIXME: shouldn't be navigating the tree like this, make the speakers name element the root object and discover the text object beneath
                m_SpeakersName.transform.parent.gameObject.SetActive(false);
            }
            
            m_CurrentText.text = text;
            if (m_SecondsBetweenPrintingChars > 0)
            {
                m_CurrentText.maxVisibleCharacters = 0;
                revealCo = StartCoroutine(RevealChars());
            }
            else
            {
                m_CurrentText.maxVisibleCharacters = m_CurrentText.text.Length;
                if (m_PlaySpeakingSounds)
                {
                    ProduceSpeechSound('a');
                }
                scrollRect.verticalNormalizedPosition = 0;
            }
        }

        public void SetText(BaseActorController speaker, string text, bool bPlaySpeakingSounds, float secondsBetweenPrintingChars)
        {
            m_SecondsBetweenPrintingChars = secondsBetweenPrintingChars;
            SetText(speaker, text, bPlaySpeakingSounds);
        }

        /// <summary>
        /// Clear all current text, stopping the reveal co routine if it is running.
        /// If previousText is set then move all existing text into the previousText element before clearing.
        /// </summary>
        public void ClearText()
        {
            m_SpeakersName.text = "";
            if (m_PreviousText != null)
            {
                m_PreviousText.text += m_CurrentText.text;
            }
            m_CurrentText.text = "";
            if (revealCo != null)
            {
                StopCoroutine(revealCo);
                revealCo = null;
            }
        }
    }
}
