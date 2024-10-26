using UnityEngine;
using UnityEngine.Rendering.Universal;

public class LightRadiusController : MonoBehaviour
{
    private static LightRadiusController instance;

    [Header("Light Settings")]
    [SerializeField] private Light2D light2D;
    [SerializeField] private float maxRadius = 10f;
    [SerializeField] private float minRadius = 5f;
    [SerializeField] private float radiusChangeSpeed = 2f;

    [Header("Intensity Settings")]
    [SerializeField] private float normalIntensity = 1f;
    [SerializeField] private float reducedIntensity = 0.5f;
    [SerializeField] private float intensityChangeDurationHold = 2f;  // Duration in seconds
    [SerializeField] private float intensityChangeDurationRelease = 2f;  // Duration in seconds

    private float targetRadius;
    private float targetIntensity;

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
        if (light2D != null)
        {
            targetRadius = light2D.pointLightOuterRadius;
            targetIntensity = normalIntensity;
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
                ? 1f / intensityChangeDurationHold
                : 1f / intensityChangeDurationRelease;

            light2D.intensity = Mathf.Lerp(light2D.intensity, targetIntensity, Time.deltaTime * intensitySpeed);
        }
    }
}