using System.Collections;
using UnityEngine;

public class AudioUISequenceController : MonoBehaviour
{
    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip introClip;
    public AudioClip secondClip;

    [Header("UI")]
    public CanvasGroup uiCanvas;

    [Header("UI Fade")]
    public float fadeDuration = 1.5f;

    private void Start()
    {
        if (uiCanvas)
        {
            uiCanvas.alpha = 0f;
            uiCanvas.gameObject.SetActive(false);
        }

        StartCoroutine(PlaySequence());
    }

    IEnumerator PlaySequence()
    {
        // ----------------------------
        // 1. Play Intro Audio
        // ----------------------------
        if (audioSource && introClip)
        {
            audioSource.clip = introClip;
            audioSource.Play();

            yield return new WaitForSeconds(introClip.length);
        }

        // ----------------------------
        // 2. Play Second Audio
        // ----------------------------
        if (audioSource && secondClip)
        {
            audioSource.clip = secondClip;
            audioSource.Play();
        }

        // ----------------------------
        // 3. Fade UI IN (while second audio plays)
        // ----------------------------
        if (uiCanvas)
        {
            uiCanvas.gameObject.SetActive(true);
            yield return StartCoroutine(FadeCanvas(0f, 1f));
        }
    }

    IEnumerator FadeCanvas(float from, float to)
    {
        float t = 0f;

        while (t < fadeDuration)
        {
            t += Time.deltaTime;

            float lerp = t / fadeDuration;

            uiCanvas.alpha = Mathf.Lerp(from, to, lerp);

            yield return null;
        }

        uiCanvas.alpha = to;
    }
}