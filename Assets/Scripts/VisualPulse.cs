using System;
using System.Collections;
using Unity.VisualScripting;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.UI;
public class VisualPulse : MonoBehaviour
{

    [SerializeField]private float pulseScale = 1.2f;
    [SerializeField]private float pulseSpeed = 8f;
    [SerializeField]private float durationOfSwitchedFeedback;
    [SerializeField]private Image Circle;
    [SerializeField] private float fillDuration = 2f;


    public static event Action Perfect;

    private Vector3 baseScale;
    private bool pulsing;
    private bool circle;
    private bool isFading = false;
    private Image image;

    private Sprite baseSprite;
    private Sprite newSprite;
    private CanvasGroup canvasGroup;


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

            if (canvasGroup == null) 
            {
                canvasGroup = gameObject.AddComponent<CanvasGroup>();
            }

            /*gameObject.SetActive(false);*/
            canvasGroup.alpha = 0f;
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
/*        if (circle)
        {
            StartCoroutine(FadeAndFillThenPulse());
        }*/

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

        newSprite = (Sprite)Resources.Load(ImagePath(state), typeof(Sprite));
        image.sprite = newSprite;

        while (elapsedTime <= durationOfSwitchedFeedback)
        {
            elapsedTime += Time.deltaTime;
            yield return null;

        }

        image.sprite = baseSprite;
    }

    IEnumerator Fade(float start, float end, float duration)
    {
        isFading = true;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(start, end, elapsed / duration);
            yield return null;
        }

        canvasGroup.alpha = end;
        
    }
    IEnumerator FillRoutine()
    {
        float elapsed = 0f;

        if (circle)
        {
            while (elapsed < fillDuration)
            {
                elapsed += Time.deltaTime;
                image.fillAmount = elapsed / fillDuration;
                yield return null;
            }

            image.fillAmount = 1f;
        }
    }

     public IEnumerator FadeAndFillThenPulse() 
    {
        if (isFading) yield break;

          StartCoroutine(Fade(0f, 1f, 0.5f));

          StartCoroutine(FillRoutine());

        pulsing = true;

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
        if (circle) 
        {
            isFading = false;
            StartCoroutine(Fade(1f, 0f, 1f));
        }
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
