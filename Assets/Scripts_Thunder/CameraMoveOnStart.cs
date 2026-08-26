using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class CameraMoveFadeAndHide : MonoBehaviour
{
    [Header("Camera Movement")]
    public Transform firstPoint;
    public Transform finalPoint;

    public float moveToFirstDuration = 5f;
    public float moveToFinalDuration = 5f;

    [Header("Wait At First Point")]
    public float waitDuration = 5f;

    [Header("Audio")]
    public AudioSource audioSource;
    public float audioFadeDuration = 2f;

    [Header("Skybox Exposure")]
    public Material skyboxMaterial;
    public float targetExposure = 1.5f;
    public float exposureTransitionTime = 2f;

    [Header("Canvas Fade")]
    public CanvasGroup[] canvasesToFade;
    public float fadeDuration = 2f;

    [Header("Objects To Hide")]
    public GameObject[] objectsToHide;

    private void Start()
    {
        StartCoroutine(MainSequence());
    }

    IEnumerator MainSequence()
    {
        // Move to first point
        yield return StartCoroutine(
            MoveCamera(
                firstPoint.position,
                firstPoint.rotation,
                moveToFirstDuration));

        // Play audio
        if (audioSource != null)
        {
            audioSource.volume = 1f;
            audioSource.Play();
        }

        // Wait before audio fade starts
        float waitBeforeFade =
            Mathf.Max(0, waitDuration - audioFadeDuration);

        yield return new WaitForSeconds(waitBeforeFade);

        // Fade audio
        if (audioSource != null && audioSource.isPlaying)
        {
            yield return StartCoroutine(FadeOutAudio());
        }

        // Move to final point
        yield return StartCoroutine(
            MoveCamera(
                finalPoint.position,
                finalPoint.rotation,
                moveToFinalDuration));

        // Change skybox exposure
        yield return StartCoroutine(ChangeExposure());

        // Fade canvases
        yield return StartCoroutine(FadeOutCanvases());

        // Hide objects
        foreach (GameObject obj in objectsToHide)
        {
            if (obj != null)
                obj.SetActive(false);
        }
    }

    IEnumerator MoveCamera(
        Vector3 targetPos,
        Quaternion targetRot,
        float duration)
    {
        Vector3 startPos = transform.position;
        Quaternion startRot = transform.rotation;

        float elapsed = 0f;

        while (elapsed < duration)
        {
            float t = elapsed / duration;
            t = Mathf.SmoothStep(0f, 1f, t);

            transform.position =
                Vector3.Lerp(startPos, targetPos, t);

            transform.rotation =
                Quaternion.Slerp(startRot, targetRot, t);

            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.position = targetPos;
        transform.rotation = targetRot;
    }

    IEnumerator FadeOutAudio()
    {
        float startVolume = audioSource.volume;
        float elapsed = 0f;

        while (elapsed < audioFadeDuration)
        {
            audioSource.volume =
                Mathf.Lerp(
                    startVolume,
                    0f,
                    elapsed / audioFadeDuration);

            elapsed += Time.deltaTime;
            yield return null;
        }

        audioSource.volume = 0f;
        audioSource.Stop();
    }

    IEnumerator ChangeExposure()
    {
        if (skyboxMaterial == null)
            yield break;

        float startExposure =
            skyboxMaterial.GetFloat("_Exposure");

        float elapsed = 0f;

        while (elapsed < exposureTransitionTime)
        {
            float exposure =
                Mathf.Lerp(
                    startExposure,
                    targetExposure,
                    elapsed / exposureTransitionTime);

            skyboxMaterial.SetFloat("_Exposure", exposure);

            DynamicGI.UpdateEnvironment();

            elapsed += Time.deltaTime;
            yield return null;
        }

        skyboxMaterial.SetFloat(
            "_Exposure",
            targetExposure);

        DynamicGI.UpdateEnvironment();
    }

    IEnumerator FadeOutCanvases()
    {
        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            float alpha =
                Mathf.Lerp(
                    1f,
                    0f,
                    elapsed / fadeDuration);

            foreach (CanvasGroup canvas in canvasesToFade)
            {
                if (canvas != null)
                    canvas.alpha = alpha;
            }

            elapsed += Time.deltaTime;
            yield return null;
        }

        foreach (CanvasGroup canvas in canvasesToFade)
        {
            if (canvas != null)
            {
                canvas.alpha = 0f;
                canvas.interactable = false;
                canvas.blocksRaycasts = false;
            }
        }
    }
}