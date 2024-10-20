using System.Collections;
using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance; // Singleton instance of AudioManager

    [Header("Audio Sources")]
    public AudioSource backgroundMusic; // Reference to the background music AudioSource
    public AudioSource buildUpSound;    // Reference to the build-up sound AudioSource

    [Header("Audio Mixer")]
    public AudioMixerGroup reverbMixerGroup; // Reference to the audio mixer group for reverb effects

    [Header("Fade Settings")]
    public float fadeDuration = 2f; // Duration of the background music fade-out

    private void Awake()
    {
        // Implementing singleton pattern
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);  // Make this object persist across scenes
        }
        else
        {
            Destroy(gameObject);  // Destroy duplicate AudioManager
        }
    }

    private void Start()
    {
        // Ensure that no audio is played on start unintentionally
        if (buildUpSound != null)
        {
            buildUpSound.Stop();  // Ensure BuildUpSound is not playing at the start
        }
    }

    // Method to play build-up sound when the button is pressed
    public void PlayBuildUpSound()
    {
        if (buildUpSound != null)
        {
            Debug.Log("BuildUpSound is being played.");
            buildUpSound.outputAudioMixerGroup = reverbMixerGroup;  // Apply reverb effect
            if (!buildUpSound.isPlaying)   // Ensure it only plays if it isn't already playing
            {
                buildUpSound.Play();
            }
        }
    }

    // Method to fade out the background music over time
    public IEnumerator FadeOutBackgroundMusic()
    {
        if (backgroundMusic != null)
        {
            float startVolume = backgroundMusic.volume;

            // Gradually decrease the volume
            for (float t = 0; t < fadeDuration; t += Time.deltaTime)
            {
                backgroundMusic.volume = Mathf.Lerp(startVolume, 0, t / fadeDuration);
                yield return null;
            }

            // Ensure the volume is completely off
            backgroundMusic.volume = 0;
            backgroundMusic.Stop();
        }
    }

    // Method to stop the build-up sound manually when desired (e.g., after a delay)
    public IEnumerator StopBuildUpSound(float delay)
    {
        yield return new WaitForSeconds(delay);  // Wait for a specific time (optional)
        if (buildUpSound != null)
        {
            buildUpSound.Stop();  // Stop build-up sound after the scene transition or a specific delay
        }
    }
}