using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using UnityEngine.UI; // Add this to use UI components

public class SceneTransitionManager : MonoBehaviour
{
    public static SceneTransitionManager Instance { get; private set; }
    private DiamondController diamondController;

    public GameObject fadeToBlack; // Reference to the FadeToBlack GameObject
    public float transitionDuration = 3f; // Duration of the fade-in and fade-out

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
        // Start fade-in
        yield return StartCoroutine(Fade(1));

        diamondController = FindObjectOfType<DiamondController>();
        if (diamondController != null)
        {
            yield return StartCoroutine(diamondController.FadeOutLights());
        }

        yield return new WaitForSeconds(1f); // Simulate transition delay
        SceneManager.LoadScene(sceneIndex);

        // Start fade-out
        yield return StartCoroutine(Fade(0));
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
}