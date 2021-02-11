using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using WizardsCode.Animation;

namespace WizardsCode.Character
{
    [Serializable ]
    public class EmotionControlAsset : PlayableAsset, ITimelineClipAsset
    {
        [SerializeField]
        public EmotionControlPlayableBehaviour template = new EmotionControlPlayableBehaviour();

        public ClipCaps clipCaps {
            get { return ClipCaps.None; }
        }

        public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
        {
            return ScriptPlayable<EmotionControlPlayableBehaviour>.Create(graph, template);
        }
    }
}