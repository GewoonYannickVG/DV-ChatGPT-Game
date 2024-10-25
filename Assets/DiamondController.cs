using UnityEngine;
using UnityEngine.Experimental.Rendering.Universal;
using System.Collections;
using UnityEngine.Rendering.Universal;

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
    public float floatSpeed = 1f;
    public float floatHeight = 0.5f;
    public float flickerSpeed = 0.1f;
    public float proximityFlickerSpeed = 0.05f;
    public float proximityScale = 1.2f;
    public float flickerIntensityChange = 0.5f;
    public float spotlightFlickerIntensityChange = 0.1f;
    public float maxScaleDistance = 5f;

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
    private float initialFalloffStrength = 0.767f;
    private bool playerNearby = false;

    private bool shouldFadeIn = true;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
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

        if (shouldFadeIn)
        {
            spotLight.color = initialSpotColor;
            spotLight.intensity = initialSpotIntensity;
            spotLight.pointLightInnerRadius = initialRadiusInner;
            spotLight.pointLightOuterRadius = initialRadiusOuter;
            spotLight.falloffIntensity = initialFalloffStrength;
        }
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
        }

        if (playerNearby)
        {
            diamondLight.intensity = initialIntensity + Random.Range(-flickerIntensityChange, flickerIntensityChange);
            diamondLight.color = Color.Lerp(diamondLight.color, Color.red, Time.deltaTime * 2);

            spotLight.intensity = initialSpotIntensity + Random.Range(-spotlightFlickerIntensityChange, spotlightFlickerIntensityChange);
            spotLight.color = Color.Lerp(spotLight.color, Color.red, Time.deltaTime * 2);
            spotLight.pointLightOuterRadius = Mathf.Lerp(spotLight.pointLightOuterRadius, initialRadiusOuter * 1.1f, Time.deltaTime * 2);
        }
        else
        {
            diamondLight.intensity = initialIntensity + Random.Range(-flickerIntensityChange / 2, flickerIntensityChange / 2);
            diamondLight.color = Color.Lerp(diamondLight.color, initialLightColor, Time.deltaTime * 2);

            spotLight.intensity = initialSpotIntensity + Random.Range(-spotlightFlickerIntensityChange / 2, spotlightFlickerIntensityChange / 2);
            spotLight.color = Color.Lerp(spotLight.color, initialSpotColor, Time.deltaTime * 2);
            spotLight.pointLightOuterRadius = Mathf.Lerp(spotLight.pointLightOuterRadius, initialRadiusOuter, Time.deltaTime * 2);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerNearby = true;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerNearby = false;
        }
    }

    public IEnumerator FadeOutLights()
    {
        shouldFadeIn = false;
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

        RemoveOriginalDiamondIfDuplicateExists();
    }

    public void StartFadeOutLights()
    {
        StartCoroutine(FadeOutLights());
    }

    private void RemoveOriginalDiamondIfDuplicateExists()
    {
        DiamondController[] diamonds = FindObjectsOfType<DiamondController>();
        if (diamonds.Length > 1)
        {
            Destroy(gameObject);
        }
    }
}