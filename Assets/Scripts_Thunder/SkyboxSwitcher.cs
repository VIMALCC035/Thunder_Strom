using UnityEngine;

public class SkyboxSwitcher : MonoBehaviour
{
    [Header("Target")]
    public GameObject targetObject;

    [Header("Skyboxes")]
    public Material activeSkybox;

    public Material originalSkybox;
    private bool wasActive;

    private void Start()
    {
        originalSkybox = RenderSettings.skybox;

        if (targetObject != null)
            wasActive = targetObject.activeSelf;
    }

    private void Update()
    {
        if (targetObject == null)
            return;

        if (targetObject.activeSelf != wasActive)
        {
            wasActive = targetObject.activeSelf;

            if (wasActive)
            {
                RenderSettings.skybox = activeSkybox;
            }
            else
            {
                RenderSettings.skybox = originalSkybox;
            }

            DynamicGI.UpdateEnvironment();
        }
    }
}