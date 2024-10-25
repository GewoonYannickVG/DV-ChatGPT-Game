using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    private static CameraFollow instance;

    [Header("Camera Settings")]
    [SerializeField] private Vector3 offset = new Vector3(0f, 0f, -10f); // Offset position from the target
    [SerializeField] private float smoothTime = 0.2f; // Smooth time for camera movement
    [SerializeField] private Transform target; // The target to follow

    [Header("Field of View Settings")]
    [SerializeField] private float normalFOV = 87f; // Normal FOV when shift is not held
    [SerializeField] private float zoomedFOV = 95f; // FOV when shift is held
    [SerializeField] private float fovSmoothTime = 0.2f; // Smooth time for FOV transition

    private Camera cameraComponent; // Reference to the camera component
    private float targetFOV; // Target FOV value
    private float currentFOV; // Current FOV value
    private Vector3 velocity = Vector3.zero; // Velocity for SmoothDamp

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);  // Make this object persist across scenes
        }
        else
        {
            Destroy(gameObject);  // Destroy duplicate CameraFollow
        }
    }

    private void Start()
    {
        cameraComponent = GetComponent<Camera>(); // Get the Camera component
        currentFOV = normalFOV; // Initialize current FOV
        cameraComponent.fieldOfView = currentFOV; // Set initial FOV
    }

    private void Update()
    {
        // Calculate the target position for the camera
        Vector3 targetPosition = target.position + offset;

        // Smoothly move the camera to the target position
        Vector3 newPosition = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, smoothTime);
        transform.position = newPosition;

        // Check if Shift is being held down
        if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
        {
            targetFOV = zoomedFOV; // Set target FOV to zoomed FOV
        }
        else
        {
            targetFOV = normalFOV; // Set target FOV back to normal
        }

        // Smoothly transition the current FOV to the target FOV
        currentFOV = Mathf.Lerp(currentFOV, targetFOV, Time.deltaTime / fovSmoothTime);
        cameraComponent.fieldOfView = currentFOV; // Apply the current FOV to the camera
    }
}