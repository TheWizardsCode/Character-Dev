using System;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace WizardsCode.Character
{
    /// <summary>
    /// A timeline asset that will send a specific cue to the
    /// appropriate ActorCharacter.
    /// </summary>
    [Serializable ]
    public class CuePromptAsset : PlayableAsset, ITimelineClipAsset
    {
        [SerializeField]
        public CuePromptPlayableBehaviour template = new CuePromptPlayableBehaviour();

        public ClipCaps clipCaps {
            get { return ClipCaps.None; }
        }

        public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
        {
            return ScriptPlayable<CuePromptPlayableBehaviour>.Create(graph, template);
        }
    }
}