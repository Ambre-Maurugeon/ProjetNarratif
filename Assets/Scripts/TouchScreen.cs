using System;
using UnityEngine;

public class TouchScreen : MonoBehaviour
{

    private bool alreadyTapped;


    // Update is called once per frame
    void Update()
    {
        if (!alreadyTapped && Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);

            if (touch.phase == TouchPhase.Began)
            {
                float currentTime = Time.time;
                alreadyTapped = true;
            }

        }
    }
}
