using System;
using UnityEngine;
using static WizardsCode.Character.EmotionalState;

namespace WizardsCode.Character.AI
{
    /// <summary>
    /// Provides a test against the current emotional state of the character.
    /// This can be used to influence behaviour decisions. For example, an AI
    /// may be more likely to adopt the take cover behaviour when they are 
    /// scared.
    /// </summary>
    [Serializable]
    public struct EmotionalConditionTest
    {
        public enum Objective { LessThan, Approximately, GreaterThan }

        [Tooltip("The emotion metric we require a value for.")]
        public EmotionType emotionType;
        [Tooltip("The objective for this emotion value, for example, greater than, less than or approximatly equal to.")]
        public Objective objective;
        [Tooltip("The value required for this emotion metric (used in conjunction with the objective).")]
        public float value;

        public bool isTrueFor(EmotionalState state)
        {
            if (state == null)
            {
                Debug.LogError("Called EmotionalConditionalTest.isTrueFor(EmotionalState state) but state is null");
                return false;
            }

            float emotionValue = state.GetEmotionValue(emotionType);

            switch (objective)
            {
                case Objective.LessThan:
                    return emotionValue < value;
                case Objective.Approximately:
                    return Mathf.Approximately(emotionValue, value);
                case Objective.GreaterThan:
                    return emotionValue > value;
                default:
                    return false;
            }
        }
    }
}
