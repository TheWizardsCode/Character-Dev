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
        private BaseActorController actor;

        public override void ProcessFrame(Playable playable, FrameData info, object playerData)
        {
            actor = playerData as BaseActorController;
            if (actor == null) return;

            if (!isPrompted)
            {
                actor.Prompt(cue);
                isPrompted = true;
            }
        }

        public override void OnBehaviourPlay(Playable playable, FrameData info)
        {
            isPrompted = false;
        }
    }
}
