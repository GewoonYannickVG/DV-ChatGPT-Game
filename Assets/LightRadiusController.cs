using UnityEngine;
using UnityEngine.Rendering.Universal;

public class LightRadiusController : MonoBehaviour
{
    [Header("Light Settings")]
    [SerializeField] private Light2D light2D;                     // Reference to the Light2D component
    [SerializeField] private float maxRadius = 10f;               // Maximum outer radius
    [SerializeField] private float minRadius = 5f;                // Minimum outer radius
    [SerializeField] private float radiusChangeSpeed = 2f;        // Speed of radius change

    [Header("Intensity Settings")]
    [SerializeField] private float normalIntensity = 1f;          // Normal intensity of the light
    [SerializeField] private float reducedIntensity = 0.5f;       // Reduced intensity when Shift is held down
    [SerializeField] private float intensityChangeSpeedHold = 2f; // Speed of intensity change when holding Shift
    [SerializeField] private float intensityChangeSpeedRelease = 2f; // Speed of intensity change when releasing Shift

    private float targetRadius;                                    // The target radius to smoothly transition to
    private float targetIntensity;                                 // The target intensity to smoothly transition to

    void Start()
    {
        // Initialize target values with the current light properties
        if (light2D != null)
        {
            targetRadius = light2D.pointLightOuterRadius;
            targetIntensity = normalIntensity;
            light2D.intensity = targetIntensity;
        }
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
            float intensitySpeed = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)
                ? intensityChangeSpeedHold
                : intensityChangeSpeedRelease;

            // Smoothly transition to the target intensity
            light2D.intensity = Mathf.Lerp(light2D.intensity, targetIntensity, Time.deltaTime * intensitySpeed);
        }
    }
}
