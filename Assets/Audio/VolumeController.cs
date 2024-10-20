using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class VolumeController : MonoBehaviour
{
    public static VolumeController Instance { get; private set; } // Singleton instance

    public AudioSource feedbackAudioSource; // Audio source for feedback sounds (volume up/down)
    public AudioClip volumeUpSound;         // Sound to play when volume is increased
    public AudioClip volumeDownSound;       // Sound to play when volume is decreased
    public AudioClip muteSound;             // Sound to play when muted

    public Image volumeImage;                // Image to display volume PNG
    public Sprite[] volumeSprites;           // Array of sprites for each volume level (12 + 1 mute sprite)
    public float volumeStep = 0.1f;          // Volume increment/decrement value
    private float currentVolume = 1.0f;      // Current volume value (0.0 to 1.5)
    private bool isMuted = false;            // Mute state

    public CanvasGroup volumeUICanvas;       // CanvasGroup for the volume UI
    public float uiAnimationDuration = 0.3f; // Duration for UI fade in/out
    public float showDuration = 5f;          // Time before hiding UI
    public Vector2 offscreenPosition;        // Offscreen position for the UI
    public Vector2 onscreenPosition;         // Onscreen position for the UI

    private Coroutine hideUICoroutine;       // Reference for hiding coroutine
    private bool isShaking = false;          // Flag to prevent shake overlap

    // Public variable to hold references to all player audio sources
    public AudioSource[] playerAudioSources;

    // SFX Volume property for external access
    public float SFXVolume { get; private set; } = 1.0f; // Default SFX volume

    // Property to get the mute state
    public bool IsMuted { get { return isMuted; } }

    [Header("Volume Key Hold Settings")]
    public float holdThreshold = 1.0f; // Adjust hold threshold in the Unity Editor

    private float increaseHoldTime = 0f;
    private float decreaseHoldTime = 0f;
    private bool isIncreasing = false;
    private bool isDecreasing = false;
    private Coroutine increaseCoroutine;
    private Coroutine decreaseCoroutine;

    private void Awake()
    {
        // Implementing Singleton Pattern
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject); // Destroy duplicate instance
        }
        else
        {
            Instance = this; // Assign this instance as the singleton instance
            DontDestroyOnLoad(gameObject); // Keep this object between scenes
        }
    }

    void Start()
    {
        // Set initial position and alpha of the UI offscreen and hidden
        volumeImage.rectTransform.anchoredPosition = offscreenPosition; // Offscreen position
        volumeUICanvas.alpha = 0f;                   // Start with UI hidden
        volumeUICanvas.interactable = false;         // Disable interaction initially
        volumeUICanvas.blocksRaycasts = false;       // Disable raycasts

        currentVolume = 1.0f;
        SetAllAudioSourcesVolume(Mathf.Clamp01(currentVolume - 0.5f));  // Set initial volume for all audio sources
        UpdateVolumeUI();
    }

    void Update()
    {
        // Key down handling
        if (Input.GetKeyDown(KeyCode.Equals) || Input.GetKeyDown(KeyCode.Plus))
        {
            if (increaseCoroutine != null) StopCoroutine(increaseCoroutine);
            increaseCoroutine = StartCoroutine(HandleIncreaseVolume());
        }
        if (Input.GetKeyDown(KeyCode.Minus))
        {
            if (decreaseCoroutine != null) StopCoroutine(decreaseCoroutine);
            decreaseCoroutine = StartCoroutine(HandleDecreaseVolume());
        }

        // Key up handling
        if (Input.GetKeyUp(KeyCode.Equals) || Input.GetKeyUp(KeyCode.Plus))
        {
            isIncreasing = false;
            increaseHoldTime = 0f;
        }
        if (Input.GetKeyUp(KeyCode.Minus))
        {
            isDecreasing = false;
            decreaseHoldTime = 0f;
        }
    }

    // Coroutine to handle increasing volume
    IEnumerator HandleIncreaseVolume()
    {
        IncreaseVolume();
        isIncreasing = true;
        increaseHoldTime = 0f;

        while (isIncreasing)
        {
            increaseHoldTime += Time.deltaTime;
            if (increaseHoldTime >= holdThreshold)
            {
                IncreaseVolume();
                yield return new WaitForSeconds(0.1f); // Adjust the repeat rate as needed
            }
            else
            {
                yield return null;
            }
        }
    }

    // Coroutine to handle decreasing volume
    IEnumerator HandleDecreaseVolume()
    {
        DecreaseVolume();
        isDecreasing = true;
        decreaseHoldTime = 0f;

        while (isDecreasing)
        {
            decreaseHoldTime += Time.deltaTime;
            if (decreaseHoldTime >= holdThreshold)
            {
                DecreaseVolume();
                yield return new WaitForSeconds(0.1f); // Adjust the repeat rate as needed
            }
            else
            {
                yield return null;
            }
        }
    }

    // Increase the volume
    void IncreaseVolume()
    {
        if (!isMuted && currentVolume < 1.5f)
        {
            currentVolume += volumeStep;
            currentVolume = Mathf.Clamp(currentVolume, 0.5f, 1.5f); // Ensure minimum volume is 0.5f (mute)
            SetAllAudioSourcesVolume(Mathf.Clamp01(currentVolume - 0.5f));  // Apply to all game audio sources
            feedbackAudioSource.PlayOneShot(volumeUpSound);
            UpdateVolumeUI();
            ShowVolumeUI();  // Show the UI when volume changes
            StartCoroutine(ShakeUI());  // Add shake effect
        }
    }

    // Decrease the volume
    void DecreaseVolume()
    {
        if (!isMuted && currentVolume > 0.5f) // Prevent going below 0.5
        {
            currentVolume -= volumeStep;
            currentVolume = Mathf.Clamp(currentVolume, 0.5f, 1.5f); // Minimum volume is now 0.5f
            SetAllAudioSourcesVolume(Mathf.Clamp01(currentVolume - 0.5f));  // Apply to all game audio sources
            feedbackAudioSource.PlayOneShot(volumeDownSound);
            UpdateVolumeUI();
            ShowVolumeUI();  // Show the UI when volume changes
            StartCoroutine(ShakeUI());  // Add shake effect
        }
    }

    // Toggle mute state
    void ToggleMute()
    {
        if (isMuted)
        {
            // Unmute
            isMuted = false;
            SetAllAudioSourcesVolume(Mathf.Clamp01(currentVolume - 0.5f)); // Restore volume
            feedbackAudioSource.PlayOneShot(volumeUpSound); // Play unmute sound
            UpdateVolumeUI();
        }
        else
        {
            // Play the mute sound, update the sprite, and delay the actual muting
            StartCoroutine(HandleMute());
        }

        ShowVolumeUI(); // Show the UI when muting or unmuting
        StartCoroutine(ShakeUI()); // Add shake effect
    }

    // Coroutine to mute after playing the mute sound
    IEnumerator HandleMute()
    {
        UpdateVolumeUIForMute();  // Update sprite to mute immediately

        feedbackAudioSource.PlayOneShot(muteSound); // Play mute sound
        yield return new WaitForSeconds(muteSound.length); // Wait for the mute sound to finish

        SetAllAudioSourcesVolume(0.0f);  // Mute all audio sources
        isMuted = true;
    }

    // New method to mute all sounds based on the parameter
    public void MuteAllSounds(bool mute)
    {
        isMuted = mute;
        if (isMuted)
        {
            SetAllAudioSourcesVolume(0.0f);  // Mute all audio sources
            feedbackAudioSource.PlayOneShot(muteSound);
        }
        else
        {
            SetAllAudioSourcesVolume(Mathf.Clamp01(currentVolume - 0.5f));  // Restore volume
            feedbackAudioSource.PlayOneShot(volumeUpSound);
        }
        UpdateVolumeUI();
        ShowVolumeUI();  // Show the UI when muting
        StartCoroutine(ShakeUI());  // Add shake effect
    }

    // Set the volume for all the audio sources in the scene
    void SetAllAudioSourcesVolume(float volume)
    {
        // Set the volume for player audio sources, if any are assigned
        if (playerAudioSources != null)
        {
            foreach (var audioSource in playerAudioSources)
            {
                audioSource.volume = Mathf.Clamp01(volume);  // Adjust player's AudioSource volume
            }
        }

        // Find all AudioSource components in the scene and set their volumes
        AudioSource[] allAudioSources = FindObjectsOfType<AudioSource>();
        foreach (var audioSource in allAudioSources)
        {
            audioSource.volume = volume;  // Apply the volume to each audio source
        }

        // Update the SFX volume property
        SFXVolume = Mathf.Clamp01(volume);  // Update SFX volume
    }

    // Update the volume UI image based on the current volume and mute state
    void UpdateVolumeUI()
    {
        int index;

        if (isMuted)
        {
            index = 0; // Mute sprite
            Debug.Log("Mute state: Sprite set to index 0 (mute icon)");
        }
        else if (currentVolume <= 0.5f)
        {
            index = 0; // Map volume 0.5 to sprite 0 (mute)
            Debug.Log("Volume low: Sprite set to index 0 (mute icon)");
        }
        else
        {
            // Map volumes from 0.6 to 1.5 to sprites 1 to 11
            index = Mathf.Clamp(Mathf.RoundToInt((currentVolume - 0.5f) / 0.1f) + 1, 1, 11);
            Debug.Log("Volume adjusted: Sprite set to index " + index);
        }

        volumeImage.sprite = volumeSprites[index]; // Set the sprite to the calculated index
    }

    // Update the volume UI specifically for mute state
    void UpdateVolumeUIForMute()
    {
        volumeImage.sprite = volumeSprites[0]; // Set sprite to mute icon (index 0)
    }

    // Show the volume UI smoothly
    void ShowVolumeUI()
    {
        // Ensure UI is visible for animation
        volumeUICanvas.alpha = 1f;
        volumeUICanvas.interactable = true;
        volumeUICanvas.blocksRaycasts = true;

        // Animate to onscreen position smoothly (without elasticity)
        LeanTween.move(volumeImage.rectTransform, onscreenPosition, uiAnimationDuration).setEaseLinear();

        // Ensure to reset the UI position before showing again
        volumeImage.rectTransform.anchoredPosition = onscreenPosition;

        // Start the coroutine to hide the UI after a delay
        if (hideUICoroutine != null) StopCoroutine(hideUICoroutine); // Stop the previous hiding coroutine
        hideUICoroutine = StartCoroutine(HideVolumeUIAfterDelay()); // Start the hiding coroutine
    }

    // Coroutine to hide the volume UI after a delay
    IEnumerator HideVolumeUIAfterDelay()
    {
        yield return new WaitForSeconds(showDuration);
        StartCoroutine(FadeOutVolumeUI());  // Fade out after delay
    }

    // Fade out the volume UI
    IEnumerator FadeOutVolumeUI()
    {
        float startAlpha = volumeUICanvas.alpha;  // Current alpha
        float time = 0f;

        // Animate position back to offscreen
        LeanTween.move(volumeImage.rectTransform, offscreenPosition, uiAnimationDuration).setEaseLinear();

        while (time < uiAnimationDuration)
        {
            volumeUICanvas.alpha = Mathf.Lerp(startAlpha, 0f, time / uiAnimationDuration);
            time += Time.deltaTime;
            yield return null;
        }

        volumeUICanvas.alpha = 0f;  // Fully transparent
        volumeUICanvas.interactable = false;  // Disable interaction
        volumeUICanvas.blocksRaycasts = false; // Disable raycasts
    }

    // Coroutine to shake UI
    IEnumerator ShakeUI()
    {
        if (isShaking) yield break;  // Prevent overlapping shakes
        isShaking = true;

        Vector3 originalRotation = volumeImage.rectTransform.localEulerAngles; // Store original rotation

        float shakeDuration = 0.3f; // Total duration of the shake
        float elapsedTime = 0f; // Time tracker

        while (elapsedTime < shakeDuration)
        {
            float t = elapsedTime / shakeDuration; // Normalized time (0 to 1)

            // Calculate a random shake angle that goes both directions
            float shakeAngle = Random.Range(-5f, 5f); // Random angle between -5 and +5 degrees

            // Apply the shake effect
            volumeImage.rectTransform.localEulerAngles = originalRotation + new Vector3(0, 0, shakeAngle);

            elapsedTime += Time.deltaTime; // Increment elapsed time
            yield return null; // Wait for the next frame
        }

        // Return to original rotation smoothly
        volumeImage.rectTransform.localEulerAngles = originalRotation;

        isShaking = false;  // Allow future shakes
    }

    // Method to get the current volume
    public float GetCurrentVolume()
    {
        return isMuted ? 0f : Mathf.Clamp01(currentVolume - 0.5f);
    }
}