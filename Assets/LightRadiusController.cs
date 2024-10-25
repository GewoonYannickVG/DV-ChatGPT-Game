using UnityEngine;
using UnityEngine.Rendering.Universal;
using System.Collections;

public class LightRadiusController : MonoBehaviour
{
    private static LightRadiusController instance;

    [Header("Light Settings")]
    [SerializeField] private Light2D light2D;
    [SerializeField] private float maxRadius = 10f;
    [SerializeField] private float minRadius = 5f;
    [SerializeField] private float radiusChangeSpeed = 2f;

    [Header("Intensity Settings")]
    [SerializeField] private float initialIntensity = 0f;
    [SerializeField] private float normalIntensity = 1f;
    [SerializeField] private float transitionDuration = 2f;
    [SerializeField] private float reducedIntensity = 0.5f;
    [SerializeField] private float intensityChangeSpeedHold = 2f;
    [SerializeField] private float intensityChangeSpeedRelease = 2f;

    private float targetRadius;
    private float targetIntensity;
    private bool shouldFadeIn = true;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);  // Make this object persist across scenes
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        if (light2D != null && shouldFadeIn)
        {
            targetRadius = light2D.pointLightOuterRadius;
            targetIntensity = normalIntensity;
            light2D.intensity = initialIntensity;
            StartCoroutine(FadeInLight(transitionDuration));
        }
    }

    private IEnumerator FadeInLight(float duration)
    {
        float elapsedTime = 0f;
        float initialIntensity = light2D.intensity;

        while (elapsedTime < duration && shouldFadeIn)
        {
            elapsedTime += Time.deltaTime;
            light2D.intensity = Mathf.Lerp(initialIntensity, normalIntensity, elapsedTime / duration);
            yield return null;
        }

        if (shouldFadeIn)
        {
            light2D.intensity = normalIntensity;
        }
    }

    void Update()
    {
        if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
        {
            targetRadius = maxRadius;
            targetIntensity = reducedIntensity;
        }
        else
        {
            targetRadius = minRadius;
            targetIntensity = normalIntensity;
        }

        if (light2D != null)
        {
            light2D.pointLightOuterRadius = Mathf.Lerp(light2D.pointLightOuterRadius, targetRadius, Time.deltaTime * radiusChangeSpeed);

            float intensitySpeed = (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
                ? intensityChangeSpeedHold
                : intensityChangeSpeedRelease;

            light2D.intensity = Mathf.Lerp(light2D.intensity, targetIntensity, Time.deltaTime * intensitySpeed);
        }
    }

    public IEnumerator FadeOutLights()
    {
        shouldFadeIn = false; // Prevent lights from fading in
        float duration = 2f;
        float startIntensity = light2D.intensity;

        for (float t = 0; t < duration; t += Time.deltaTime)
        {
            light2D.intensity = Mathf.Lerp(startIntensity, 0, t / duration);
            yield return null;
        }

        light2D.intensity = 0;

        DiamondController.Instance.StartFadeOutLights();
    }
}