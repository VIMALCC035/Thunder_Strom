using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections.Generic;

public class Newthunder : MonoBehaviour
{
    [Header("Temperature")]
    public Slider temperatureSlider;
    [Range(0f, 1f)]
    public float defaultTemperature = 0.35f;
    public TMP_Text temperatureText;

    [Header("Humidity")]
    public Slider humiditySlider;
    [Range(0f, 1f)]
    public float defaultHumidity = 0f; // ADDED: Default humidity
    public TMP_Text humidityText;

    [Header("Directional Light")]
    public Light directionalLight;

    public float initialDirectionalIntensity = 2f;
    public float stormDirectionalIntensity = 0.1f;

    public Color coldColor = new Color(0.8f, 0.9f, 1f);
    public Color warmColor = new Color(1f, 0.75f, 0.4f);

    [Header("Environment Lighting")]
    public float initialEnvironmentIntensity = 1.5f;
    public float stormEnvironmentIntensity = 0f;

    [Header("Cloud Material")]
    public Material cloudMaterial;

    [Header("Events")]
    public UnityEvent onCloudReverseCompleted;

    public Color dryCloudColor = Color.white;

    public Color stormCloudColor =
        new Color(0.25f, 0.25f, 0.3f, 1f);

    public string baseColorProperty = "_Basecolor";

    [Header("Fog")]
    public float maxFogDensity = 0.4f;

    [Header("Cloud Animation")]
    public Animator[] cloudAnimators;
    public string animationStateName = "CloudFormation";

    [Header("Condition UI")]
    public GameObject correctIndicatorUI;
    public Toggle lockSlidersCheckbox; // ADDED: Checkbox to lock sliders

    [Header("Storm")]
    public AudioSource stormSFX;

    public GameObject[] lightningObjects;
    public float lightningInterval = 0.5f;

    [Header("Rain")]
    public List<ParticleSystem> rainParticles;

    [Header("Transition")]
    public float lightTransitionDuration = 5f;

    // ======================================================
    // 🔔 BLINK SYSTEM (ADDED ONLY)
    // ======================================================
    [Header("Blink UI")]
    public float blinkSpeed = 0.5f;

    private Coroutine blinkRoutine;
    private bool waitingForTrigger;

    private bool conditionsMet;
    private bool stormRunning;

    [Header("Guided Temperature")]

    public float temperatureStep1 = 50f;
    public float temperatureStep2 = 80f;

    public float humidityStep1 = 30f;
    public float humidityStep2 = 60f;
    private bool firstStepReached = false;
    private bool audioPlaying = false;

    [Header("Guided Audio")]
    public AudioSource temperatureAudio;
    public AudioSource humidityAudio;

    [Header("Slider Animation")]
    public float sliderMoveDuration = 2f;

    private bool stage1Completed;
    private bool stage2Completed;

    private bool temp50Reached;
    private bool humidity30Reached;

    private bool temp80Reached;
    private bool humidity60Reached;

    public GameObject video;

    public GameObject Reverseanimtion;
    //public AudioSource fiststaudio;
    [Header("Step Events")]
    public UnityEvent onFirstStepCompleted;
    public UnityEvent onSecondStepCompleted;
    // public AudioSource thankyou;

    //[Header("Completion")]
    //public AudioSource completionAudio;
    //public CanvasGroup completionPanel;
    //public float fadeDuration = 1f;
    private void Start()
    {
        temperatureSlider.minValue = 0;
        temperatureSlider.maxValue = 100;

        humiditySlider.minValue = 0;
        humiditySlider.maxValue = 100;

        temperatureSlider.value = 0;
        humiditySlider.value = 0;

        temperatureSlider.onValueChanged.AddListener(UpdateTemperature);
        humiditySlider.onValueChanged.AddListener(UpdateHumidity);

        if (correctIndicatorUI)
            correctIndicatorUI.SetActive(false);
        UpdateTemperature(0);
        UpdateHumidity(0);
    }

