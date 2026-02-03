using UnityEngine;
using System;

public class Pulse : MonoBehaviour
{
    [SerializeField]private AudioSource MusicSource;
    [SerializeField]private float BPM;
    [SerializeField]private float speed;
    [SerializeField]private float Tolerance;

    private float beatInterval;
    private float nextBeatInterval;

    TouchScreen touchScreen;

    private void OnEnable()
    {
        TouchScreen.OnTouch += BeatInterval;
    }

    private void OnDisable()
    {
        TouchScreen.OnTouch -= BeatInterval;
    }

    void Start()
    {
        InitMusic();
    }

    void Update()
    {
        NextBeat();
        BPMSpeed(1);
    }


    void InitMusic()
    {
        if (MusicSource == null)
        {
            Debug.Log("Select a sound in the field MusicSource of Heart Beats please");
            return;
        }
          
/*      MusicToEnabled = MusicSource.GetComponent<AudioSource>();
        MusicToEnabled.enabled = true;*/
        MusicSource.Play();
        beatInterval = 60f / BPM;
        nextBeatInterval = MusicSource.time + beatInterval;
        /*MusicSource.pitch = beatInterval;*/

    }

    void NextBeat() 
    {
        if (MusicSource.isPlaying && MusicSource.time >= nextBeatInterval) 
        {
            nextBeatInterval += beatInterval;
            Debug.LogWarning($"top : {MusicSource.time}");
        }
    }

    void BeatInterval()
    {
        if (MusicSource.isPlaying && MusicSource.time >= nextBeatInterval - Tolerance || MusicSource.isPlaying && MusicSource.time <= nextBeatInterval + Tolerance)
        {
            Debug.Log($"timing ok :{MusicSource.time}");
        }
        else 
        {
            Debug.Log($"timing ko{MusicSource.time}");
        }
    }

    void BPMSpeed(int speed)
    {
        MusicSource.pitch = speed;
    }

}

