using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class LoadingScreenManager : MonoBehaviour
{
    public GameObject loadingScreen; // Should be Panel GameObject
    public Slider progressBar; // Should be Slider UI element
    public GameObject progressTextObject; // Should be Text Mesh Pro UI element

    public string[] scenesToLoad;

    private TMP_Text progressText;

    void Start()
    {
        if (loadingScreen != null)
        {
            loadingScreen.SetActive(true);
        }

        if (progressTextObject != null)
        {
            progressText = progressTextObject.GetComponent<TMP_Text>();
        }

        StartCoroutine(PreloadScenes());
    }

    IEnumerator PreloadScenes()
    {
        for (int i = 0; i < scenesToLoad.Length; i++)
        {
            Debug.Log("Preloading scene: " + scenesToLoad[i]);
            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(scenesToLoad[i], LoadSceneMode.Additive);
            asyncLoad.allowSceneActivation = false; // Preload without activation

            while (!asyncLoad.isDone)
            {
                float progress = Mathf.Clamp01(asyncLoad.progress / 0.9f);
                progressBar.value = (i + progress) / scenesToLoad.Length;
                progressText.text = Mathf.RoundToInt(progressBar.value * 100f) + "%";
                Debug.Log($"Preloading progress for {scenesToLoad[i]}: {progress * 100f}%, Overall progress: {progressBar.value * 100f}%");

                if (progress >= 0.9f)
                {
                    Debug.Log("Scene almost ready for activation: " + scenesToLoad[i]);
                    break; // Exit loop to allow scene activation
                }

                yield return null;
            }

            asyncLoad.allowSceneActivation = true;
            Debug.Log("Scene activated: " + scenesToLoad[i]);

            // Wait until the scene is fully activated
            while (!asyncLoad.isDone)
            {
                yield return null;
            }

            Debug.Log("Scene fully loaded: " + scenesToLoad[i]);
        }

        Debug.Log("All scenes preloaded. Ready to activate.");
        StartCoroutine(FadeOutLoadingScreen());
    }

    public void ActivateScene(string sceneName)
    {
        Debug.Log("Activating scene: " + sceneName);
        SceneManager.LoadScene(sceneName, LoadSceneMode.Single); // Activate scene when needed
    }

    IEnumerator FadeOutLoadingScreen()
    {
        CanvasGroup canvasGroup = loadingScreen.GetComponent<CanvasGroup>();
        if (canvasGroup != null)
        {
            while (canvasGroup.alpha > 0)
            {
                canvasGroup.alpha -= Time.deltaTime;
                yield return null;
            }
        }

        loadingScreen.SetActive(false);
        Debug.Log("Loading screen faded out.");
    }
}