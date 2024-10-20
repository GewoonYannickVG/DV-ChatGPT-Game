using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal; // Ensure you have this if you're using URP
using UnityEngine.Experimental.Rendering.Universal; // Import for Light2D

public class StartButton : MonoBehaviour
{
    [SerializeField] private float cameraMoveDuration = 1.5f;       // Duration for the camera to move
    [SerializeField] private string sceneName = "SampleScene";      // Name of the scene to load
    [SerializeField] private CanvasGroup uiCanvasGroup;             // Reference to CanvasGroup for UI fading
    [SerializeField] private float fadeDuration = 1.5f;             // Duration of fade-out effect
    [SerializeField] private float lensDistortionDuration = 1.5f;   // Duration for lens distortion adjustment
    [SerializeField] private Transform hexagonTarget;               // Reference to the hexagon GameObject
    [SerializeField] private Light2D hexagonLight;                  // Reference to the Light2D attached to the hexagon
    [SerializeField] private float cameraDistanceFromTarget = 5f;   // Distance the camera should stay from the hexagon
    [SerializeField] private float lightStartIntensity = 0.5f;      // Starting intensity of the hexagon's light
    [SerializeField] private float lightTargetIntensity = 2.5f;     // Target intensity of the hexagon's light

    // Added variables for button click sound
    [SerializeField] private AudioSource buttonClickAudioSource;     // Reference to the AudioSource for button click sound
    [SerializeField] private AudioClip buttonClickClip;              // Reference to the AudioClip for button click sound

    private Camera mainCamera;
    private Volume volume;
    private LensDistortion lensDistortion;

    // Cooldown for button clicks
    private bool isButtonInteractable = true; // Track button interactability
    [SerializeField] private float buttonCooldownDuration = 1.5f; // Cooldown duration in seconds

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

        // Initialize the hexagon Light2D intensity
        if (hexagonLight != null)
        {
            hexagonLight.intensity = lightStartIntensity;
        }
    }

    // This method will be called when the button is clicked
    public void GoToGameScene()
    {
        if (!isButtonInteractable) return; // Exit if the button is not interactable

        // Play the button click sound when the button is pressed
        if (buttonClickAudioSource != null && buttonClickClip != null)
        {
            buttonClickAudioSource.PlayOneShot(buttonClickClip); // Play the button click sound
        }

        // Play the BuildUpSound from the AudioManager when the button is pressed
        if (AudioManager.instance != null)
        {
            AudioManager.instance.PlayBuildUpSound(); // Play the build-up sound
            StartCoroutine(AudioManager.instance.FadeOutBackgroundMusic()); // Fade out background music

            // Optionally stop the BuildUpSound after the scene transition or a delay
            StartCoroutine(AudioManager.instance.StopBuildUpSound(3f)); // Stops after 3 seconds
        }

        // Start the transition coroutine
        StartCoroutine(MoveCameraAndLoadScene());
    }

    private IEnumerator MoveCameraAndLoadScene()
    {
        // Set the button to non-interactable
        isButtonInteractable = false;

        // Disable specific camera scripts like CameraFollowMenu
        DisableSpecificCameraScripts();

        // Check if the hexagon target and light are assigned
        if (hexagonTarget == null || hexagonLight == null)
        {
            Debug.LogError("Hexagon target or Light2D not assigned.");
            yield break;
        }

        // Store initial values for the UI and lens distortion
        float startUIAlpha = uiCanvasGroup.alpha;
        float targetUIAlpha = 0f;
        float startIntensity = 0.7f; // Starting intensity for lens distortion
        float targetIntensity = 0.5f; // Target intensity for lens distortion

        // Get initial camera position and rotation
        Vector3 startPosition = mainCamera.transform.position;
        Quaternion startRotation = mainCamera.transform.rotation;

        // Calculate target position at a fixed distance from the hexagon
        Vector3 directionToHexagon = (mainCamera.transform.position - hexagonTarget.position).normalized;
        Vector3 targetPosition = hexagonTarget.position + directionToHexagon * cameraDistanceFromTarget;

        // Initialize elapsed time
        float elapsedTime = 0f;

        // Smoothly transition the camera, UI, lens distortion, and light intensity in the same loop
        while (elapsedTime < cameraMoveDuration || elapsedTime < fadeDuration || elapsedTime < lensDistortionDuration)
        {
            // Calculate the normalized time (t) for camera movement
            float cameraT = Mathf.Clamp01(elapsedTime / cameraMoveDuration);
            // Apply a smooth easing function to cameraT (ease-in, ease-out)
            cameraT = cameraT * cameraT * (3f - 2f * cameraT);

            // Move the camera smoothly towards the target position
            mainCamera.transform.position = Vector3.Lerp(startPosition, targetPosition, cameraT);

            // Smoothly rotate the camera to look at the hexagon
            Quaternion targetRotation = Quaternion.LookRotation(hexagonTarget.position - mainCamera.transform.position);
            mainCamera.transform.rotation = Quaternion.Slerp(startRotation, targetRotation, cameraT);

            // Fade out the UI
            uiCanvasGroup.alpha = Mathf.Lerp(startUIAlpha, targetUIAlpha, elapsedTime / fadeDuration);

            // Adjust lens distortion intensity
            lensDistortion.intensity.value = Mathf.Lerp(startIntensity, targetIntensity, elapsedTime / lensDistortionDuration);

            // Increase hexagon's light intensity smoothly (Light2D component)
            hexagonLight.intensity = Mathf.Lerp(lightStartIntensity, lightTargetIntensity, cameraT);

            // Increment elapsed time
            elapsedTime += Time.deltaTime;
            yield return null; // Wait until the next frame
        }

        // Ensure all final positions and states are set correctly
        mainCamera.transform.position = targetPosition;
        mainCamera.transform.rotation = Quaternion.LookRotation(hexagonTarget.position - mainCamera.transform.position);
        uiCanvasGroup.alpha = targetUIAlpha;
        lensDistortion.intensity.value = targetIntensity;

        // Set final Light2D intensity
        hexagonLight.intensity = lightTargetIntensity;

        // Load the specified scene after all transitions are complete
        SceneManager.LoadScene(sceneName);

        // Start cooldown for button
        yield return new WaitForSeconds(buttonCooldownDuration);

        // Re-enable the button after cooldown
        isButtonInteractable = true;
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
