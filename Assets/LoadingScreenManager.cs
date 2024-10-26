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
    private AsyncOperation[] asyncLoads;

    void Start()
    {
        if (SceneManager.GetActiveScene().name == "Menu")
        {
            if (loadingScreen != null)
            {
                loadingScreen.SetActive(true);
            }

            if (progressTextObject != null)
            {
                progressText = progressTextObject.GetComponent<TMP_Text>();
            }

            asyncLoads = new AsyncOperation[scenesToLoad.Length];
            StartCoroutine(PreloadScenes());
        }
    }

    IEnumerator PreloadScenes()
    {
        for (int i = 0; i < scenesToLoad.Length; i++)
        {
            Debug.Log("Preloading scene: " + scenesToLoad[i]);
            asyncLoads[i] = SceneManager.LoadSceneAsync(scenesToLoad[i], LoadSceneMode.Additive);
            asyncLoads[i].allowSceneActivation = false; // Preload without activation

            while (asyncLoads[i].progress < 0.9f)
            {
                float progress = Mathf.Clamp01(asyncLoads[i].progress / 0.9f);
                progressBar.value = (i + progress) / scenesToLoad.Length;
                progressText.text = Mathf.RoundToInt(progressBar.value * 100f) + "%";
                Debug.Log($"Preloading progress for {scenesToLoad[i]}: {progress * 100f}%, Overall progress: {progressBar.value * 100f}%");

                yield return null;
            }

            Debug.Log("Scene preloaded: " + scenesToLoad[i]);
        }

        Debug.Log("All scenes preloaded. Ready to activate.");
        StartCoroutine(FadeOutLoadingScreen());
    }

    public void ActivateScene(int sceneIndex)
    {
        if (asyncLoads != null && sceneIndex < asyncLoads.Length)
        {
            asyncLoads[sceneIndex].allowSceneActivation = true;
        }
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
    }
}