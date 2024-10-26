using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class SceneTransitionManager : MonoBehaviour
{
    public static SceneTransitionManager Instance { get; private set; }
    private DiamondController diamondController;

    public GameObject fadeToBlack; // Reference to the FadeToBlack GameObject
    public float transitionDuration = 3f; // Duration of the fade-in and fade-out

    private CanvasGroup panelCanvasGroup; // Reference to the CanvasGroup component of the panel

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
            panelCanvasGroup = fadeToBlack.GetComponentInChildren<CanvasGroup>();
            if (panelCanvasGroup == null)
            {
                Debug.LogError("CanvasGroup component not found in FadeToBlack GameObject.");
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
        float startAlpha = panelCanvasGroup.alpha;
        float elapsedTime = 0f;

        while (elapsedTime < transitionDuration)
        {
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Lerp(startAlpha, targetAlpha, elapsedTime / transitionDuration);
            panelCanvasGroup.alpha = alpha;
            yield return null;
        }

        panelCanvasGroup.alpha = targetAlpha;
    }
}