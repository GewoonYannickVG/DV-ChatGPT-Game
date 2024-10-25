using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class SceneTransitionManager : MonoBehaviour
{
    public static SceneTransitionManager Instance { get; private set; }
    private DiamondController diamondController;

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
    }

    public void TransitionToNextScene()
    {
        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
        int nextSceneIndex = (currentSceneIndex + 1) % SceneManager.sceneCountInBuildSettings;
        StartCoroutine(Transition(nextSceneIndex));
    }

    private IEnumerator Transition(int sceneIndex)
    {
        diamondController = FindObjectOfType<DiamondController>();
        if (diamondController != null)
        {
            yield return StartCoroutine(diamondController.FadeOutLights());
        }

        yield return new WaitForSeconds(1f); // Simulate transition delay
        SceneManager.LoadScene(sceneIndex);
    }
}