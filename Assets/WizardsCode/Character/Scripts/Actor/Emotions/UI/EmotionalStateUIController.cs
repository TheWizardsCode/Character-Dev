using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace WizardsCode.Character.UI
{
    /// <summary>
    /// Manages a screen overlay that displays a characters current emotional state.
    /// </summary>
    public class EmotionalStateUIController : MonoBehaviour
    {
        [Header("Character")]
        [SerializeField, Tooltip("The Emotional State of the character we want to view")]
        EmotionalState m_EmotionalState;
        [SerializeField, Tooltip("The Animator of the character we want to view")]
        Animator m_Animator;

        [Header("UI Elements")]
        [SerializeField, Tooltip("The numerical display of the current Activation state of the character.")]
        Text m_ActivationValueText;
        [SerializeField, Tooltip("The numerical display of the current Enjoyment state of the character.")]
        Text m_EnjoymentValueText;
        [SerializeField, Tooltip("The numerical display of the current Animator Activation value.")]
        Text m_AnimatorActivationValueText;
        [SerializeField, Tooltip("The numerical display of the current Animator Enjoyment value.")]
        Text m_AnimatorEnjoymentValueText;
        [SerializeField, Tooltip("The emotion control template to use when creating new UI elements for emotions.")]
        GameObject m_EmotionTemplate;

        private void Awake()
        {
            if (m_EmotionalState == null)
            {
                Debug.LogWarning("No Emotional State has been set to track, disabling the EmotionalStateUI.");
                gameObject.SetActive(false);
            }
        }

        private void Start()
        {
            for (int i = 0; i < m_EmotionalState.emotions.Count; i++)
            {
                GameObject control = Instantiate(m_EmotionTemplate, transform);
                string name = m_EmotionalState.emotions[i].type.ToString(); ;
                control.name = name;
                control.GetComponentInChildren<Text>().text = name;
                Slider slider = control.GetComponentInChildren<Slider>();
                slider.onValueChanged.AddListener(delegate { SetEmotionValue(name, slider.value); });
                control.SetActive(true);
            }
        }

        public void SetEmotionValue(string name, float value)
        {
            EmotionalState.EmotionType type = (EmotionalState.EmotionType)Enum.Parse(typeof(EmotionalState.EmotionType), name);
            m_EmotionalState.SetEmotionValue(type, value);
        }

        void Update()
        {
            m_AnimatorActivationValueText.text = m_Animator.GetFloat("Activation").ToString("0.000");
            if (m_ActivationValueText != null)
            {
                m_ActivationValueText.text = m_EmotionalState.activationValue.ToString("0.000");
            }

            m_AnimatorEnjoymentValueText.text = m_Animator.GetFloat("Enjoyment").ToString("0.000");
            if (m_EnjoymentValueText != null)
            {
                m_EnjoymentValueText.text = m_EmotionalState.enjoymentValue.ToString("0.000");
            }
        }
    }
}