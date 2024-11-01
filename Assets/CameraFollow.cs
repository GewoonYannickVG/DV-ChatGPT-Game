using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CameraFollow : MonoBehaviour
{
    private static CameraFollow instance;

    [Header("Camera Settings")]
    [SerializeField] private Vector3 offset = new Vector3(0f, 0f, -10f);
    [SerializeField] private float smoothTime = 0.2f;
    [SerializeField] private Transform target;

    [Header("Field of View Settings")]
    [SerializeField] private float normalFOV = 87f;
    [SerializeField] private float shiftZoomedFOV = 95f;
    [SerializeField] private float cZoomedFOV = 60f;
    [SerializeField] private float fovSmoothTime = 0.2f;
    [SerializeField] private float cFovSmoothTime = 0.1f;

    private Camera cameraComponent;
    private float targetFOV;
    private float currentFOV;
    private Vector3 velocity = Vector3.zero;
    private bool isZoomingC = false;

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

    private void Start()
    {
        cameraComponent = GetComponent<Camera>();
        currentFOV = normalFOV;
        cameraComponent.fieldOfView = currentFOV;
    }

    private void Update()
    {
        // Check the current scene and set smoothTime to 0 for Level2
        if (SceneManager.GetActiveScene().name == "Level2")
        {
            smoothTime = 0f;
        }
        else
        {
            smoothTime = 0.2f; // Reset to default or any other value you prefer
        }

        Vector3 targetPosition = target.position + offset;
        Vector3 newPosition = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, smoothTime);
        transform.position = newPosition;

        if (Input.GetKey(KeyCode.C))
        {
            targetFOV = cZoomedFOV;
            isZoomingC = true;
        }
        else if (isZoomingC)
        {
            targetFOV = normalFOV;
            isZoomingC = false;
        }
        else if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
        {
            targetFOV = shiftZoomedFOV;
        }
        else
        {
            targetFOV = normalFOV;
        }

        float currentFovSmoothTime = isZoomingC ? cFovSmoothTime : fovSmoothTime;
        currentFOV = Mathf.Lerp(currentFOV, targetFOV, Time.deltaTime / currentFovSmoothTime);
        cameraComponent.fieldOfView = currentFOV;
    }
}