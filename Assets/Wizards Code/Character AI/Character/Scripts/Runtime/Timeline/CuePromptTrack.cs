﻿using UnityEngine.Timeline;

namespace WizardsCode.Character
{
    /// <summary>
    /// A Timeline track that enables cues to be sent to an 
    /// ActorCharacter.
    /// </summary>
    [TrackColor(241/255, 249/255, 99/255)]
    [TrackBindingType(typeof(BaseActorController))]
    [TrackClipType(typeof(CuePromptAsset))]
    public class CuePromptTrack : TrackAsset
    {
    }
}
