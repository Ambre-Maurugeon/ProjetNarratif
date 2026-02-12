using UnityEngine;
using System;
using System.Diagnostics;
using Debug = UnityEngine.Debug;

public class Pulse : MonoBehaviour
{
        public static event Action OnEndRythm;
        public static event Action<bool> OnTiming;

        [SerializeField]VisualPulse heartPulse;
        [SerializeField]VisualPulse circlePulse;


        [Header("Rythme")]
        [SerializeField]public float bpm = 60f;          
        [SerializeField]public float tolerance = 0.25f; 
        [SerializeField]private float sequenceDuration = 30f;

        private float beatInterval;
        private float nextBeatTime;
        private bool sequenceActive = false;
        private float sequenceEndTime;

        private int failCount;
        private int perfectCount;

        public bool perfect;
        public bool good;   
        public bool bad;

        
    
        void Start()
        {
            beatInterval = 60f / bpm;
            /*StartSequence();*/

        }

    void Update()
        {
            if (sequenceActive)
            {
                if (Time.time >= sequenceEndTime || failCount > 5) 
                {   
                    sequenceActive = false;
                    Debug.Log("Séquence fini");
                    Result();
                    OnEndRythm?.Invoke();
                }

                // Génération du beat
                if (Time.time >= nextBeatTime)
                {
                    Debug.Log($"❤️ BEAT , Time : {Time.time}");
                    nextBeatTime += beatInterval;
                    Debug.Log($"nxtBtsTime : {nextBeatTime}");
                    heartPulse.Pulsing();
                    circlePulse.Pulsing();
                }

                // Touch tactile
                if (Input.touchCount > 0)
                {
                    Touch touch = Input.GetTouch(0);

                    if (touch.phase == TouchPhase.Began)
                    {
                        CheckTiming();
                    }
                }
            }
 
        }



        void CheckTiming()
        {
            float difference = Mathf.Abs(Time.time - (nextBeatTime - beatInterval));

            if (difference <= tolerance)
            {
                OnTiming?.Invoke(true);
                Debug.Log($"BON ✅, difference : {difference}, Tolerance : {tolerance}");
                Handheld.Vibrate();
        }
            else
            {
                OnTiming?.Invoke(false);
                failCount += 1;
                Debug.Log($"RATÉ ❌, difference : {difference}, Tolerance : {tolerance}");
            }

            if (Mathf.Approximately(difference, 0f))
            {
                perfectCount += 1;
               Handheld.Vibrate();
               Debug.Log("PERFEECT !!");
            }   
        }


    /* 60 Etat normal 
     * 90 Etat géné
     * 120 Stréssé
     * Utilisez Uniquement les valeurs ci dessus /!!!!!\ (risque de désynchro des séquences de rythmes)
     */
    public void BPMSpeed(float bpm)
        {
            this.bpm = bpm;
        }


    public void StartSequence() 
    {
        sequenceActive = true;
        nextBeatTime = Time.time + beatInterval;
        sequenceEndTime = Time.time + sequenceDuration;
        Debug.Log($"▶ Séquence commencée pour {sequenceDuration} secondes !");
    }

    public void Result()
    {
        if (perfectCount == nextBeatTime - 1) 
        {
            bool Perfect() => perfect;
        }

        if (failCount <= 3)
        {
            bool Good() => good;
        }
        else 
        {
            bool Bad() => bad;
        }
    }

}