    void CheckConditions()
    {
        conditionsMet =
            temp80Reached &&
            humidity60Reached;
    }
    void PlayRain()
    {
        foreach (ParticleSystem rain in rainParticles)
        {
            if (rain != null)
                rain.Play();
        }
    }
    IEnumerator FadeRainAndStop(float duration)
    {
        float timer = 0f;
        List<float> startRates = new List<float>();

        foreach (ParticleSystem rain in rainParticles)
        {
            if (rain != null)
                startRates.Add(rain.emission.rateOverTime.constant);
        }

        while (timer < duration)
        {
            timer += Time.deltaTime;

            for (int i = 0; i < rainParticles.Count; i++)
            {
                if (rainParticles[i] == null) continue;

                var emission = rainParticles[i].emission;

                emission.rateOverTime =
                    Mathf.Lerp(
                        startRates[i],
                        0f,
                        timer / duration);
            }

            yield return null;
        }

        StopRain();
    }
    void StopRain()
    {
        foreach (ParticleSystem rain in rainParticles)
        {
            if (rain != null)
                rain.Stop();
        }
    }
    void ResetRain()
    {
        foreach (ParticleSystem rain in rainParticles)
        {
            if (rain == null) continue;

            rain.Stop(
                true,
                ParticleSystemStopBehavior.StopEmittingAndClear);

            var emission = rain.emission;
            emission.rateOverTime =
                new ParticleSystem.MinMaxCurve(0f);
        }
    }
    IEnumerator GuidedSequence()
    {
        Debug.Log("Guided Sequence Started");

        yield return StartCoroutine(
            AnimateSlider(
                temperatureSlider,
                0,
                temperatureStep1));

        Debug.Log("Temperature Reached 50");

        yield return StartCoroutine(
            AnimateSlider(
                humiditySlider,
                0,
                humidityStep1));

        Debug.Log("Humidity Reached 30");

        if (temperatureAudio != null)
        {
            Debug.Log("Playing Temperature Audio");

            temperatureAudio.Play();

            yield return new WaitWhile(
                () => temperatureAudio.isPlaying);
        }

        if (humidityAudio != null)
        {
            Debug.Log("Playing Humidity Audio");

            humidityAudio.Play();

            yield return new WaitWhile(
                () => humidityAudio.isPlaying);
        }

        temperatureSlider.interactable = true;
        humiditySlider.interactable = true;

        Debug.Log("Sliders Unlocked");
    }
    void CheckFirstStep()
    {
        if (stage1Completed)
            return;

        if (!temp50Reached &&
            temperatureSlider.value >= temperatureStep1)
        {
            temp50Reached = true;

            temperatureSlider.value = temperatureStep1;
            temperatureSlider.interactable = false;
        }

        if (!humidity30Reached &&
            humiditySlider.value >= humidityStep1)
        {
            humidity30Reached = true;

            humiditySlider.value = humidityStep1;
            humiditySlider.interactable = false;
        }

        // When Temp = 50 and Humidity = 30
        if (temp50Reached && humidity30Reached)
        {
            stage1Completed = true;

            onFirstStepCompleted?.Invoke(); // EVENT CALL

            StartCoroutine(Stage1Audio());
        }
    }
    IEnumerator Stage1Audio()
    {
        if (temperatureAudio != null)
        {
            temperatureAudio.Play();

            yield return new WaitWhile(
                () => temperatureAudio.isPlaying);
        }

        temperatureSlider.interactable = true;
        humiditySlider.interactable = true;
    }
    void CheckSecondStep()
    {
        if (!stage1Completed || stage2Completed)
            return;

        if (!temp80Reached &&
            temperatureSlider.value >= temperatureStep2)
        {
            temp80Reached = true;

            temperatureSlider.value = temperatureStep2;
            temperatureSlider.interactable = false;
        }

        if (!humidity60Reached &&
            humiditySlider.value >= humidityStep2)
        {
            humidity60Reached = true;

            humiditySlider.value = humidityStep2;
            humiditySlider.interactable = false;
        }

        // When Temp = 80 and Humidity = 60
        if (temp80Reached && humidity60Reached)
        {
            stage2Completed = true;

            conditionsMet = true;

            onSecondStepCompleted?.Invoke(); // EVENT CALL

            StartCoroutine(Stage2Audio());
        }
    }
    IEnumerator Stage2Audio()
    {
        if (humidityAudio != null)
        {
            humidityAudio.Play();

            yield return new WaitWhile(
                () => humidityAudio.isPlaying);
        }

        //// Play third audio
        //if (completionAudio != null)
        //{
        //    completionAudio.Play();
        //}

        //// Fade in UI
        //if (completionPanel != null)
        //{
        //    yield return StartCoroutine(
        //        FadeCanvasGroup(
        //            completionPanel,
        //            0f,
        //            1f,
        //            fadeDuration));
        //}

        if (correctIndicatorUI != null)
        {
            correctIndicatorUI.SetActive(true);
        }

        Debug.Log("Stage 2 Completed");
    }
    IEnumerator FadeCanvasGroup(
    CanvasGroup canvasGroup,
    float startAlpha,
    float endAlpha,
    float duration)
    {
        float timer = 0f;

        canvasGroup.alpha = startAlpha;

        while (timer < duration)
        {
            timer += Time.deltaTime;

            canvasGroup.alpha =
                Mathf.Lerp(
                    startAlpha,
                    endAlpha,
                    timer / duration);

            yield return null;
        }

        canvasGroup.alpha = endAlpha;
    }
    IEnumerator PlayGuidedAudio()
    {
        audioPlaying = true;

        temperatureSlider.interactable = false;
        humiditySlider.interactable = false;

        if (temperatureAudio != null)
        {
            temperatureAudio.Play();

            yield return new WaitWhile(
                () => temperatureAudio.isPlaying);
        }

        if (humidityAudio != null)
        {
            humidityAudio.Play();

            yield return new WaitWhile(
                () => humidityAudio.isPlaying);
        }

        temperatureSlider.interactable = true;
        humiditySlider.interactable = true;

        audioPlaying = false;
    }
    IEnumerator AnimateSlider(
      Slider slider,
      float startValue,
      float endValue)
    {
        Debug.Log("Animating " + slider.name);

        float timer = 0f;

        while (timer < sliderMoveDuration)
        {
            timer += Time.deltaTime;

            slider.value = Mathf.Lerp(
                startValue,
                endValue,
                timer / sliderMoveDuration);

            Debug.Log(slider.name + " = " + slider.value);

            yield return null;
        }

        slider.value = endValue;

        Debug.Log(slider.name + " Finished");
    }
    // =========================================================
    // TEMPERATURE (UNCHANGED)
    // =========================================================
    void UpdateTemperature(float value)
    {
        int percentage = Mathf.RoundToInt(value);

        if (temperatureText)
        {
            temperatureText.text =
                "Temperature : " + percentage + "%";
        }

        float normalizedValue = value / 100f;

        directionalLight.color =
            Color.Lerp(
                coldColor,
                warmColor,
                normalizedValue);

        RenderSettings.ambientLight = Color.Lerp(
            new Color(0.75f, 0.85f, 1f),
            new Color(1f, 0.85f, 0.65f),
            normalizedValue * 0.4f
        );

        if (cloudAnimators != null)
        {
            foreach (Animator anim in cloudAnimators)
            {
                if (anim == null)
                    continue;

                anim.Play(
                    animationStateName,
                    0,
                    normalizedValue);

                anim.speed = 0;
            }
        }
        CheckFirstStep();
        CheckSecondStep();
    }
    // =========================================================
    // HUMIDITY (UNCHANGED)
    // =========================================================
    void UpdateHumidity(float value)
    {
        int percentage = Mathf.RoundToInt(value);

        if (humidityText)
        {
            humidityText.text =
                "Humidity : " + percentage + "%";
        }

        float normalizedValue = value / 100f;

        RenderSettings.fogDensity =
            normalizedValue * maxFogDensity;

        if (cloudMaterial)
        {
            Color currentColor =
                Color.Lerp(
                    dryCloudColor,
                    stormCloudColor,
                    normalizedValue);

            cloudMaterial.SetColor(
                baseColorProperty,
                currentColor);
        }

        RenderSettings.ambientLight = Color.Lerp(
            RenderSettings.ambientLight,
            new Color(0.7f, 0.8f, 1f),
            normalizedValue * 0.25f
        );
        CheckFirstStep();
        CheckSecondStep();
    }
    // =========================================================
    // CHECK CONDITIONS (ONLY BLINK ADDED)
    // =========================================================
    //void CheckConditions()
    //{
    //    bool tempCorrect =
    //        temperatureSlider.value >= temperatureStep2;

