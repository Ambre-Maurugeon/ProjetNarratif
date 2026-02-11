using UnityEngine;
using System;
using System.Diagnostics;
using Debug = UnityEngine.Debug;

public class Pulse : MonoBehaviour
{
        public static event Action OnEndRythm;
        public static event Action<bool> OnTiming;

        [SerializeField]VisualPulse visualPulse;

        [Header("Rythme")]
        [SerializeField]public float bpm = 60f;          
        [SerializeField]public float tolerance = 0.25f; 
        [SerializeField] private float sequenceDuration = 30f;

        private float beatInterval;
        private float nextBeatTime;
        private bool sequenceActive = false;
        private float sequenceEndTime;

        void Start()
        {
            beatInterval = 60f / bpm;
            //StartSequence();

        }

    void Update()
        {
            if (sequenceActive)
            {
                if (Time.time >= sequenceEndTime) 
                {   
                    sequenceActive = false;
                    Debug.Log("Séquence fini");
                    OnEndRythm?.Invoke();
                }

                // Génération du beat
                if (Time.time >= nextBeatTime)
                {
                    Debug.Log($"❤️ BEAT , Time : {Time.time}");
                    nextBeatTime += beatInterval;
                    Debug.Log($"nxtBtsTime : {nextBeatTime}");
                    visualPulse.Pulsing();
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
            }
            else
            {
                OnTiming?.Invoke(false);
                Debug.Log($"RATÉ ❌, difference : {difference}, Tolerance : {tolerance}");
            }
        }

        /* 60 Etat normal 
         * 90 Etat gémé
         * 120 Stréssé
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

}

