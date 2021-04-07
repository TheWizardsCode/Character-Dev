using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Timeline;

namespace WizardsCode.Character
{
    [TrackColor(241/255, 249/255, 99/255)]
    [TrackBindingType(typeof(EmotionalState))]
    [TrackClipType(typeof(EmotionControlAsset))]
    public class EmotionControlTrack : TrackAsset
    {
    }
}
