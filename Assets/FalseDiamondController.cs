using UnityEngine;
using UnityEngine.Experimental.Rendering.Universal;
using UnityEngine.Rendering.Universal;

public class SimpleDiamondController : MonoBehaviour
{
    [SerializeField] private Transform diamondLightObject;
    [SerializeField] private Transform spotLightObject;
    [SerializeField] private float floatSpeed = 1f;
    [SerializeField] private float floatHeight = 0.5f;
    [SerializeField] private float flickerIntensityChange = 0.5f;
    [SerializeField] private float spotlightFlickerIntensityChange = 0.1f;

    private Light2D diamondLight;
    private Light2D spotLight;
    private Vector3 initialPosition;
    private float initialIntensity;
    private float initialSpotIntensity;

    void Start()
    {
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

        initialPosition = transform.position;
        initialIntensity = diamondLight.intensity;
        initialSpotIntensity = spotLight.intensity;
    }

    void Update()
    {
        // Levitation
        float newY = initialPosition.y + Mathf.Sin(Time.time * floatSpeed) * floatHeight;
        transform.position = new Vector3(initialPosition.x, newY, initialPosition.z);

        // Light Flickering
        diamondLight.intensity = initialIntensity + Random.Range(-flickerIntensityChange, flickerIntensityChange);
        spotLight.intensity = initialSpotIntensity + Random.Range(-spotlightFlickerIntensityChange, spotlightFlickerIntensityChange);
    }
}