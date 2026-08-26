using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class RedFogSequenceController : MonoBehaviour
{
    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip introClip;

    [Header("Fog Material")]
    public Material fogMaterial;
    public string densityProperty = "_Density";

    public float startDensity = 5f;
    public float clearDensity = 1.2f;
    public float densityTransitionTime = 3f;

    [Header("Objects To Reveal")]
    public GameObject[] objectsToReveal;
    public float visibleDuration = 10f;

    [Header("Permanent Object")]
    public GameObject permanentObject;

    [Header("UI")]
    public CanvasGroup firstCanvasGroup;
    public CanvasGroup resultCanvas;

    public float canvasFadeDuration = 2f;

    [Header("Canvas Transition After Sequence")]
    public CanvasGroup canvasToFadeOut;
    public CanvasGroup canvasToFadeIn;
    public float transitionFadeDuration = 1.5f;

    [Header("Sequence Complete Event")]
    public UnityEvent onSequenceCompleted;

    private bool sequenceRunning;

    private void Start()
    {
        if (fogMaterial)
            fogMaterial.SetFloat(densityProperty, startDensity);

        if (firstCanvasGroup)
        {
            firstCanvasGroup.alpha = 0f;
            firstCanvasGroup.gameObject.SetActive(false);
        }

        if (resultCanvas)
        {
            resultCanvas.alpha = 0f;
            resultCanvas.gameObject.SetActive(false);
        }

        if (canvasToFadeIn)
        {
            canvasToFadeIn.alpha = 0f;
            canvasToFadeIn.gameObject.SetActive(false);
        }

        foreach (GameObject obj in objectsToReveal)
        {
            if (obj)
                obj.SetActive(false);
        }

        if (permanentObject)
            permanentObject.SetActive(false);
    }

    // ==================================================
    // START BUTTON
    // ==================================================
    public void PlayIntroAndStartSequence()
    {
        if (sequenceRunning)
            return;

        StartCoroutine(MainRoutine());
    }

    // ==================================================
    // MAIN ROUTINE
    // ==================================================
    IEnumerator MainRoutine()
    {
        sequenceRunning = true;

        if (audioSource && introClip)
        {
            audioSource.clip = introClip;
            audioSource.Play();
        }

        if (firstCanvasGroup)
        {
            firstCanvasGroup.gameObject.SetActive(true);

            yield return StartCoroutine(
                FadeCanvas(firstCanvasGroup, 0f, 1f));
        }

        yield return StartCoroutine(
            PlaySceneSequence());

        if (audioSource)
        {
            yield return new WaitWhile(
                () => audioSource.isPlaying);
        }

        if (resultCanvas)
        {
            resultCanvas.gameObject.SetActive(true);

            yield return StartCoroutine(
                FadeCanvas(resultCanvas, 0f, 1f));
        }

        yield return StartCoroutine(
            TransitionCanvases());

        onSequenceCompleted?.Invoke();

        sequenceRunning = false;
    }

    // ==================================================
    // SCENE SEQUENCE
    // ==================================================
    IEnumerator PlaySceneSequence()
    {
        yield return StartCoroutine(
            AnimateFog(startDensity, clearDensity));

        foreach (GameObject obj in objectsToReveal)
        {
            if (obj)
                obj.SetActive(true);
        }

        if (permanentObject)
            permanentObject.SetActive(true);

        yield return new WaitForSeconds(
            visibleDuration);

        foreach (GameObject obj in objectsToReveal)
        {
            if (obj)
                obj.SetActive(false);
        }

        yield return StartCoroutine(
            AnimateFog(clearDensity, startDensity));
    }

    // ==================================================
    // FOG ANIMATION
    // ==================================================
    IEnumerator AnimateFog(float from, float to)
    {
        float t = 0f;

        while (t < densityTransitionTime)
        {
            t += Time.deltaTime;

            float lerp = t / densityTransitionTime;

            if (fogMaterial)
            {
                fogMaterial.SetFloat(
                    densityProperty,
                    Mathf.Lerp(from, to, lerp));
            }

            yield return null;
        }

        if (fogMaterial)
            fogMaterial.SetFloat(
                densityProperty,
                to);
    }

    // ==================================================
    // CANVAS FADE
    // ==================================================
    IEnumerator FadeCanvas(
        CanvasGroup cg,
        float from,
        float to)
    {
        float t = 0f;

        while (t < canvasFadeDuration)
        {
            t += Time.deltaTime;

            float lerp = t / canvasFadeDuration;

            cg.alpha = Mathf.Lerp(
                from,
                to,
                lerp);

            yield return null;
        }

        cg.alpha = to;
    }

    // ==================================================
    // CANVAS TRANSITION
    // ==================================================
    IEnumerator TransitionCanvases()
    {
        if (!canvasToFadeOut &&
            !canvasToFadeIn)
            yield break;

        if (canvasToFadeIn)
        {
            canvasToFadeIn.gameObject.SetActive(true);
            canvasToFadeIn.alpha = 0f;
        }

        float timer = 0f;

        float startOutAlpha =
            canvasToFadeOut
            ? canvasToFadeOut.alpha
            : 0f;

        while (timer < transitionFadeDuration)
        {
            timer += Time.deltaTime;

            float t =
                timer / transitionFadeDuration;

            if (canvasToFadeOut)
            {
                canvasToFadeOut.alpha =
                    Mathf.Lerp(
                        startOutAlpha,
                        0f,
                        t);
            }

            if (canvasToFadeIn)
            {
                canvasToFadeIn.alpha =
                    Mathf.Lerp(
                        0f,
                        1f,
                        t);
            }

            yield return null;
        }

        if (canvasToFadeOut)
        {
            canvasToFadeOut.alpha = 0f;
            canvasToFadeOut.gameObject.SetActive(false);
        }

        if (canvasToFadeIn)
        {
            canvasToFadeIn.alpha = 1f;
        }
    }

    // ==================================================
    // RESET
    // ==================================================
    public void ResetSequence()
    {
        StopAllCoroutines();

        sequenceRunning = false;

        if (audioSource)
            audioSource.Stop();

        if (fogMaterial)
            fogMaterial.SetFloat(
                densityProperty,
                startDensity);

        if (firstCanvasGroup)
        {
            firstCanvasGroup.alpha = 0f;
            firstCanvasGroup.gameObject.SetActive(false);
        }

        if (resultCanvas)
        {
            resultCanvas.alpha = 0f;
            resultCanvas.gameObject.SetActive(false);
        }

        if (canvasToFadeOut)
        {
            canvasToFadeOut.alpha = 1f;
        }

        if (canvasToFadeIn)
        {
            canvasToFadeIn.alpha = 0f;
            canvasToFadeIn.gameObject.SetActive(false);
        }

        foreach (GameObject obj in objectsToReveal)
        {
            if (obj)
                obj.SetActive(false);
        }

        if (permanentObject)
            permanentObject.SetActive(false);
    }
}