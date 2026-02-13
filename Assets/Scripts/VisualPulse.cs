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

    public static event Action Perfect;

    private Vector3 baseScale;
    private bool pulsing;
    private bool circle;
    private Image image;

    private Sprite baseSprite;
    private Sprite newSprite;


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
            gameObject.SetActive(false);
            Debug.LogWarning($"Image: {image}");
        }


    }

    void OnEnable()
    {
        Pulse.OnTiming += SwitchImage;
        Pulse.OnEndRythm += DisabledCircle;

        MainEvents e = FindAnyObjectByType<MainEvents>();
        if (e != null)
            e.OnDarkLevel += SetDarkAssets;
    }

    void OnDisable()
    {
        Pulse.OnTiming -= SwitchImage;
        Pulse.OnEndRythm -= DisabledCircle;

        MainEvents e = FindAnyObjectByType<MainEvents>();
        if (e != null)
            e.OnDarkLevel -= SetDarkAssets;
    }

    public void Pulsing()
    {
        pulsing = true;
        if (circle)
        {
            gameObject.SetActive(true);
        }

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
                if (MainEvents.IsDarkLevel)
                    return "GA/UI/ui_test_sérieux_success"; 
                else
                    return "GA/UI/heart_UI_success";
            }
            else 
            {
                if (MainEvents.IsDarkLevel)
                    return "GA/UI/ui_test_sérieux_fail"; 
                else
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
            /*if (Input.touchCount > 0) break;*/
            yield return null;

        }

        if(MainEvents.IsDarkLevel)
            image.sprite = (Sprite)Resources.Load("GA/UI/heart_ui_sérieux", typeof(Sprite));
        else
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
        if (circle) 
        {
            gameObject.SetActive(false);
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

    private void SetDarkAssets()
    {
        image.sprite = (Sprite)Resources.Load("GA/UI/heart_ui_sérieux", typeof(Sprite));
        Circle.sprite = (Sprite)Resources.Load("GA/UI/Dark/ui_cercle_sérieux",typeof(Sprite));
    }

}
