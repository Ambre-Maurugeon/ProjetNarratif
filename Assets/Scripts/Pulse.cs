using UnityEngine;
using System;

public class Pulse : MonoBehaviour
{
    [SerializeField] private AudioSource MusicSource;
    [SerializeField] private float BPM;
    [SerializeField] private float speed;
    [SerializeField] private float Tolerance;

    private double dspStartTime;
    private float beatInterval;
    private float nextBeatInterval;
    private bool playing;
    private int nbInterval = 0;
    private int timingResult = 0;


    public bool perfectAchievement;
    public bool goodAchievement;
    public bool mediumAchievement;
    public bool badAchievement;
    public double bpmTimePlaying;

    public bool PerfectAchievement() => perfectAchievement;
    public bool GoodAchievement() => goodAchievement;
    public bool MediumAchievement() => mediumAchievement;
    public bool BadAchievement() => badAchievement;


    public static event Action OnEndRythm;

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
        perfectAchievement = false;
        goodAchievement = false;
        mediumAchievement = false;
        badAchievement = false;
        playing = false;

        //PlayMusic();
    }

    void Update()
    {
        NextBeat();
        BPMSpeed(speed);
    }


    public void PlayMusic()
    {
        if (MusicSource == null)
        {
            Debug.Log("Select a sound in the field MusicSource of Heart Beats please");
            return;
        }

        beatInterval = 60f / BPM;
        MusicSource.Play();

        dspStartTime = AudioSettings.dspTime;
        Debug.LogWarning(dspStartTime);
        nextBeatInterval = (float)dspStartTime + beatInterval;

        /*MusicSource.loop = true;*/
        playing = true;

    }

    void NextBeat()
    {
        if (!playing) return;

         bpmTimePlaying = AudioSettings.dspTime - dspStartTime;

        if (bpmTimePlaying >= nextBeatInterval)
        {
            nextBeatInterval += beatInterval;
            nbInterval++;
            /*Debug.Log($"Count: {nbInterval}");*/
            Debug.LogWarning($"nextBeatInterval : {nextBeatInterval}");
        }

        if (nbInterval >= 22)
        {
            Result();
            Stop();
        }

    }

    void BeatInterval()
    {
        if (!playing) return;

        double tapTime = AudioSettings.dspTime - dspStartTime;
        // Beat le plus proche
        double closestBeat = Math.Round(tapTime / beatInterval) * beatInterval;
        double delta = Math.Abs(tapTime - closestBeat);

        Debug.Log($"time: {bpmTimePlaying}, tapTime: {tapTime}, delta: {delta}, closestBeat: {closestBeat}");

        if (delta <= Tolerance)
        {
            timingResult  += 1;
            Debug.Log($"Timing OK : {dspStartTime}");
        }
        else
        {
            timingResult -= 1;
            Debug.Log($"Timing KO : {dspStartTime}");
        }
    }

    public void BPMSpeed(float speed)
    {
        if (!playing) return;
        MusicSource.pitch = speed;
    }

        void Result()
    {


        if (timingResult < 0) 
        {
            timingResult = 1;
        }

        float result =  timingResult / nbInterval;

        if (result == 1f)
        {
            Debug.Log("100%");
            perfectAchievement = true;
        }
        else if (result >= 0.8f && result < 1f)
        {
            Debug.Log("80% ~ 99%");
            goodAchievement = true;
        }
        else if (result >= 0.5f && result <= 0.7f)
        {
            Debug.Log("50% ~ 79%");
            mediumAchievement = true;
        }
        else if (result < 0.4f)
        {
            Debug.Log("0% ~ 49%");
            badAchievement = true;
        }
    }

    public void Stop()
    {
        if (MusicSource.isPlaying)
        {
            MusicSource.Stop();
            playing = false;
            OnEndRythm?.Invoke();
            Debug.Log("fini");
        }
    }

}