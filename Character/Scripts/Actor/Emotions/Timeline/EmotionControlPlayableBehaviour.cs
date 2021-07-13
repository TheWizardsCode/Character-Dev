using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using static WizardsCode.Character.EmotionalState;

namespace WizardsCode.Character
{
    [Serializable]
    public class EmotionControlPlayableBehaviour : PlayableBehaviour
    {
        [SerializeField]
        EmotionType emotionType = EmotionType.Pleasure;
        [SerializeField, Range(0f, 1f)]
        internal float value = 0.5f;

        private EmotionalState emotionalState;

        public override void ProcessFrame(Playable playable, FrameData info, object playerData)
        {
            emotionalState = playerData as EmotionalState;
            if (emotionalState == null) return;

            emotionalState.SetEmotionValue(emotionType, value);
        }
    }
}
