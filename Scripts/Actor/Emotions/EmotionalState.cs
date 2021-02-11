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
    [ExecuteAlways]
    public class EmotionalState : MonoBehaviour
    {
        public enum EmotionType { Anger, Interest, Fear, Sadness, Pleasure }
        public List<EmotionMetric> emotions = new List<EmotionMetric>();

        public void Awake()
        {
            emotions = new List<EmotionMetric>();

            EmotionMetric emotion = new EmotionMetric(EmotionType.Anger, 0, 1, -0.2f);
            emotions.Add(emotion);

            // Suggestions from https://en.wikipedia.org/wiki/Emotion#/media/File:Geneva_Emotion_Wheel_-_English.png
            // Hate - activationMultiplier = 0.8, pleasantnessMultiplier = -0.4
            // Contempt - activationMultiplier = 0.5, pleasantnessMultiplier = -0.5
            // Disgust - activationMultiplier = 0.3, pleasantnessMultiplier = -0.8


            emotion = new EmotionMetric(EmotionType.Fear, 0, 0.1f, -1f);
            emotions.Add(emotion);

            // Suggestions from https://en.wikipedia.org/wiki/Emotion#/media/File:Geneva_Emotion_Wheel_-_English.png
            // Disappointment - activationMultiplier = -0.1, pleasantnessMultiplier = -1
            // Shame - activationMultiplier = -0.3, pleasantnessMultiplier = -0.8
            // Regret - activationMultiplier = -0.5, pleasantnessMultiplier = -0.5
            // Guilt - activationMultiplier = -0.8, pleasantnessMultiplier = -0.3
            // Sadness - activationMultiplier = -1, pleasantnessMultiplier = -0.1

            emotion = new EmotionMetric(EmotionType.Sadness, 0, -1, -0.1f);
            emotions.Add(emotion);

            // Suggestions from https://en.wikipedia.org/wiki/Emotion#/media/File:Geneva_Emotion_Wheel_-_English.png
            // Compassion - activationMultiplier = -1, pleasantnessMultiplier = 0.1
            // Relief - activationMultiplier = -0.8, pleasantnessMultiplier = 0.3
            // Admiration - activationMultiplier = -0.6, pleasantnessMultiplier = 0.5
            // Love - activationMultiplier = -0.3, pleasantnessMultiplier = 0.8
            // Contentment - activationMultiplier = -0.1, pleasantnessMultiplier = 1

            emotion = new EmotionMetric(EmotionType.Pleasure, 0, 0.1f, 1f);
            emotions.Add(emotion);

            // Suggestions from https://en.wikipedia.org/wiki/Emotion#/media/File:Geneva_Emotion_Wheel_-_English.png
            // Joy - activationMultiplier = 0.3, pleasantnessMultiplier = 0.8
            // Pride - activationMultiplier = 0.5, pleasantnessMultiplier = 0.5
            // Amusement - activationMultiplier = 0.8, pleasantnessMultiplier = 0.3

            emotion = new EmotionMetric(EmotionType.Interest, 0, 0.45f, 0.2f);
            emotions.Add(emotion);

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
    }

    /// <summary>
    /// An EmotionMetric is a measure of a single emotion on the Two Dimensions of emotions:
    /// https://en.wikipedia.org/wiki/Emotion#/media/File:Geneva_Emotion_Wheel_-_English.png
    /// </summary>
    [Serializable]
    public class EmotionMetric
    {
        public EmotionType type; // The name of the emotion
        public float value; // the value, from 0 to 1 of this emotion
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
    }
}