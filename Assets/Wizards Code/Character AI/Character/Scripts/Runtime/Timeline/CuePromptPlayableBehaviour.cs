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
    public class CuePromptPlayableBehaviour : PlayableBehaviour
    {
        [SerializeField]
        ActorCue cue;

        private bool isPrompted;

        public override void ProcessFrame(Playable playable, FrameData info, object playerData)
        {
            BaseActorController baseActor = playerData as BaseActorController;
            if (baseActor == null) return;

            if (!isPrompted)
            {
                baseActor.Prompt(cue, (float)playable.GetDuration());
                
                isPrompted = true;
            }
        }

        public override void OnBehaviourPlay(Playable playable, FrameData info)
        {
            isPrompted = false;
          }
    }
}
