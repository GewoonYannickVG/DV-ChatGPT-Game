using UnityEngine;
using UnityEngine.Experimental.Rendering.Universal;
using System.Collections;
using UnityEngine.Rendering.Universal;

public class DiamondController : MonoBehaviour
{
    public Transform hexagonTransform;    // Reference to the hexagon's transform
    public Transform diamondLightObject;  // Reference to the Light2D object inside the diamond (main light)
    public Transform spotLightObject;     // Reference to the Light2D SpotLight object
    public float floatSpeed = 1f;         // Speed of floating animation
    public float floatHeight = 0.5f;      // Height of floating
    public float flickerSpeed = 0.1f;     // Speed of light flicker
    public float proximityFlickerSpeed = 0.05f; // Flicker speed when player is near
    public float proximityScale = 1.2f;   // Scale increase when player is near
    public float flickerIntensityChange = 0.5f;  // How much the intensity changes when flickering
    public float spotlightFlickerIntensityChange = 0.1f; // Smaller intensity change for spotlight
    public float maxScaleDistance = 5f;   // Maximum distance for scaling effect

    private Light2D diamondLight;         // Reference to the Light2D component (main light)
    private Light2D spotLight;            // Reference to the Light2D component (spotlight)
    private Vector3 initialPosition;      // Original position of the diamond
    private Vector3 initialScale;         // Original scale of the diamond
    private Color initialLightColor;      // Original color of the diamond light
    private Color initialSpotColor = Color.white; // Default white color for the spotlight
    private float initialIntensity;       // Original intensity of the diamond light
    private float initialSpotIntensity = 0.26f;   // Default intensity for the spotlight
    private float initialRadiusInner = 0f;        // Default inner radius for the spotlight
    private float initialRadiusOuter = 12f;       // Default outer radius for the spotlight
    private float initialFalloffStrength = 0.767f; // Default falloff strength for the spotlight
    private bool playerNearby = false;    // Track if the player is near

    void Start()
    {
        // Find the Light2D components for the diamond light and spotlight
        if (diamondLightObject != null)
        {
            diamondLight = diamondLightObject.GetComponent<Light2D>();
        }
        else
        {
            Debug.LogError("Diamond Light2D object is not assigned.");
            return;
        }

        if (spotLightObject != null)
        {
            spotLight = spotLightObject.GetComponent<Light2D>();
        }
        else
        {
            Debug.LogError("SpotLight object is not assigned.");
            return;
        }

        // Store initial values
        initialPosition = transform.position;
        initialScale = transform.localScale;
        initialLightColor = diamondLight.color;
        initialIntensity = diamondLight.intensity;

        // Initialize spotlight with default values
        spotLight.color = initialSpotColor;
        spotLight.intensity = initialSpotIntensity;
        spotLight.pointLightInnerRadius = initialRadiusInner;
        spotLight.pointLightOuterRadius = initialRadiusOuter;
        spotLight.falloffIntensity = initialFalloffStrength;
    }

    void Update()
    {
        // Floating effect (up and down)
        float newY = initialPosition.y + Mathf.Sin(Time.time * floatSpeed) * floatHeight;
        transform.position = new Vector3(initialPosition.x, newY, initialPosition.z);

        if (hexagonTransform != null)
        {
            // Calculate the distance between the diamond and the hexagon
            float distance = Vector3.Distance(transform.position, hexagonTransform.position);
            // Calculate the scale factor based on the distance
            float scale = Mathf.Clamp(1f + (proximityScale - 1f) * (1f - (distance / maxScaleDistance)), 1f, proximityScale);
            // Apply the scale to the diamond
            transform.localScale = initialScale * scale;
        }

        // Light flickering effect for diamond light
        if (playerNearby)
        {
            // Flicker faster and turn red when player is nearby (diamond light)
            diamondLight.intensity = initialIntensity + Random.Range(-flickerIntensityChange, flickerIntensityChange);
            diamondLight.color = Color.Lerp(diamondLight.color, Color.red, Time.deltaTime * 2); // Smooth transition to red

            // Flicker faster and turn red when player is nearby (spotlight)
            spotLight.intensity = initialSpotIntensity + Random.Range(-spotlightFlickerIntensityChange, spotlightFlickerIntensityChange);
            spotLight.color = Color.Lerp(spotLight.color, Color.red, Time.deltaTime * 2);
            spotLight.pointLightOuterRadius = Mathf.Lerp(spotLight.pointLightOuterRadius, initialRadiusOuter * 1.1f, Time.deltaTime * 2); // Slightly increase radius
        }
        else
        {
            // Normal flickering (diamond light)
            diamondLight.intensity = initialIntensity + Random.Range(-flickerIntensityChange / 2, flickerIntensityChange / 2);
            diamondLight.color = Color.Lerp(diamondLight.color, initialLightColor, Time.deltaTime * 2); // Smooth transition back to original color

            // Normal flickering (spotlight)
            spotLight.intensity = initialSpotIntensity + Random.Range(-spotlightFlickerIntensityChange / 2, spotlightFlickerIntensityChange / 2);
            spotLight.color = Color.Lerp(spotLight.color, initialSpotColor, Time.deltaTime * 2); // Smooth transition back to white
            spotLight.pointLightOuterRadius = Mathf.Lerp(spotLight.pointLightOuterRadius, initialRadiusOuter, Time.deltaTime * 2); // Restore original radius
        }
    }

    // Trigger when player enters the diamond's proximity
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player")) // Make sure the player has the "Player" tag
        {
            playerNearby = true;
        }
    }

    // Trigger when player leaves the diamond's proximity
    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerNearby = false;
        }
    }
}