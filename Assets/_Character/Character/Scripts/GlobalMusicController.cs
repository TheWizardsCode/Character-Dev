using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;


public enum EMusicTrackName
{
    MTN_Main, // almost always plays alongside other tracks
    MTN_Fun,
    MTN_Investigation,
    MTN_Suspense
}

[Serializable]
public class MusicTrackData {
     public EMusicTrackName name;
     public AudioSource audioSource;
     public float targetWeightForMixing; // changes at runtime often
}

public class GlobalMusicController : MonoBehaviour
{
    // ******************************************************************************** local fields
    [SerializeField] EMusicTrackName _startingTrackName = EMusicTrackName.MTN_Fun;

    [SerializeField] List<MusicTrackData> _musicTrackDataList;

    EMusicTrackName _currentPrimaryTrackName; // note: main is always played

    [SerializeField] float _blendTime = 6.0f;

    const float _smallNumber = 0.01f;
    float _interpParam = 1.0f;



    // ****************************************************************************** public methods
    public void SetPrimaryTrack(EMusicTrackName trackName)
    {
        _currentPrimaryTrackName = trackName;
        for (int i = 0; i < _musicTrackDataList.Count; i++)
        {
            if (_musicTrackDataList[i].name == trackName ||
                _musicTrackDataList[i].name == EMusicTrackName.MTN_Main)
            {
                _musicTrackDataList[i].targetWeightForMixing = 1.0f;
            }
            else
            {
                _musicTrackDataList[i].targetWeightForMixing = 0.0f;
            }
        }

        _interpParam = 0.0f;
    }



    // ********************************************************************************* unity stuff
    void Awake()
    {
        if (_musicTrackDataList == null)
        {
            Debug.LogError("_musicTrackDataList null");
            return;
        }
    }

    void Start()
    {
        if (_musicTrackDataList == null) return;

        _currentPrimaryTrackName = _startingTrackName;
        for (int i = 0; i < _musicTrackDataList.Count; i++)
        {
            var musicTrackData = _musicTrackDataList[i];

            if (musicTrackData.audioSource == null)
            {
                Debug.LogError("musicTrackData.audioSource is null");
                break;
            }
            else if (musicTrackData.audioSource.clip == null)
            {
                Debug.LogError("musicTrackData.audioSource.clip is null");
                break;
            }

            musicTrackData.audioSource.Play();

            if (musicTrackData.name == _startingTrackName ||
                musicTrackData.name == EMusicTrackName.MTN_Main)
            {
                musicTrackData.targetWeightForMixing = 1.0f;
                musicTrackData.audioSource.volume = 1.0f;
            }
            else
            {
                musicTrackData.targetWeightForMixing = 0.0f;
                musicTrackData.audioSource.volume = 0.0f;
            }
        }
    }

    void Update()
    {
        if (Mathf.Abs(_interpParam) > 0.999) return;

        _interpParam += Time.deltaTime / _blendTime;

        for (int i = 0; i < _musicTrackDataList.Count; i++)
        {
            var musicTrackData = _musicTrackDataList[i];
            if (musicTrackData.name == _currentPrimaryTrackName &&
                    musicTrackData.audioSource != null)
            {
                // lerp from 0 to 1
                float currentVol = musicTrackData.audioSource.volume;
                float pendingVol = _interpParam * musicTrackData.targetWeightForMixing;
                if (pendingVol > currentVol)
                {
                    musicTrackData.audioSource.volume = pendingVol;
                }
            }
            else if (musicTrackData.name != EMusicTrackName.MTN_Main &&
                    musicTrackData.audioSource != null)
            {
                // lerp from 1 to 0
                float currentVol = musicTrackData.audioSource.volume;
                float pendingVol = 1 - _interpParam;
                if (pendingVol < currentVol)
                {
                    musicTrackData.audioSource.volume = pendingVol;
                }
            }
        }
    }
}
