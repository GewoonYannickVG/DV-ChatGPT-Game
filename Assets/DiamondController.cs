using UnityEngine;
using UnityEngine.Experimental.Rendering.Universal;
using UnityEngine.Rendering.Universal;
using System.Collections;

public class DiamondController : MonoBehaviour
{
    private static DiamondController instance;

    public static DiamondController Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<DiamondController>();
                if (instance == null)
                {
                    GameObject obj = new GameObject("DiamondController");
                    instance = obj.AddComponent<DiamondController>();
                }
            }
            return instance;
        }
    }

    public Transform hexagonTransform;
    public Transform diamondLightObject;
    public Transform spotLightObject;
    public Transform playerTransform;
    public float floatSpeed = 1f;
    public float floatHeight = 0.5f;
    public float flickerSpeed = 0.1f;
    public float baseFlickerSpeed = 0.1f;
    public float proximityFlickerSpeed = 0.05f;
    public float proximityScale = 1.2f;
    public float flickerIntensityChange = 0.5f;
    public float spotlightFlickerIntensityChange = 0.1f;
    public float maxScaleDistance = 5f;
    public float maxShakeAmount = 0.1f;
    public float maxRotationAmount = 5f;
    public float transitionRadius = 0.5f;
    public float initialFalloffStrength = 0.767f; // Add this line
    public float redLightStartDistance = 10f; // Distance from which red light starts
    public float maxRedLightIntensity = 2f; // Maximum red light intensity
    public float maxFlickerSpeed = 0.2f; // Maximum flicker speed near the diamond

    private Light2D diamondLight;
    private Light2D spotLight;
    private Vector3 initialPosition;
    private Vector3 initialScale;
    private Color initialLightColor;
    private Color initialSpotColor = Color.white;
    private float initialIntensity;
    private float initialSpotIntensity = 0.26f;
    private float initialRadiusInner = 0f;
    private float initialRadiusOuter = 12f;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

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
        initialScale = transform.localScale;
        initialLightColor = diamondLight.color;
        initialIntensity = diamondLight.intensity;

        spotLight.color = initialSpotColor;
        spotLight.intensity = initialSpotIntensity;
        spotLight.pointLightInnerRadius = initialRadiusInner;
        spotLight.pointLightOuterRadius = initialRadiusOuter;
        spotLight.falloffIntensity = initialFalloffStrength;
    }

    void Update()
    {
        float newY = initialPosition.y + Mathf.Sin(Time.time * floatSpeed) * floatHeight;
        transform.position = new Vector3(initialPosition.x, newY, initialPosition.z);

        if (hexagonTransform != null)
        {
            float distance = Vector3.Distance(transform.position, hexagonTransform.position);
            float scale = Mathf.Clamp(1f + (proximityScale - 1f) * (1f - (distance / maxScaleDistance)), 1f, proximityScale);
            transform.localScale = initialScale * scale;

            // Calculate shake intensity based on distance
            float shakeIntensity = Mathf.Clamp01(1f - (distance / maxScaleDistance));
            Vector3 shakeOffset = new Vector3(
                Random.Range(-maxShakeAmount, maxShakeAmount) * shakeIntensity,
                Random.Range(-maxShakeAmount, maxShakeAmount) * shakeIntensity,
                0
            );
            float rotationOffset = Random.Range(-maxRotationAmount, maxRotationAmount) * shakeIntensity;

            transform.position += shakeOffset;
            transform.rotation = Quaternion.Euler(0, 0, rotationOffset);
        }

        UpdateLightEffects();
        CheckPlayerProximity();
    }

    private void UpdateLightEffects()
    {
        float distanceToPlayer = playerTransform != null ? Vector3.Distance(transform.position, playerTransform.position) : float.MaxValue;
        float proximityFactor = Mathf.Clamp01((redLightStartDistance - distanceToPlayer) / redLightStartDistance);

        diamondLight.intensity = initialIntensity + Random.Range(-flickerIntensityChange, flickerIntensityChange) * proximityFactor;
        diamondLight.color = Color.Lerp(initialLightColor, Color.red, proximityFactor);

        spotLight.intensity = initialSpotIntensity + Random.Range(-spotlightFlickerIntensityChange, spotlightFlickerIntensityChange) * proximityFactor;
        spotLight.color = Color.Lerp(initialSpotColor, Color.red, proximityFactor);
        spotLight.pointLightOuterRadius = Mathf.Lerp(initialRadiusOuter, initialRadiusOuter * 1.1f, proximityFactor);

        // Adjust flicker speed based on proximity
        float currentFlickerSpeed = Mathf.Lerp(baseFlickerSpeed, maxFlickerSpeed, proximityFactor);
        diamondLight.intensity += Mathf.Sin(Time.time * currentFlickerSpeed) * flickerIntensityChange * proximityFactor;
        spotLight.intensity += Mathf.Sin(Time.time * currentFlickerSpeed) * spotlightFlickerIntensityChange * proximityFactor;
    }

    private void CheckPlayerProximity()
    {
        if (playerTransform != null)
        {
            float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);
            if (distanceToPlayer <= transitionRadius)
            {
                SceneTransitionManager.Instance.TransitionToNextScene();
            }
        }
    }

    public IEnumerator FadeOutLights()
    {
        float fadeDuration = 1f;
        float timer = 0f;

        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            float t = timer / fadeDuration;
            diamondLight.intensity = Mathf.Lerp(initialIntensity, 0, t);
            spotLight.intensity = Mathf.Lerp(initialSpotIntensity, 0, t);
            yield return null;
        }

        diamondLight.intensity = 0;
        spotLight.intensity = 0;
    }
}