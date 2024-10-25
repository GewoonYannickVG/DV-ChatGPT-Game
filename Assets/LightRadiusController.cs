using UnityEngine;
using UnityEngine.Rendering.Universal;
using System.Collections; // Ensure this is included for IEnumerator

public class LightRadiusController : MonoBehaviour
{
    private static LightRadiusController instance;

    [Header("Light Settings")]
    [SerializeField] private Light2D light2D;                     // Reference to the Light2D component
    [SerializeField] private float maxRadius = 10f;               // Maximum outer radius
    [SerializeField] private float minRadius = 5f;                // Minimum outer radius
    [SerializeField] private float radiusChangeSpeed = 2f;        // Speed of radius change

    [Header("Intensity Settings")]
    [SerializeField] private float initialIntensity = 0f;         // Initial intensity of the light (editable in Unity Editor)
    [SerializeField] private float normalIntensity = 1f;          // Normal intensity of the light (editable in Unity Editor)
    [SerializeField] private float transitionDuration = 2f;       // Duration for the intensity transition (editable in Unity Editor)
    [SerializeField] private float reducedIntensity = 0.5f;       // Reduced intensity when Shift is held down
    [SerializeField] private float intensityChangeSpeedHold = 2f; // Speed of intensity change when holding Shift
    [SerializeField] private float intensityChangeSpeedRelease = 2f; // Speed of intensity change when releasing Shift

    private float targetRadius;                                    // The target radius to smoothly transition to
    private float targetIntensity;                                 // The target intensity to smoothly transition to

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);  // Destroy duplicate LightRadiusController
        }
    }

    void Start()
    {
        // Initialize target values with the current light properties
        if (light2D != null)
        {
            targetRadius = light2D.pointLightOuterRadius;
            targetIntensity = normalIntensity;  // Set targetIntensity initially to normalIntensity
            light2D.intensity = initialIntensity; // Set the initial intensity from the serialized field
            StartCoroutine(FadeInLight(transitionDuration)); // Use transitionDuration for the fade-in
        }
    }

    private IEnumerator FadeInLight(float duration)
    {
        float elapsedTime = 0f;
        float initialIntensity = light2D.intensity; // Start from the current intensity

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            // Calculate the new intensity using Lerp for a smooth transition
            light2D.intensity = Mathf.Lerp(initialIntensity, normalIntensity, elapsedTime / duration);
            yield return null; // Wait until the next frame
        }

        // Ensure the intensity is set to the target at the end
        light2D.intensity = normalIntensity;
    }

    void Update()
    {
        // Check if the Shift key is being held down
        if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
        {
            // Increase the target radius towards maxRadius
            targetRadius = maxRadius;
            // Decrease the target intensity towards reducedIntensity
            targetIntensity = reducedIntensity;
        }
        else
        {
            // Decrease the target radius back to minRadius
            targetRadius = minRadius;
            // Restore the target intensity back to normalIntensity
            targetIntensity = normalIntensity;
        }

        // Update the Light2D's outer radius and intensity using Lerp for smooth transitions
        if (light2D != null)
        {
            // Smoothly transition to the target radius
            light2D.pointLightOuterRadius = Mathf.Lerp(light2D.pointLightOuterRadius, targetRadius, Time.deltaTime * radiusChangeSpeed);

            // Check for the Shift key state to apply different intensity change speeds
            float intensitySpeed = (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
                ? intensityChangeSpeedHold
                : intensityChangeSpeedRelease;

            // Smoothly transition to the target intensity
            light2D.intensity = Mathf.Lerp(light2D.intensity, targetIntensity, Time.deltaTime * intensitySpeed);
        }
    }

    public IEnumerator FadeOutLights()
    {
        float duration = 2f; // Duration of the fadeout
        float startIntensity = light2D.intensity;

        for (float t = 0; t < duration; t += Time.deltaTime)
        {
            light2D.intensity = Mathf.Lerp(startIntensity, 0, t / duration);
            yield return null;
        }

        light2D.intensity = 0;
    }
}