    //    bool humidityCorrect =
    //        humiditySlider.value >= humidityStep2;

    //    conditionsMet =
    //        tempCorrect && humidityCorrect;

    //    if (correctIndicatorUI)
    //    {
    //        if (conditionsMet && !stormRunning)
    //        {
    //            if (!waitingForTrigger)
    //            {
    //                waitingForTrigger = true;

    //                if (blinkRoutine != null)
    //                    StopCoroutine(blinkRoutine);

    //                blinkRoutine =
    //                    StartCoroutine(BlinkIndicator());
    //            }
    //        }
    //        else
    //        {
    //            waitingForTrigger = false;

    //            if (blinkRoutine != null)
    //            {
    //                StopCoroutine(blinkRoutine);
    //                blinkRoutine = null;
    //            }

    //            correctIndicatorUI.SetActive(false);
    //        }
    //    }
    //}

    // =========================================================
    // BLINK ROUTINE (ADDED ONLY)
    // =========================================================
    IEnumerator BlinkIndicator()
    {
        while (waitingForTrigger)
        {
            if (correctIndicatorUI)
                correctIndicatorUI.SetActive(
                    !correctIndicatorUI.activeSelf);

            yield return new WaitForSeconds(blinkSpeed);
        }

        if (correctIndicatorUI)
            correctIndicatorUI.SetActive(true);
    }

