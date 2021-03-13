using System;
using System.Collections;
using System.Collections.Generic;
using _nathan.scripts;
using UnityEngine;

public enum EMusicTrackName
{
    MTN_Normal,
    MTN_Happy,
    MTN_Intense,
    MTN_Flourish_Intro,
    MTN_Flourish_Outro
}

[Serializable]
public struct AudioSource_NameSourcePair {
     public EMusicTrackName name;
     public AudioClip source;
}

[RequireComponent(typeof(AudioSource))]
public class GlobalMusic : MonoBehaviour
{
    // ******************************************************************************** local fields
    // [SerializeField] List<AudioSource_NameSourcePair> AudioClips;
    [SerializeField] MusicTracks _musicTracks;

    [SerializeField] EMusicTrackName _startingTrackName = EMusicTrackName.MTN_Normal;
    [SerializeField] AudioSource _trackA;
    public AudioSource TrackA
    { get => _trackA; }

    [SerializeField] AudioSource _trackB;
    public AudioSource TrackB
    { get => _trackB; }


    /** the  sum of the volumes of track A and B will always add to 1 */
    [SerializeField] float _trackA_targetWeight;
    [SerializeField] float _trackB_targetWeight;

    /** used when performing a track replacement */
    // AudioSource _trackA_pending;
    // AudioSource _trackB_pending;

    [SerializeField] float _interpSpeed;

    const float _smallNumber = 0.01f;



    // ****************************************************************************** public methods
    public void SetTrackA()
    {
    }



    // ********************************************************************************* unity stuff
    void Awake()
    {
        if (_musicTracks == null)
        {
            Debug.LogError("music tracks null");
            return;
        }

        if (_trackA == null) Debug.LogError("trackA null");
        if (_trackB == null) Debug.LogError("trackB null");
    }

    void Start()
    {
        if (_musicTracks == null) return;

        foreach (var nameSourcePair in _musicTracks.AudioClips)
        {
            if (nameSourcePair.name == _startingTrackName)
            {
                _trackA.clip = nameSourcePair.source;
                break;
            }
        }

        if (_trackA.clip)
        {
            _trackA.Play();
        }
    }

    void Update()
    {
        // for (int i = 0; i < audioSources.Length; i++) {
        //         AudioSource track = audioSources[i];
        //         if (track == activeMoodLoop) {
        //             if (track.volume < 1) {
        //                 track.volume += Time.deltaTime / fadeTime;
        //             }
        //         } else {
        //             if (track.volume > 0) {
        //                 track.volume -= Time.deltaTime / fadeTime;
        //             }
        //         }
        //     }
    }
}
