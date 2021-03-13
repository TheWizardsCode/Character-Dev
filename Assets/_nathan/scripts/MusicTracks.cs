using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace _nathan.scripts
{
[CreateAssetMenu(fileName = "NewMusicTrackList", menuName = "inkjam_SOs")]
public class MusicTracks : ScriptableObject
{
    [SerializeField] public List<AudioSource_NameSourcePair> AudioClips;
}
}
