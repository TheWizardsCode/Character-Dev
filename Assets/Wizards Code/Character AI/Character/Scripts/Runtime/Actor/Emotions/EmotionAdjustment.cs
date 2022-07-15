using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using static WizardsCode.Character.EmotionalState;
using System;

namespace WizardsCode.Character.AI
{
    /// <summary>
    /// Change an emotion by a given amount.
    /// </summary>
    [Serializable]
    public struct EmotionAdjustment
    {
        [Tooltip("The emotion type to adjust.")]
        public EmotionType emotionType;
        [Tooltip("The amound to increase or decrease the emotion. Not the final value of the emotion is clamped between -1 and 1.")]
        [Range(-1f, 1f)]
        public float change;

        public void ApplyTo(EmotionalState state)
        {
            if (state == null)
            {
                Debug.LogError("Called EmotionalAdjustment.ApplyFor(EmotionalState state) but state is null.");
                return;
            }

            state.SetEmotionValue(emotionType, state.GetEmotionValue(emotionType) + change);
        }
    }
}
