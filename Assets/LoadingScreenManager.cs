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

        StartCoroutine(LoadAllScenes());
    }

    IEnumerator LoadAllScenes()
    {
        Debug.Log("Starting to load scenes...");
        for (int i = 0; i < scenesToLoad.Length; i++)
        {
            Debug.Log("Loading scene: " + scenesToLoad[i]);
            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(scenesToLoad[i], LoadSceneMode.Additive);
            asyncLoad.allowSceneActivation = false;

            while (!asyncLoad.isDone)
            {
                float progress = Mathf.Clamp01(asyncLoad.progress / 0.9f);
                progressBar.value = (i + progress) / scenesToLoad.Length;
                progressText.text = Mathf.RoundToInt(progressBar.value * 100f) + "%";
                Debug.Log($"Scene: {scenesToLoad[i]}, Progress: {progress * 100f}%, Total Progress: {progressBar.value * 100f}%");

                // Check if the operation is stuck
                if (progress >= 0.9f)
                {
                    Debug.Log($"Scene {scenesToLoad[i]} is ready to activate.");
                    asyncLoad.allowSceneActivation = true;
                }

                yield return null;
            }

            Debug.Log("Scene loaded: " + scenesToLoad[i]);
        }

        yield return new WaitForSeconds(1f); // Ensure all scenes are activated

        StartCoroutine(FadeOutLoadingScreen());
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