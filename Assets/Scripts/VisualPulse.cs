using UnityEngine;

public class VisualPulse : MonoBehaviour
{

    [SerializeField]private float pulseScale = 1.2f;
    [SerializeField]private float pulseSpeed = 8f;

    private Vector3 baseScale;
    private bool pulsing;

    void Start()
    {
        baseScale = transform.localScale;
    }

    public void Pulse()
    {
        pulsing = true;
    }

    void Update()
    {
        if (pulsing)
        {
            transform.localScale = Vector3.Lerp(
                transform.localScale,
                baseScale * pulseScale,
                Time.deltaTime * pulseSpeed
            );

            if (Vector3.Distance(transform.localScale, baseScale * pulseScale) < 0.01f)
            {
                pulsing = false;
            }
        }
        else
        {
            transform.localScale = Vector3.Lerp(
                transform.localScale,
                baseScale,
                Time.deltaTime * pulseSpeed
            );
        }
    }
}