    // =========================================================
    // TRIGGER STORM (BLINK STOP ADDED ONLY)
    // =========================================================
    public void TriggerStorm()
    {
        // fiststaudio.Play();


        waitingForTrigger = false;

        if (blinkRoutine != null)
        {
            StopCoroutine(blinkRoutine);
            blinkRoutine = null;
        }

        if (stormRunning)
            return;

        if (!conditionsMet)
        {
            Debug.Log("Conditions not met.");
            return;
        }

        StartCoroutine(StormSequence());
    }

    // ADDED: Listener function for the UI Checkbox
    void OnLockCheckboxChanged(bool isChecked)
    {
        LockSliders(stormRunning);
    }

    // MODIFIED: Takes both Storm state and Checkbox state into account
    void LockSliders(bool state)
    {
        bool isCheckboxLocked = lockSlidersCheckbox != null && lockSlidersCheckbox.isOn;
        temperatureSlider.interactable = !(state || isCheckboxLocked);
        humiditySlider.interactable = !(state || isCheckboxLocked);
    }

    // =========================================================
    // REST OF YOUR SCRIPT (UNCHANGED)
    // =========================================================
    IEnumerator StormSequence()
    {
        stormRunning = true;

        LockSliders(true);

        if (correctIndicatorUI)
            correctIndicatorUI.GetComponent<Image>().enabled = false;

        StartCoroutine(
            AnimateLighting(
                stormDirectionalIntensity,
                stormEnvironmentIntensity,
                lightTransitionDuration));

        yield return new WaitForSeconds(1f);

        if (stormSFX)
            stormSFX.Play();

        yield return new WaitForSeconds(4f);

        yield return StartCoroutine(PlayLightningSequence());

        // Start all rain particles
        PlayRain();

        float timer = 0f;
        List<float> startRates = new List<float>();

        // Store initial emission rates
        foreach (ParticleSystem rain in rainParticles)
        {
            if (rain == null) continue;

            startRates.Add(
                rain.emission.rateOverTime.constant);
        }

        // Fade all rain particles
        while (timer < 10f)
        {
            timer += Time.deltaTime;

            for (int i = 0; i < rainParticles.Count; i++)
            {
                if (rainParticles[i] == null) continue;

                var emission =
                    rainParticles[i].emission;

                emission.rateOverTime =
                    Mathf.Lerp(
                        startRates[i],
                        0f,
                        timer / 6f);
            }

            yield return null;
        }

        // Stop all rain


        yield return new WaitForSeconds(3f);

        yield return StartCoroutine(ReverseCloudAnimation());

        yield return StartCoroutine(
            AnimateLighting(
                initialDirectionalIntensity,
                initialEnvironmentIntensity,
                lightTransitionDuration));

        yield return StartCoroutine(ResetSimulation());
        StopRain();
        // thankyou.Play();
        LockSliders(false);

        stormRunning = false;

    }
    IEnumerator PlayLightningSequence()
    {
        if (lightningObjects == null ||
            lightningObjects.Length == 0)
            yield break;

        foreach (GameObject lightning in lightningObjects)
        {
            if (lightning == null)
                continue;

            lightning.SetActive(true);
            yield return new WaitForSeconds(lightningInterval);
            lightning.SetActive(false);
        }
    }

    IEnumerator ReverseCloudAnimation()
    {
        Reverseanimtion.gameObject.SetActive(true);
        StartCoroutine(FadeVolume(stormSFX, 0f, 3f));
        float t = 1f;

        while (t > 0f)
        {
            t -= Time.deltaTime / 4f;

            if (cloudAnimators != null)
            {
                foreach (Animator anim in cloudAnimators)
                {
                    if (anim == null)
                        continue;

                    anim.Play(animationStateName, 0, Mathf.Clamp01(t));
                    anim.speed = 0;
                }
            }

            yield return null;
        }

        // Event fires when reverse animation is fully completed
        onCloudReverseCompleted?.Invoke();
    }
    public IEnumerator FadeVolume(AudioSource stormSFX, float targetVolume, float duration)
    {
        float startVolume = stormSFX.volume;

        float timer = 0f;

        while (timer < duration)
        {
            timer += Time.deltaTime;

            stormSFX.volume = Mathf.Lerp(
                startVolume,
                targetVolume,
                timer / duration);

            yield return null;
        }

        stormSFX.volume = targetVolume;
    }
    IEnumerator AnimateLighting(
        float targetDirectionalIntensity,
        float targetEnvironmentIntensity,
        float duration)
    {
        float startDirectional = directionalLight.intensity;
        float startEnvironment = RenderSettings.ambientIntensity;

        float timer = 0f;

        while (timer < duration)
        {
            timer += Time.deltaTime;

            float t = timer / duration;

            directionalLight.intensity =
                Mathf.Lerp(startDirectional, targetDirectionalIntensity, t);

            RenderSettings.ambientIntensity =
                Mathf.Lerp(startEnvironment, targetEnvironmentIntensity, t);

            yield return null;
        }

        directionalLight.intensity = targetDirectionalIntensity;
        RenderSettings.ambientIntensity = targetEnvironmentIntensity;
    }

