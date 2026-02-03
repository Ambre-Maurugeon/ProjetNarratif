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

    public bool PerfectAchievement;
    public bool GoodAchievement;
    public bool MediumAchievement;
    public bool BadAchievement;

    public bool goodscore()
    {
        return GoodAchievement;
    }


    TouchScreen touchScreen;
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
        PerfectAchievement = false;
        GoodAchievement = false;
        MediumAchievement = false;
        BadAchievement = false;
        playing = false;

        //InitMusic();
    }

    void Update()
    {
        NextBeat();
        BPMSpeed(1);
    }


    public void InitMusic()
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

        double bpmTimePlaying = dspStartTime;

        if (bpmTimePlaying >= nextBeatInterval)
        {
            nextBeatInterval += beatInterval;
            nbInterval++;
            /*Debug.Log($"Count: {nbInterval}");*/
            Debug.LogWarning($"nextBeatInterval : {nextBeatInterval}");
        }

        //if (nbInterval >= 22)
        //{
        //    Result();
        //    Stop();
        //}

        Invoke("Result", 6f);
        Invoke("Stop", 7f);
    }

    void BeatInterval()
    {
        if (!playing) return;

        double tapTime = AudioSettings.dspTime - dspStartTime;
        Debug.Log($"tapTime: {tapTime}");

        // Beat le plus proche
        double closestBeat = Math.Round(tapTime / beatInterval) * beatInterval;
        Debug.Log($"closestBeat: {closestBeat}");
        double delta = Math.Abs(tapTime - closestBeat);
        Debug.Log($"delta: {delta}");

        if (delta <= Tolerance)
        {
            timingResult  += 1;
            Debug.Log($"Timing OK");
        }
        else
        {
            timingResult -= 1;
            Debug.Log($"Timing KO");
        }
    }

    public void BPMSpeed(int speed)
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
            PerfectAchievement = true;
        }
        else if (result >= 0.8f && result < 1f)
        {
            Debug.Log("80% ~ 99%");
            GoodAchievement = true;
        }
        else if (result >= 0.5f && result <= 0.7f)
        {
            Debug.Log("50% ~ 79%");
            MediumAchievement = true;
        }
        else if (result < 0.4f)
        {
            Debug.Log("0% ~ 49%");
            BadAchievement = true;
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