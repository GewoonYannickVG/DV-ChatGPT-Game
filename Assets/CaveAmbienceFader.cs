using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class CaveAmbienceFader : MonoBehaviour
{
    public AudioSource caveAmbienceAudioSource; // Reference to the CaveAmbience AudioSource
    public float fadeDuration = 2.0f; // Duration of the fade-in effect

    private void Start()
    {
        // Ensure the audio source is not playing at the start
        caveAmbienceAudioSource.volume = 0f;
        caveAmbienceAudioSource.loop = true;
        caveAmbienceAudioSource.Play();

        // Start the coroutine to fade in the audio after a delay
        StartCoroutine(FadeInCaveAmbience());
    }

    private IEnumerator FadeInCaveAmbience()
    {
        // Wait for 2 seconds before starting the fade-in
        yield return new WaitForSeconds(2.0f);

        float elapsedTime = 0f;

        while (elapsedTime < fadeDuration)
        {
            // Calculate the new volume
            caveAmbienceAudioSource.volume = Mathf.Lerp(0f, 1f, elapsedTime / fadeDuration);

            // Increment the elapsed time
            elapsedTime += Time.deltaTime;

            // Wait for the next frame
            yield return null;
        }

        // Ensure the volume is set to 1 after the fade-in completes
        caveAmbienceAudioSource.volume = 1f;
    }
}