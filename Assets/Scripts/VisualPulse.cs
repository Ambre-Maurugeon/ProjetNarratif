using System.Collections;
using UnityEngine;
using UnityEngine.UI;
public class VisualPulse : MonoBehaviour
{

    [SerializeField]private float pulseScale = 1.2f;
    [SerializeField]private float pulseSpeed = 8f;
    [SerializeField] private float durationOfSwitchedFeedback;

    private Vector3 baseScale;
    private bool pulsing;
    private Image image;

    private Sprite baseSprite;
    private Sprite newSprite;

    void Start()
    {
        baseScale = transform.localScale;
        image = GetComponent<Image>();
        if (image == null) Debug.LogWarning("Le composant Image n'est pas trouvé sur cet objet.");
        baseSprite = image.sprite;
    }

    void OnEnable()
    {
        Pulse.OnTiming += SwitchImage;
    }

    void OnDisable()
    {
        Pulse.OnTiming -= SwitchImage;
    }

    public void Pulsing()
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

    private string ImagePath(bool b)
    {

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
        StartCoroutine(ChangeSpriteForDuration(durationOfSwitchedFeedback, timing));        
    }

    IEnumerator ChangeSpriteForDuration(float duration, bool state) 
    {

        float elapsedTime = 0f;
        newSprite = Resources.Load<Sprite>(ImagePath(state));
        image.sprite = newSprite;

        while (elapsedTime <= durationOfSwitchedFeedback)
        {
            elapsedTime += Time.deltaTime;
            if (Input.touchCount > 0) break;
            yield return null;

        }

        image.sprite = baseSprite;
        /*yield return null;*/
    }

}
