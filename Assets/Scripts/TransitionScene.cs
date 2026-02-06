using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class TransitionScene : MonoBehaviour
{
    public float FadeRate = 5f;
    private Image image;
    private float targetAlpha;

    void Start()
    {
        image = GetComponent<Image>();
        if (image == null)
        {
            Debug.LogError("TransitionScene: Image component manquant.");
            return;
        }
    
        if (image.material != null)
        {
            Material instantiatedMaterial = Instantiate(image.material);
            image.material = instantiatedMaterial;
        }
    
        Color c = image.color;
        c.a = 0f;
        image.color = c;
        targetAlpha = c.a;
    }

    public IEnumerator FadeIn()
    {
        if (image == null)
            yield break;

        gameObject.SetActive(true);
        targetAlpha = 1.0f;
        Color curColor = image.color;

        while (Mathf.Abs(curColor.a - targetAlpha) > 0.001f)
        {
            curColor.a = Mathf.MoveTowards(curColor.a, targetAlpha, FadeRate * Time.deltaTime);
            image.color = curColor;
            yield return null;
        }

        curColor.a = targetAlpha;
        image.color = curColor;
    }

    public IEnumerator FadeOut()
    {
        if (image == null)
            yield break;

        targetAlpha = 0f;
        Color curColor = image.color;

        while (Mathf.Abs(curColor.a - targetAlpha) > 0.001f)
        {
            curColor.a = Mathf.MoveTowards(curColor.a, targetAlpha, FadeRate * Time.deltaTime);
            image.color = curColor;
            yield return null;
        }

        curColor.a = targetAlpha;
        image.color = curColor;

        gameObject.SetActive(false);
    }
}