using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AtmosphereConditionController : MonoBehaviour
{
    [Header("Temperature")]
    public Slider temperatureSlider;

    [Range(0f, 1f)]
    public float defaultTemperature = 0.35f;

    public TMP_Text temperatureText;

    [Header("Humidity")]
    public Slider humiditySlider;

    public TMP_Text humidityText;

    [Header("Lighting")]
    public Light directionalLight;

    [Header("Current UI To Hide")]
    public CanvasGroup currentCanvas;

    [Header("Voice Over")]
    public AudioSource voiceOverSource;

    public AudioClip successVoiceOver;

    [Header("Success Canvases")]
    public CanvasGroup[] successCanvases;

    [Header("Fade Settings")]
    public float fadeDuration = 1.5f;

    private bool conditionsMet;
    private bool locked;

    private void Start()
    {
        InitializeController();
    }

    void InitializeController()
    {
        temperatureSlider.value =
            defaultTemperature;

        humiditySlider.value = 0f;

        temperatureSlider.onValueChanged
            .AddListener(HandleTemperatureUpdate);

        humiditySlider.onValueChanged
            .AddListener(HandleHumidityUpdate);

        if (currentCanvas)
        {
            currentCanvas.alpha = 1f;
            currentCanvas.gameObject.SetActive(false);
        }

        foreach (CanvasGroup canvas in successCanvases)
        {
            if (canvas)
            {
                canvas.alpha = 0f;
                canvas.gameObject.SetActive(false);
            }
        }

        HandleTemperatureUpdate(
            temperatureSlider.value);

        HandleHumidityUpdate(
            humiditySlider.value);
    }

    void HandleTemperatureUpdate(float value)
    {
        if (temperatureText)
        {
            temperatureText.text =
                "Temperature : " +
                Mathf.RoundToInt(value * 100f) +
                "%";
        }

        Color coldTone =
            new Color(
                0.55f,
                0.75f,
                1.0f);

        Color warmTone =
            new Color(
                1.0f,
                0.72f,
                0.45f);

        if (directionalLight)
        {
            directionalLight.color =
                Color.Lerp(
                    coldTone,
                    warmTone,
                    value);
        }

        UnityEngine.RenderSettings.ambientLight =
            Color.Lerp(
                new Color(
                    0.6f,
                    0.75f,
                    1f),
                new Color(
                    1f,
                    0.7f,
                    0.6f),
                value);

        EvaluateAtmosphereState();
    }

    void HandleHumidityUpdate(float value)
    {
        if (humidityText)
        {
            humidityText.text =
                "Humidity : " +
                Mathf.RoundToInt(value * 100f) +
                "%";
        }

        EvaluateAtmosphereState();
    }

    void EvaluateAtmosphereState()
    {
        if (locked)
            return;

        bool tempCorrect =
            temperatureSlider.value >= 0.80f;

        bool humidityCorrect =
            humiditySlider.value >= 0.60f;

        conditionsMet =
            tempCorrect &&
            humidityCorrect;

        if (conditionsMet)
        {
            locked = true;

            temperatureSlider.interactable = false;
            humiditySlider.interactable = false;

            StartCoroutine(
                ConditionReachedSequence());
        }
    }

    IEnumerator ConditionReachedSequence()
    {
        // Fade out current UI
        if (currentCanvas)
        {
            float timer = 0f;

            while (timer < fadeDuration)
            {
                timer += Time.deltaTime;

                currentCanvas.alpha =
                    Mathf.Lerp(
                        1f,
                        0f,
                        timer / fadeDuration);

                yield return null;
            }

            currentCanvas.alpha = 0f;
            currentCanvas.gameObject.SetActive(false);
        }

        // Play voice over
        if (voiceOverSource &&
            successVoiceOver)
        {
            voiceOverSource.clip =
                successVoiceOver;

            voiceOverSource.Play();
        }

        // Show all canvases while audio plays
        yield return StartCoroutine(
            PlaySuccessCanvasSequence());
    }

    IEnumerator PlaySuccessCanvasSequence()
    {
        foreach (CanvasGroup canvas in successCanvases)
        {
            if (!canvas)
                continue;

            canvas.alpha = 0f;
            canvas.gameObject.SetActive(true);
        }

        float timer = 0f;

        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;

            float t =
                timer / fadeDuration;

            foreach (CanvasGroup canvas in successCanvases)
            {
                if (canvas)
                {
                    canvas.alpha =
                        Mathf.Lerp(
                            0f,
                            1f,
                            t);
                }
            }

            yield return null;
        }

        foreach (CanvasGroup canvas in successCanvases)
        {
            if (canvas)
                canvas.alpha = 1f;
        }
    }

    // Optional reset function
    public void ResetAtmosphere()
    {
        StopAllCoroutines();

        locked = false;
        conditionsMet = false;

        temperatureSlider.interactable = true;
        humiditySlider.interactable = true;

        temperatureSlider.value =
            defaultTemperature;

        humiditySlider.value = 0f;

        if (voiceOverSource)
            voiceOverSource.Stop();

        if (currentCanvas)
        {
            currentCanvas.alpha = 1f;
            currentCanvas.gameObject.SetActive(true);
        }

        foreach (CanvasGroup canvas in successCanvases)
        {
            if (canvas)
            {
                canvas.alpha = 0f;
                canvas.gameObject.SetActive(false);
            }
        }

        HandleTemperatureUpdate(
            defaultTemperature);

        HandleHumidityUpdate(0f);
    }
}