using UnityEngine;

public class Pulse : MonoBehaviour
{
    [SerializeField]private AudioSource MusicSource;
    [SerializeField]private float BPM;

    private float beatInterval;
    private float nextBeatInterval;

    private AudioSource MusicToEnabled;

    void Start()
    {
        InitMusic();
    }

    void Update()
    {
        NextBeat();
    }

    public void AugBPM(float bpm)
    {
        BPM = 10;
        beatInterval = 60f / BPM;
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
        MusicSource.pitch = beatInterval;
        nextBeatInterval = MusicSource.time + beatInterval;

    }

    void NextBeat() 
    {
        if (MusicSource.isPlaying && MusicSource.time >= nextBeatInterval) 
        {
            nextBeatInterval += beatInterval;
            Debug.Log($"top : {MusicSource.time}");
        }
    }

}

