using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal; // Ensure you have this if you're using URP

public class StartButton : MonoBehaviour
{
    [SerializeField] private float cameraMoveDuration = 1.5f;       // Duration for the camera to move
    [SerializeField] private string sceneName = "SampleScene";      // Name of the scene to load
    [SerializeField] private CanvasGroup uiCanvasGroup;             // Reference to CanvasGroup for UI fading
    [SerializeField] private float fadeDuration = 1.5f;             // Duration of fade-out effect
    [SerializeField] private float lensDistortionDuration = 1.5f;   // Duration for lens distortion adjustment
    private Camera mainCamera;
    private Volume volume;
    private LensDistortion lensDistortion;

    private void Start()
    {
        // Get the main camera reference
        mainCamera = Camera.main;

        // Get the Volume component from the camera
        volume = mainCamera.GetComponent<Volume>();

        // Check if the Volume component exists and retrieve the LensDistortion override
        if (volume != null)
        {
            volume.profile.TryGet(out lensDistortion);
            // Set the initial intensity to 0.7
            if (lensDistortion != null)
            {
                lensDistortion.intensity.value = 0.7f; // Set initial intensity
            }
        }
    }

    // This method will be called when the button is clicked
    public void GoToGameScene()
    {
        // Start the transition coroutine
        StartCoroutine(MoveCameraAndLoadScene());
    }

    private IEnumerator MoveCameraAndLoadScene()
    {
        // Disable specific camera scripts like CameraFollowMenu
        DisableSpecificCameraScripts();

        // Define target position for the camera movement
        Vector3 targetPosition = new Vector3(2.05f, 0f, mainCamera.transform.position.z);

        // Store initial values for the UI and lens distortion
        float startUIAlpha = uiCanvasGroup.alpha;
        float targetUIAlpha = 0f;
        float startIntensity = 0.7f; // Starting intensity for lens distortion
        float targetIntensity = 0.5f; // Target intensity for lens distortion

        // Initialize elapsed time
        float elapsedTime = 0f;

        // Smoothly transition the camera, UI, and lens distortion in the same loop
        while (elapsedTime < cameraMoveDuration || elapsedTime < fadeDuration || elapsedTime < lensDistortionDuration)
        {
            // Calculate the normalized time (t) for camera movement
            float cameraT = Mathf.Clamp01(elapsedTime / cameraMoveDuration);
            // Apply a smooth easing function to `cameraT` (ease-in, ease-out)
            cameraT = cameraT * cameraT * (3f - 2f * cameraT);

            // Move the camera smoothly towards the target position
            mainCamera.transform.position = Vector3.Lerp(mainCamera.transform.position, targetPosition, cameraT);

            // Fade out the UI
            uiCanvasGroup.alpha = Mathf.Lerp(startUIAlpha, targetUIAlpha, elapsedTime / fadeDuration);

            // Adjust lens distortion intensity
            lensDistortion.intensity.value = Mathf.Lerp(startIntensity, targetIntensity, elapsedTime / lensDistortionDuration);

            // Increment elapsed time
            elapsedTime += Time.deltaTime;
            yield return null; // Wait until the next frame
        }

        // Ensure all final positions and states are set correctly
        mainCamera.transform.position = targetPosition;
        uiCanvasGroup.alpha = targetUIAlpha;
        lensDistortion.intensity.value = targetIntensity;

        // Load the specified scene after all transitions are complete
        SceneManager.LoadScene(sceneName);
    }

    private void DisableSpecificCameraScripts()
    {
        // Identify and disable only specific camera movement scripts like CameraFollowMenu
        MonoBehaviour[] cameraScripts = mainCamera.GetComponents<MonoBehaviour>();
        foreach (MonoBehaviour script in cameraScripts)
        {
            if (script is CameraFollowMenu) // Check if the script is a CameraFollowMenu script
            {
                script.enabled = false;
            }
        }
    }
}
