using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using WizardsCode.Character;
using Random = UnityEngine.Random;

namespace WizardsCode.Ink
{
    public class TextBubbleController : MonoBehaviour
    {
        #region Inspector Fields
        [SerializeField]
        [FormerlySerializedAs("The GUI component that will display the speakers name.")]
        TextMeshProUGUI m_SpeakersName;

        [SerializeField]
        [FormerlySerializedAs("The GUI component that will display the complete text of this chunk.")]
        TextMeshProUGUI m_StoryText;

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
        [FormerlySerializedAs("RUNTIME ONLY: The delay between characters being printed. If set to 0 all characters will be printed at once. In the editor a value of 0 will always be used, regardless of the setting here.")]
        internal float m_SecondsBetweenPrintingChars = 0.01f;

        [SerializeField]
        [FormerlySerializedAs("_GrowShrinkSpeed")]
        float m_GrowOrShrinkSpeed = 4.0f;

        [SerializeField, Tooltip("Should the story text in the UI be cleared every time we add new text? Set to true if your UI does not handle scrolling well.")]
        bool m_ClearOnNewText = false;
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
            m_StoryText.maxVisibleCharacters = 0;
            ClearText();
            ShowWidget(false);
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

        IEnumerator ShowOrHide()
        {
            RectTransform rectTransform = GetComponent<RectTransform>();
            while (!ScaleIsCloseEnough(rectTransform))
            {
                InterpScale(rectTransform);
                yield return null;
            }

            if (_targetScale < _prettySmall)
            {
                rectTransform.transform.localScale = new Vector2(0.0f, 0.0f);
            }
        }

        bool ScaleIsCloseEnough(RectTransform rectTransform)
        {
            return Mathf.Abs(rectTransform.transform.localScale.x - _targetScale) < _closeEnough &&
                   Mathf.Abs(rectTransform.transform.localScale.y - _targetScale) < _closeEnough;
        }


        void InterpScale(RectTransform rectTransform)
        {
            float t = Time.deltaTime * m_GrowOrShrinkSpeed;
            float x = rectTransform.transform.localScale.x * (1 - t) + (_targetScale * t);
            float y = rectTransform.transform.localScale.y * (1 - t) + (_targetScale * t);
            rectTransform.transform.localScale = new Vector2(x, y);
        }

        /** reveal chars, once per pass */
        IEnumerator RevealChars()
        {
            isFinished = false;
            float lastTime = Time.realtimeSinceStartup;

            while (m_StoryText.maxVisibleCharacters < m_StoryText.text.Length)
            {
                if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Mouse0) || Input.GetKeyDown(KeyCode.Mouse1))
                {
                    m_StoryText.maxVisibleCharacters = m_StoryText.text.Length;
                    break;
                }

                m_StoryText.maxVisibleCharacters += Mathf.RoundToInt((Time.realtimeSinceStartup - lastTime) / m_SecondsBetweenPrintingChars);
                if (m_PlaySpeakingSounds)
                {
                    ProduceSpeechSound(m_StoryText.text.ToCharArray()[m_StoryText.maxVisibleCharacters - 1]);
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



        // ****************************************************************************** public methods
        /** convenience method, detects if _trget_ scale is small */
        public bool IsHidden()
        {
            return _targetScale < _prettySmall;
        }

        /// <summary>
        /// Add text to the current speech bubble. If the speaker has changed then clear the existing text, otherwise add the new text to existing text. The number of displayed characters will remain the same. This has the effect of continuing the display cycle.
        /// </summary>
        /// <param name="speaker">The curent speaker. Set to null if this is descriptive text or an unnamed narrator.</param>
        /// <param name="text">The text to display. This will be displayed one character at a time based on the default delay between characters (set in the inspector, use `SetText(speaker, text, playSounds, delay)` to override.</param>
        public void AddText(BaseActorController speaker, string text)
        {
            ShowWidget(true);
            
            if (m_ActiveSpeaker != null && m_ActiveSpeaker != speaker)
            {
                if (m_ClearOnNewText)
                {
                    ClearText();
                    m_StoryText.maxVisibleCharacters = 0;
                }
            }
            m_ActiveSpeaker = speaker;

            int displayedCharacters = m_StoryText.maxVisibleCharacters;
            if (m_ActiveSpeaker)
            {
                SetText(speaker, $"{m_StoryText.text}\n\n{speaker.displayName}: {text}");
            } else
            {
                SetText(speaker, $"{m_StoryText.text}\n\n{text}");
            }

            if (m_SecondsBetweenPrintingChars > 0)
            {
                m_StoryText.maxVisibleCharacters = displayedCharacters;
            }
        }

        public void SetText(BaseActorController speaker, string text)
        {
            SetText(speaker, text, m_PlaySpeakingSounds);
        }

        /// <summary>
        /// Set the test to a specific value. Any previous content will be removed.
        /// </summary>
        /// <param name="speaker">The curent speaker. Set to null if this is descriptive text or an unnamed narrator.</param>
        /// <param name="text">The text to display. This will be displayed one character at a time based on the default delay between characters (set in the inspector, use `SetText(speaker, text, playSounds, delay)` to override.</param>
        /// <param name="bPlaySpeakingSounds">Override the default setting (in the inspector) for playing speaking sounds.</param>
        public void SetText(BaseActorController speaker, string text, bool bPlaySpeakingSounds)
        {
            m_PlaySpeakingSounds = bPlaySpeakingSounds;
            ShowWidget(true);

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

            m_StoryText.text = text;
            if (m_SecondsBetweenPrintingChars > 0)
            {
                m_StoryText.maxVisibleCharacters = 0;
                revealCo = StartCoroutine(RevealChars());
            }
            else
            {
                m_StoryText.maxVisibleCharacters = m_StoryText.text.Length;
                if (m_PlaySpeakingSounds)
                {
                    ProduceSpeechSound('a');
                }
                scrollRect.verticalNormalizedPosition = 0;
            }
        }

        /** note: this invokes show widget automatically */
        public void SetText(BaseActorController speaker, string text, bool bPlaySpeakingSounds, float secondsBetweenPrintingChars)
        {
            ShowWidget(true);
            m_SecondsBetweenPrintingChars = secondsBetweenPrintingChars;
            SetText(speaker, text, bPlaySpeakingSounds);
        }

        public void ClearText()
        {
            m_SpeakersName.text = "";
            m_StoryText.text = "";
            if (revealCo != null)
            {
                StopCoroutine(revealCo);
                revealCo = null;
            }
        }

        /** if false and currently not hidden, it will call ClearText() automatically */
        public void ShowWidget(bool Value)
        {
            if (Value)
            {
                if (!IsHidden()) return;
                _targetScale = 1.0f;
            }
            else
            {
                if (IsHidden()) return;
                ClearText();
                _targetScale = 0.0f;
            }

            StartCoroutine("ShowOrHide");
        }
    }
}
