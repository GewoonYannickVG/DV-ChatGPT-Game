using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollowMenu : MonoBehaviour
{
    private Vector3 offset = new Vector3(0f, 0f, -10f);
    private Vector3 initialPosition;
    private Vector3 velocity = Vector3.zero;

    [SerializeField] private float sensitivity = 0.5f;      // Sensitivity of camera movement based on mouse position
    [SerializeField] private float smoothTime = 0.3f;       // Time it takes for the camera to reach the target position
    [SerializeField] private float maxOffsetX = 1.5f;       // Maximum horizontal offset from initial position
    [SerializeField] private float maxOffsetY = 1.5f;       // Maximum vertical offset from initial position

    private void Start()
    {
        // Store the initial position of the camera
        initialPosition = transform.position;
    }

    private void Update()
    {
        // Get the mouse position in screen space and normalize it to a range of -0.5 to 0.5
        Vector3 mousePosition = Input.mousePosition;
        mousePosition.x = (mousePosition.x / Screen.width) - 0.5f; // Normalize X position (-0.5 to 0.5)
        mousePosition.y = (mousePosition.y / Screen.height) - 0.5f; // Normalize Y position (-0.5 to 0.5)

        // Calculate target position based on mouse position and sensitivity
        Vector3 targetPosition = new Vector3(
            Mathf.Clamp(initialPosition.x + mousePosition.x * maxOffsetX * sensitivity, initialPosition.x - maxOffsetX, initialPosition.x + maxOffsetX),
            Mathf.Clamp(initialPosition.y + mousePosition.y * maxOffsetY * sensitivity, initialPosition.y - maxOffsetY, initialPosition.y + maxOffsetY),
            initialPosition.z
        );

        // Smoothly move the camera to the target position using SmoothDamp
        transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, smoothTime);
    }
}
