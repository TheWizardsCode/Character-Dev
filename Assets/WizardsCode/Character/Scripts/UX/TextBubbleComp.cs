using System.Collections;
using TMPro;
using UnityEngine;
using Random = UnityEngine.Random;

// todo -find a permanent home for this, it wouldn't let it be in my own directory
namespace WizardsCode.Character.Scripts.UX
{


public class TextBubbleComp : MonoBehaviour
{
    // ****************************************************************************** private fields
    [SerializeField]
    TextMeshProUGUI _TMP_SpeakersName;

    [SerializeField]
    TextMeshProUGUI _TMP_Text;

    [SerializeField]
    AudioClip[] _speechSounds;

    [SerializeField]
    AudioClip[] _punctuationSounds;

    [SerializeField]
    AudioSource _audioSource_Speech;

    [SerializeField]
    AudioSource _audioSource_Punctuation;

    [SerializeField]
    float _secondsBetweenPrintingChars = 0.01f;

    [SerializeField]
    float _growOrShrinkSpeed = 4.0f;

    [SerializeField]
    bool _bPlaySpeakingSounds = true;

    float _targetScale = 1.0f;
    float _closeEnough = 0.01f; // small enough to be invisible
    float _prettySmall = 0.1f; // small enough to be able to detect we're aiming for small



    // ***************************************************************************** private methods
    /** interpolate scale to 1 or to 0 */
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
        // helper method, possibly used int he future to help with a bounce effect
        float t = Time.deltaTime * _growOrShrinkSpeed;
        float x = rectTransform.transform.localScale.x * (1 - t) + (_targetScale * t);
        float y = rectTransform.transform.localScale.y * (1 - t) + (_targetScale * t);
        rectTransform.transform.localScale = new Vector2(x, y);
    }

    /** reveal chars, once per pass */
    IEnumerator RevealChars()
    {
        while (_TMP_Text.maxVisibleCharacters < _TMP_Text.text.Length)
        {
            _TMP_Text.maxVisibleCharacters++;
            if (_bPlaySpeakingSounds)
            {
                ProduceSpeechSound(_TMP_Text.text.ToCharArray()[_TMP_Text.maxVisibleCharacters - 1]);
            }

            yield return new WaitForSeconds(_secondsBetweenPrintingChars);
        }
    }

    /** produce a very short sound based on what type of char is passed in. */
    void ProduceSpeechSound(char c)
    {
        if (char.IsPunctuation(c) && !_audioSource_Punctuation.isPlaying)
        {
            _audioSource_Speech.Stop();

            if (_punctuationSounds != null && _punctuationSounds.Length > 0)
            {
                _audioSource_Punctuation.clip = _punctuationSounds[Random.Range(0, _punctuationSounds.Length)];
                _audioSource_Punctuation.Play();
            }
        }
        else if (char.IsLetter(c) && !_audioSource_Speech.isPlaying)
        {
            _audioSource_Punctuation.Stop();
            if (_speechSounds != null && _speechSounds.Length > 0)
            {
                _audioSource_Speech.clip = _speechSounds[Random.Range(0, _speechSounds.Length)];
                _audioSource_Speech.Play();
            }
        }
    }



    // ****************************************************************************** public methods
    /** convenience method, detects if _trget_ scale is small */
    public bool IsHidden()
    {
        return _targetScale < _prettySmall;
    }

    /** note: this invokes show widget automatically */
    public void SetText(string speakersName, string text, bool bPlaySpeakingSounds)
    {
        ShowWidget(true);

        if (_TMP_SpeakersName == null || _TMP_Text == null)
        {
            Debug.LogError("tmp component(s) are  null");
            return;
        }

        _TMP_SpeakersName.text = speakersName;
        _TMP_Text.text = text;

        // this is what lets us show characters one at a time: we increment this later
        _TMP_Text.maxVisibleCharacters = 0;

        _bPlaySpeakingSounds = bPlaySpeakingSounds;

        StartCoroutine("RevealChars");
    }

    /** note: this invokes show widget automatically */
    public void SetText(string speakersName, string text, bool bPlaySpeakingSounds, float secondsBetweenPrintingChars)
    {
        ShowWidget(true);
        _secondsBetweenPrintingChars = secondsBetweenPrintingChars;
        SetText(speakersName, text, bPlaySpeakingSounds);
    }

    /** convenience method used in test scene in lieu of having a test suite */
    public void TestDisplayingText()
    {
        SetText("buddy", "Yeah, but, that doesn't mean he always gets it right away. Sometimes it takes a couple of shots to capture an entire moment in a single shot.", true);
    }

    public void ClearText()
    {
        _TMP_SpeakersName.text = "";

        _TMP_Text.text = "";
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



    // ********************************************************************************* unity stuff
}
}
