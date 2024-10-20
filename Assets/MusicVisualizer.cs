using UnityEngine;
using UnityEngine.Rendering.Universal;

public class MusicVisualizer : MonoBehaviour
{
    [Header("Light Settings")]
    [SerializeField] private Light2D hexagonLight; // Reference to the Light2D component of the hexagon
    [SerializeField] private AudioSource audioSource; // Reference to the AudioSource component
    [SerializeField] private float intensityMultiplier = 2f; // Multiplier for visual intensity
    [SerializeField] private float smoothTime = 0.1f; // Time to smooth the intensity change

    private float targetIntensity; // Target intensity based on audio data
    private float currentIntensity; // Current intensity of the light
    private float velocity; // Used for smoothing

    private void Start()
    {
        if (hexagonLight == null)
        {
            Debug.LogError("Light2D component is not assigned in MusicVisualizer.");
        }

        if (audioSource == null)
        {
            Debug.LogError("AudioSource component is not assigned in MusicVisualizer.");
        }
    }

    private void Update()
    {
        // Get the audio spectrum data
        float[] spectrum = new float[256];
        audioSource.GetSpectrumData(spectrum, 0, FFTWindow.BlackmanHarris);

        // Calculate the intensity based on audio spectrum data
        float maxSpectrumValue = 0f;
        for (int i = 0; i < spectrum.Length; i++)
        {
            // Get the maximum value from the spectrum data
            if (spectrum[i] > maxSpectrumValue)
            {
                maxSpectrumValue = spectrum[i];
            }
        }

        // Calculate the target intensity based on the maximum spectrum value
        targetIntensity = maxSpectrumValue * intensityMultiplier;

        // Smoothly transition to the target intensity
        currentIntensity = Mathf.SmoothDamp(currentIntensity, targetIntensity, ref velocity, smoothTime);

        // Set the light intensity
        if (hexagonLight != null)
        {
            hexagonLight.intensity = currentIntensity;
        }
    }
}
