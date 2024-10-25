using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class SceneTransitionManager : MonoBehaviour
{
    public static SceneTransitionManager Instance { get; private set; }

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

    public void TransitionToScene(string Level1)
    {
        StartCoroutine(Transition(Level1));
    }

    private IEnumerator Transition(string Level1)
    {
        // Add any transition effects here (e.g., fade out)
        yield return new WaitForSeconds(1f); // Simulate transition delay
        SceneManager.LoadScene(Level1);
    }
}