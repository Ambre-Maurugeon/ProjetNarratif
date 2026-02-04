using System;
using UnityEngine;

public class TouchScreen : MonoBehaviour
{

    public static event Action OnTouch;
    Pulse pulse;
    private Touch touch;


    void Update()
    {
        Touch();
    }

    void Touch()
    {
        if (Input.touchCount > 0)
        {
            touch = Input.GetTouch(0);

            switch (touch.phase)
            {
                case TouchPhase.Began:
                    OnTouch?.Invoke();
                    /*Debug.Log($"Ecran touché au point: {touch.position}");*/
                    break;

                case TouchPhase.Moved:
                    /*Debug.Log($"Le doigt bouge: {touch.position}");*/
                    break;

                case TouchPhase.Stationary:
                    break;

                case TouchPhase.Canceled:
                    break;

                case TouchPhase.Ended:
                    break;
            }
        }
    }
}
