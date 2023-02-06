using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static WizardsCode.Character.EmotionalState;

namespace WizardsCode.Character
{
    /// <summary>
    /// The IEmotionalState tracks the current state of mind, in terms of emotions, of a character.
    /// 
    /// See https://en.wikipedia.org/wiki/Emotion#/media/File:Geneva_Emotion_Wheel_-_English.png
    /// </summary>
    public class EmotionalState : MonoBehaviour
    {
        public enum EmotionType { Anger, Interest, Fear, Sadness, Pleasure }

        [Header("Animator")]
        // TODO consider if this is the right place for this parameter, it seems odd to directly control it in the emotional state. Can it be separated out into another layer? Or perhaps this class is poorly named?
        [SerializeField, Tooltip("Should the character crouch when fearful, interested and not angry?")]
        internal bool m_CrouchInFear = true;
        [SerializeField, Tooltip("The Animator boolean parameter name that will cause the character to crouch when moving/idle.")]
        string m_CrouchParameterName = "Crouch";

        [HideInInspector]
        public List<EmotionMetric> emotions = new List<EmotionMetric>();

        Animator m_Animator;
        int m_CrouchParameterHash;

        public void Awake()
        {
            m_Animator = transform.root.GetComponentInChildren<Animator>();
            m_CrouchParameterHash = Animator.StringToHash(m_CrouchParameterName);
        }

        private void Update()
        {
            if (m_CrouchInFear && GetEmotionValue(EmotionType.Fear) > 0.9f 
                && GetEmotionValue(EmotionType.Anger) < 0.6 )
            {
                m_Animator.SetBool(m_CrouchParameterHash, true);
            } else
            {
                m_Animator.SetBool(m_CrouchParameterHash, false);
            }
        }

        /// <summary>
        /// Get the current value of a given emotion.
        /// </summary>
        public float GetEmotionValue(EmotionType type)
        {
            // TODO: cache the emotion metrics in a dictionary for faster access
            for (int i = 0; i < emotions.Count; i++)
            {
                if (emotions[i].type == type)
                {
                    return emotions[i].value;
                }
            }

            throw new KeyNotFoundException("No emotion with the required name: " + name);
        }

        public void SetEmotionValue(EmotionType type, float value)
        {
            for (int i = 0; i < emotions.Count; i++)
            {
                if (emotions[i].type == type)
                {
                    emotions[i].value = value;
                    return;
                }
            }
        }

        /// <summary>
        /// Returns a value between 0 and 1 reflecting how activated the character is.
        /// 0 being not activated at all, 0.5 being average and 1 being highly activated.
        /// </summary>
        /// <returns>A value between 0 and 1 reflecting how activated the character is.</returns>
        public float activationValue
        {
            get {
                // TODO Cache this?
                float activation = 0.5f;
                for (int i = 0; i < emotions.Count; i++)
                {
                    activation += emotions[i].value * emotions[i].activationMultiplier * 0.5f;
                }
                return Mathf.Clamp01(activation);
            }
        }

        /// <summary>
        /// Get a descriptive name for the current level of activation of the character.
        /// </summary>
        /// <returns></returns>
        public string activationDescriptor
        {
            get
            {
                float activation = activationValue;
                if (activation >= 0.9)
                {
                    return "Activated";
                }
                else if (activation >= 0.7)
                {
                    return "Engaged";
                }
                else if (activation > 0.4)
                {
                    return "Stable";
                }
                else if (activation > 0.2)
                {
                    return "Disengaged";
                }
                else
                {
                    return "Calm";
                }
            }
        }

        /// <summary>
        /// Returns a value between 0 and 1 reflecting how much the characters enjoys their current emotional state.
        /// 0 being depressed, 0.5 being average and 1 being elated.
        /// </summary>
        /// <returns>A value between 0 and 1 reflecting how activated the character is.</returns>
        public float enjoymentValue
        {
            get
            {
                // TODO Cache this?
                float enjoyment = 0.5f;
                for (int i = 0; i < emotions.Count; i++)
                {
                    enjoyment += emotions[i].value * emotions[i].enjoymentMultiplier * 0.5f;
                }
                return Mathf.Clamp01(enjoyment);
            }
        }

        /// <summary>
        /// Get a descriptive name for the current enjoyment level of the character.
        /// </summary>
        /// <returns></returns>
        public string enjoymentDescriptor
        {
            get
            {
                float enjoyment = enjoymentValue;
                if (enjoyment >= 0.9)
                {
                    return "Elated";
                }
                else if (enjoyment >= 0.7)
                {
                    return "Happy";
                }
                else if (enjoyment > 0.4)
                {
                    return "Content";
                }
                else if (enjoyment > 0.2)
                {
                    return "Bored";
                }
                else
                {
                    return "Sad";
                }
            }
        }

        /// <summary>
        /// A measure of how noticable this character is from 0 to 1. 
        /// 0 is as good as invisible, 1 is can't miss them.
        /// How noticable an actor is depends on their emational state.
        /// For example, a fearful character who is resting is less noticeable
        /// than an interested character. Anger will increase noticability,
        /// but sadness will reduce it.
        /// </summary>
        public float Noticability
        {
            get
            {
                //TODO this algorithm for noticability should not be hard coded. Add a noticability factor to individual emotion types.
                float result = GetEmotionValue(EmotionType.Anger);
                result -= GetEmotionValue(EmotionType.Fear);
                result += GetEmotionValue(EmotionType.Interest) / 2;
                result -= GetEmotionValue(EmotionType.Sadness) / 2;
                result /= 4;

                // we now have a result between -1 and 1, so normalize it
                return (result + 1) / 2;
            }
        }
    }

    /// <summary>
    /// An EmotionMetric is a measure of a single emotion on the Two Dimensions of emotions:
    /// https://en.wikipedia.org/wiki/Emotion#/media/File:Geneva_Emotion_Wheel_-_English.png
    /// </summary>
    [Serializable]
    public class EmotionMetric
    {
        public EmotionType type; // The name of the emotion
        float m_value; // the value, from 0 to 1 of this emotion
        public float activationMultiplier;
        public float enjoymentMultiplier;

        /// <summary>
        /// Sets up an emotion to influence the overall mood of the character based on the
        /// Two Dimensions of Emotion - see https://en.wikipedia.org/wiki/Emotion#/media/File:Geneva_Emotion_Wheel_-_English.png
        /// </summary>
        /// <param name="type">The type of the emotion.</param>
        /// <param name="value">The value of this emotion metric from 0 to 1. 0 is at the center of the model.</param>
        /// <param name="activationMultiplier">The impact this emotion has on the characters level of activation. 1 will mean maximum positive activation effect, 0 is no effect, -1 means maximum negative effect on activation.</param>
        /// <param name="enjoymentMultiplier">The impact this emotion has on the characters level of pleasure. 1 will mean maximum positive pleasure effect, 0 is no effect, -1 is the maximum negative effect on pleasure.</param>
        public EmotionMetric(EmotionType type, float value, float activationMultiplier, float enjoymentMultiplier)
        {
            this.type = type;
            this.value = value;
            this.activationMultiplier = activationMultiplier;
            this.enjoymentMultiplier = enjoymentMultiplier;
        }

        /// <summary>
        /// Get or set the value of this emotion. The value is clamped between -1 and +1.
        /// </summary>
        public float value
        {
            get { return m_value; }
            set
            {
                m_value = Mathf.Clamp(m_value + value, -1f, 1f);
            }
        }
    }
}