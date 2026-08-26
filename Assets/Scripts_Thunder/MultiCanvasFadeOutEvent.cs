using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class MultiCanvasFadeController : MonoBehaviour
{
    [Header("Canvases To Fade Out")]
    public CanvasGroup[] canvases;

    [Header("Fade Settings")]
    public float fadeDuration = 1.5f;

    [Header("Event After Fade")]
    public UnityEvent onFadeComplete;

    private bool isFading;

    public void TriggerFadeOut()
    {
        if (isFading)
            return;

        StartCoroutine(FadeOutRoutine());
    }

    IEnumerator FadeOutRoutine()
    {
        isFading = true;

        float t = 1f;

        while (t > 0f)
        {
            t -= Time.deltaTime / fadeDuration;

            foreach (var canvas in canvases)
            {
                if (canvas)
                    canvas.alpha = t;
            }

            yield return null;
        }

        foreach (var canvas in canvases)
        {
            if (canvas)
            {
                canvas.alpha = 0f;
                canvas.gameObject.SetActive(false);
            }
        }

        onFadeComplete?.Invoke();

        isFading = false;
    }
}