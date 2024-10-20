using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollowMenu : MonoBehaviour
{
    private Vector3 initialPosition;          // Initial position of the camera
    private Vector3 velocity = Vector3.zero;

    [SerializeField] private Transform lookAtTarget;        // Target for the camera to focus on
    [SerializeField] private float sensitivity = 0.5f;      // Sensitivity of camera movement based on mouse position
    [SerializeField] private float smoothTime = 0.3f;       // Time it takes for the camera to reach the target position
    [SerializeField] private float maxOffsetX = 2f;         // Maximum horizontal offset
    [SerializeField] private float maxOffsetY = 1.5f;       // Maximum vertical offset
    [SerializeField] private float distanceFromTarget = 10f; // Distance camera stays from the target

    private void Start()
    {
        // Store the initial position of the camera (offset back from the target by 'distanceFromTarget')
        initialPosition = lookAtTarget.position - transform.forward * distanceFromTarget;
        transform.position = initialPosition; // Set the camera's initial position
    }

    private void Update()
    {
        // Get the mouse position in screen space, normalized to a range of -0.5 to 0.5
        Vector3 mousePosition = Input.mousePosition;
        mousePosition.x = (mousePosition.x / Screen.width) - 0.5f; // Normalize X position (-0.5 to 0.5)
        mousePosition.y = (mousePosition.y / Screen.height) - 0.5f; // Normalize Y position (-0.5 to 0.5)

        // Flip the direction of movement by multiplying by -1
        Vector3 targetOffset = new Vector3(
            Mathf.Clamp(-mousePosition.x * maxOffsetX * sensitivity, -maxOffsetX, maxOffsetX), // Horizontal offset (flipped)
            Mathf.Clamp(-mousePosition.y * maxOffsetY * sensitivity, -maxOffsetY, maxOffsetY), // Vertical offset (flipped)
            0f // No offset in Z direction
        );

        // Determine the camera's target position by applying the offset to the initial position
        Vector3 targetPosition = initialPosition + targetOffset;

        // Smoothly move the camera to the target position
        transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, smoothTime);

        // Ensure the camera always looks at the target object
        transform.LookAt(lookAtTarget);
    }
}
