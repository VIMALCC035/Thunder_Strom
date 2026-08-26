using UnityEngine;
using UnityEngine.UI;

public class SkyboxExposureController : MonoBehaviour
{
    public Material skyboxMaterial;
    public Slider humiditySlider;

    [Header("Humidity Range")]
    public float minHumidity = 0f;
    public float maxHumidity = 1f;

    [Header("Exposure Range")]
    public float minExposure = 0f;
    public float maxExposure = 1f;

    private void Start()
    {
      //  humiditySlider.onValueChanged.AddListener(UpdateExposure);
    }

    public void UpdateExposure(float sliderValue)
    {
        if (skyboxMaterial == null)
            return;

        float normalizedHumidity = Mathf.InverseLerp(
            minHumidity,
            maxHumidity,
            sliderValue
        );

        // Reverse mapping
        float exposure = Mathf.Lerp(
            maxExposure,
            minExposure,
            normalizedHumidity
        );

        skyboxMaterial.SetFloat("_Exposure", exposure);

        DynamicGI.UpdateEnvironment();
    }
}