    IEnumerator ResetSimulation()
    {
        float duration = 3f;

        float startTemp = temperatureSlider.value;
        float startHumidity = humiditySlider.value;

        float timer = 0f;

        while (timer < duration)
        {
            timer += Time.deltaTime;

            float t = timer / duration;

            temperatureSlider.value =
                Mathf.Lerp(startTemp, defaultTemperature, t);

            humiditySlider.value =
                Mathf.Lerp(startHumidity, defaultHumidity, t); // MODIFIED: Uses defaultHumidity

            yield return null;
        }

        temperatureSlider.value = defaultTemperature;
        humiditySlider.value = defaultHumidity; // MODIFIED: Uses defaultHumidity

        RenderSettings.fogDensity = 0f;

        if (cloudMaterial)
            cloudMaterial.SetColor(baseColorProperty, dryCloudColor);

        conditionsMet = false;

        if (correctIndicatorUI)
            correctIndicatorUI.SetActive(false);
    }

    public void ForceResetToInitialState()
    {
        StopAllCoroutines();

        // Reset flags
        stormRunning = false;
        conditionsMet = false;
        waitingForTrigger = false;

        // Stop blink
        if (blinkRoutine != null)
        {
            StopCoroutine(blinkRoutine);
            blinkRoutine = null;
        }

        // Unlock sliders
        if (lockSlidersCheckbox != null)
            lockSlidersCheckbox.isOn = false;

        LockSliders(false);

        // Reset slider values
        temperatureSlider.value = defaultTemperature;
        humiditySlider.value = defaultHumidity;

        // Reset text
        if (temperatureText)
        {
            temperatureText.text =
                "Temperature : " +
                Mathf.RoundToInt(defaultTemperature * 100f) +
                "%";
        }

        if (humidityText)
        {
            humidityText.text =
                "Humidity : " +
                Mathf.RoundToInt(defaultHumidity * 100f) +
                "%";
        }

        // Reset directional light
        if (directionalLight)
        {
            directionalLight.intensity =
                initialDirectionalIntensity;

            directionalLight.color =
                Color.Lerp(
                    coldColor,
                    warmColor,
                    defaultTemperature);
        }

        // Reset environment lighting
        RenderSettings.ambientIntensity =
            initialEnvironmentIntensity;

        RenderSettings.ambientLight =
            new Color(
                0.75f,
                0.85f,
                1f);

        // Reset fog
        RenderSettings.fog = true;
        RenderSettings.fogDensity = 0f;

        // Reset cloud material
        if (cloudMaterial)
        {
            cloudMaterial.SetColor(
                baseColorProperty,
                dryCloudColor);
        }

        // Reset cloud animation
        if (cloudAnimators != null)
        {
            foreach (Animator anim in cloudAnimators)
            {
                if (anim == null)
                    continue;

                anim.Play(
                    animationStateName,
                    0,
                    defaultTemperature);

                anim.speed = 0;
            }
        }

        // Stop storm audio
        if (stormSFX)
        {
            stormSFX.Stop();
        }

        // Disable lightning
        if (lightningObjects != null)
        {
            foreach (GameObject lightning in lightningObjects)
            {
                if (lightning != null)
                    lightning.SetActive(false);
            }
        }

        // Stop all rain particles
        if (rainParticles != null)
        {
            foreach (ParticleSystem rain in rainParticles)
            {
                if (rain == null)
                    continue;

                rain.Stop(
                    true,
                    ParticleSystemStopBehavior.StopEmittingAndClear);

                var emission = rain.emission;

                emission.rateOverTime =
                    new ParticleSystem.MinMaxCurve(0f);
            }
        }

        // Hide indicator UI
        if (correctIndicatorUI)
        {
            correctIndicatorUI.SetActive(false);
        }

        // Refresh everything
        UpdateTemperature(defaultTemperature);
        UpdateHumidity(defaultHumidity);
    }
}