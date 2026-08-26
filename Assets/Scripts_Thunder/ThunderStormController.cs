using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ThunderStormController : MonoBehaviour
{
    [Header("Temperature")] public Slider temperatureSlider; [Range(0f, 1f)] public float defaultTemperature = 0.35f; public TMP_Text temperatureText;

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
    public ParticleSystem rainParticle;

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
    [Header("Skybox")]
    public SkyboxExposureController skyboxExposureController;
    private void Start()
    {
        RenderSettings.fog = true;

        RenderSettings.ambientIntensity =
            initialEnvironmentIntensity;

        directionalLight.intensity =
            initialDirectionalIntensity;

        temperatureSlider.value =
            defaultTemperature;

       // humiditySlider.value = defaultHumidity; // MODIFIED: Uses defaultHumidity

        if (correctIndicatorUI)
            correctIndicatorUI.SetActive(false);

        // ADDED: Hook up the lock checkbox
        if (lockSlidersCheckbox != null)
        {
            lockSlidersCheckbox.onValueChanged.AddListener(OnLockCheckboxChanged);
            OnLockCheckboxChanged(lockSlidersCheckbox.isOn);
        }

        if (lightningObjects != null)
        {
            foreach (GameObject lightning in lightningObjects)
            {
                if (lightning)
                    lightning.SetActive(false);
            }
        }

        if (rainParticle)
        {
            rainParticle.Stop(
                true,
                ParticleSystemStopBehavior.StopEmittingAndClear);
        }

        temperatureSlider.onValueChanged
            .AddListener(UpdateTemperature);

        humiditySlider.onValueChanged
            .AddListener(UpdateHumidity);

        UpdateTemperature(temperatureSlider.value);
       // UpdateHumidity(humiditySlider.value);
    }

    // =========================================================
    // TEMPERATURE (UNCHANGED)
    // =========================================================
    void UpdateTemperature(float value)
    {
        int percentage =
            Mathf.RoundToInt(value * 100f);

        if (temperatureText)
        {
            temperatureText.text =
                "Temperature : " +
                percentage + "%";
        }

        directionalLight.color =
            Color.Lerp(
                coldColor,
                warmColor,
                value);

        RenderSettings.ambientLight = Color.Lerp(
            new Color(0.75f, 0.85f, 1f),
            new Color(1f, 0.85f, 0.65f),
            value * 0.4f
        );

        if (cloudAnimators != null)
        {
            foreach (Animator anim in cloudAnimators)
            {
                if (anim == null)
                    continue;

                anim.Play(animationStateName, 0, value);
                anim.speed = 0;
            }
        }

        CheckConditions();
    }

    // =========================================================
    // HUMIDITY (UNCHANGED)
    // =========================================================
    void UpdateHumidity(float value)
    {

        int percentage =
            Mathf.RoundToInt(value * 100f);

        if (humidityText)
        {
            humidityText.text =
                "Humidity : " +
                percentage + "%";
            if (skyboxExposureController != null)
            {
                skyboxExposureController.UpdateExposure(percentage);
            }
            if (skyboxExposureController != null)
            {
                skyboxExposureController.UpdateExposure(value);
            }
        }

        RenderSettings.fogDensity =
            Mathf.Clamp01(value) * maxFogDensity;

        if (cloudMaterial)
        {
            Color currentColor =
                Color.Lerp(
                    dryCloudColor,
                    stormCloudColor,
                    value);

            cloudMaterial.SetColor(
                baseColorProperty,
                currentColor);
        }

        RenderSettings.ambientLight = Color.Lerp(
            RenderSettings.ambientLight,
            new Color(0.7f, 0.8f, 1f),
            value * 0.25f
        );

        CheckConditions();
    }

    // =========================================================
    // CHECK CONDITIONS (ONLY BLINK ADDED)
    // =========================================================
    void CheckConditions()
    {
        bool tempCorrect =
            temperatureSlider.value >= 0.80f;

        bool humidityCorrect =
            humiditySlider.value >= 0.60f &&
            humiditySlider.value <= 0.65f;

        conditionsMet =
            tempCorrect &&
            humidityCorrect;

        if (correctIndicatorUI)
        {
            if (conditionsMet && !stormRunning)
            {
                if (!waitingForTrigger)
                {
                    waitingForTrigger = true;

                    if (blinkRoutine != null)
                        StopCoroutine(blinkRoutine);

                    blinkRoutine =
                        StartCoroutine(BlinkIndicator());
                }
            }
            else
            {
                waitingForTrigger = false;

                if (blinkRoutine != null)
                {
                    StopCoroutine(blinkRoutine);
                    blinkRoutine = null;
                }

                correctIndicatorUI.SetActive(false);
            }
        }
    }

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
            correctIndicatorUI.SetActive(false);

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

        if (rainParticle)
            rainParticle.Play();

        var emission = rainParticle.emission;

        float startRate = emission.rateOverTime.constant;
        float timer = 0f;

        while (timer < 15f)
        {
            timer += Time.deltaTime;
            emission.rateOverTime =
                Mathf.Lerp(startRate, 0f, timer / 15f);

            yield return null;
        }

        rainParticle.Stop();

        yield return new WaitForSeconds(2f);

        yield return StartCoroutine(ReverseCloudAnimation());

        yield return StartCoroutine(
            AnimateLighting(
                initialDirectionalIntensity,
                initialEnvironmentIntensity,
                lightTransitionDuration));

        yield return StartCoroutine(ResetSimulation());

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

        // Unlock sliders (and uncheck the UI box)
        if (lockSlidersCheckbox != null)
            lockSlidersCheckbox.isOn = false; // MODIFIED: Also resets the checkbox

        LockSliders(false);

        // Reset slider values
        temperatureSlider.value = defaultTemperature;
        humiditySlider.value = defaultHumidity; // MODIFIED: Uses defaultHumidity

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
                "Humidity : " + Mathf.RoundToInt(defaultHumidity * 100f) + "%"; // MODIFIED: Uses defaultHumidity
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
                if (lightning)
                {
                    lightning.SetActive(false);
                }
            }
        }

        // Stop rain
        if (rainParticle)
        {
            rainParticle.Stop(
                true,
                ParticleSystemStopBehavior.StopEmittingAndClear);

            var emission =
                rainParticle.emission;

            emission.rateOverTime =
                new ParticleSystem.MinMaxCurve(0f);
        }

        // Hide indicator UI
        if (correctIndicatorUI)
        {
            correctIndicatorUI.SetActive(false);
        }

        // Refresh everything
        UpdateTemperature(
            defaultTemperature);

        UpdateHumidity(
            defaultHumidity); // MODIFIED: Uses defaultHumidity
    }

}