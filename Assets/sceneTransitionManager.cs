using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.Audio;

public class SceneTransitionManager : MonoBehaviour
{
    public static SceneTransitionManager Instance { get; private set; }
    private DiamondController diamondController;

    public GameObject fadeToBlack; // Reference to the FadeToBlack GameObject
    public float transitionDuration = 3f; // Duration of the fade-in and fade-out
    public AudioMixer audioMixer; // Reference to the AudioMixer
    public string exposedVolumeParameter = "Volume"; // Exposed parameter name

    private Image blackPanel; // Reference to the Image component of the black panel

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        if (fadeToBlack != null)
        {
            blackPanel = fadeToBlack.GetComponentInChildren<Image>();
            if (blackPanel == null)
            {
                Debug.LogError("Black panel Image component not found in FadeToBlack GameObject.");
            }
        }
        else
        {
            Debug.LogError("FadeToBlack GameObject is not assigned.");
        }
    }

    public void TransitionToNextScene()
    {
        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
        int nextSceneIndex = (currentSceneIndex + 1) % SceneManager.sceneCountInBuildSettings;
        StartCoroutine(Transition(nextSceneIndex));
    }

    private IEnumerator Transition(int sceneIndex)
    {
        Time.timeScale = 1; // Ensure normal speed
        yield return StartCoroutine(Fade(1));
        yield return StartCoroutine(FadeMixerGroupVolume(0f, -80f)); // Fade out volume

        diamondController = FindObjectOfType<DiamondController>();
        if (diamondController != null)
        {
            yield return StartCoroutine(diamondController.FadeOutLights());
        }

        yield return new WaitForSeconds(1f); // Simulate transition delay
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneIndex);
        while (!asyncLoad.isDone)
        {
            yield return null;
        }

        // Add a delay to ensure the scene is fully loaded before starting the fade-out
        yield return new WaitForSeconds(1f);

        // Start fade-out
        yield return StartCoroutine(Fade(0));
        yield return StartCoroutine(FadeMixerGroupVolume(-80f, 0f)); // Fade in volume
    }

    private IEnumerator Fade(float targetAlpha)
    {
        float startAlpha = blackPanel.color.a;
        float elapsedTime = 0f;

        while (elapsedTime < transitionDuration)
        {
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Lerp(startAlpha, targetAlpha, elapsedTime / transitionDuration);
            blackPanel.color = new Color(blackPanel.color.r, blackPanel.color.g, blackPanel.color.b, alpha);
            yield return null;
        }

        blackPanel.color = new Color(blackPanel.color.r, blackPanel.color.g, blackPanel.color.b, targetAlpha);
    }

    private IEnumerator FadeMixerGroupVolume(float startVolume, float targetVolume)
    {
        float currentTime = 0f;
        float duration = transitionDuration;

        audioMixer.GetFloat(exposedVolumeParameter, out startVolume);
        while (currentTime < duration)
        {
            currentTime += Time.deltaTime;
            float newVolume = Mathf.Lerp(startVolume, targetVolume, currentTime / duration);
            audioMixer.SetFloat(exposedVolumeParameter, newVolume);
            yield return null;
        }
        audioMixer.SetFloat(exposedVolumeParameter, targetVolume);
    }
}