using System;
using UnityEngine;

public class TouchScreen : MonoBehaviour
{

    [SerializeField] private float noteTime;
    [SerializeField] private float tolerance;

    private bool alreadyTapped;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (!alreadyTapped && Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);

            if (touch.phase == TouchPhase.Began)
            {
                float currentTime = Time.time;
                float difference = Mathf.Abs(currentTime - noteTime);

                alreadyTapped = true;
            }

        }
    }
}
