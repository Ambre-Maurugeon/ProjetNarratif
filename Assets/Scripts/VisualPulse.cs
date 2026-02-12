using System;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
public class VisualPulse : MonoBehaviour
{

    [SerializeField]private float pulseScale = 1.2f;
    [SerializeField]private float pulseSpeed = 8f;
    [SerializeField]private float durationOfSwitchedFeedback;
    [SerializeField]private Image Circle;

    public static event Action Perfect;

    private Vector3 baseScale;
    private bool pulsing;
    private bool circle;
    private Image image;

    private Sprite baseSprite;
    private Sprite newSprite;

    private void Awake()
    {
        if (image == Circle)
        {
            gameObject.SetActive(false);
        }
    }
    void Start()
    {
        baseScale = transform.localScale;
        image = GetComponent<Image>();

        if (image == null) Debug.LogWarning("Le composant Image n'est pas trouvé sur cet objet.");

        if (image != Circle)
        {

            baseSprite = image.sprite;
            circle = false;
            Debug.LogWarning($"Image: {image}");
        }
        else
        {
            circle = true;
            /*gameObject.SetActive(false);*/
            Debug.LogWarning($"Image: {image}");
        }


    }

    void OnEnable()
    {
        Pulse.OnTiming += SwitchImage;
        Pulse.OnEndRythm += DisabledCircle;
    }

    void OnDisable()
    {
        Pulse.OnTiming -= SwitchImage;
        Pulse.OnEndRythm -= DisabledCircle;
    }

    public void Pulsing()
    {
        pulsing = true;
        gameObject.SetActive(true);

    }

    void Update()
    {
        Pulsation();
        /*pulseProgress = CalculatePulseProgress(); // Calcul de l'avancement de la pulsation*/

    }

    private string ImagePath(bool b)
    {

        if (circle) return "";
        
            if (b)
            {
                return "GA/UI/heart_UI_success";
            }
            else 
            {
                return "GA/UI/heart_UI_fail";
            }
        
    }

    private void SwitchImage(bool timing)
    {
        if (!circle)
        {
            StartCoroutine(ChangeSpriteForDuration(durationOfSwitchedFeedback, timing));        
        }
    }

    IEnumerator ChangeSpriteForDuration(float duration, bool state) 
    {

        float elapsedTime = 0f;
        newSprite = Resources.Load<Sprite>(ImagePath(state));
        image.sprite = newSprite;

        while (elapsedTime <= durationOfSwitchedFeedback)
        {
            elapsedTime += Time.deltaTime;
            /*if (Input.touchCount > 0) break;*/
            yield return null;

        }

        image.sprite = baseSprite;
        /*yield return null;*/
    }


    public void Pulsation() 
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

    public void DisabledCircle()
    {

    }
/*
 * Pour une précision Diabolique |
 *                               |
 *                               V
    public float CalculatePulseProgress()
    {
        if (pulsing)
        {
            return Mathf.Clamp01(Time.time % pulseSpeed / pulseSpeed);
        }

        return 0f;
    }

    public bool IsPerfectTiming()
    {
        Handheld.Vibrate();
        Perfect?.Invoke();
        return Mathf.Approximately(pulseProgress, 1f);
    }*/

}
