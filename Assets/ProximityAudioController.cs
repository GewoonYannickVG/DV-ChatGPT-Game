using UnityEngine;
using UnityEngine.Audio;

public class ProximityAudioController : MonoBehaviour
{
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioMixerGroup audioMixerGroup;
    [SerializeField] private float triggerRadius = 10f;
    [SerializeField] private string volumeParameter = "Volume";
    [SerializeField] private string bassParameter = "LowPassFreq"; // Use a low-pass filter to simulate bass increase

    private Transform playerTransform;

    void Start()
    {
        GameObject playerObj = GameObject.Find("Player");
        if (playerObj != null)
        {
            playerTransform = playerObj.transform;
        }
        else
        {
            Debug.LogError("Player object not found in the scene.");
        }

        if (audioSource == null)
        {
            Debug.LogError("AudioSource is not assigned in ProximityAudioController.");
        }
        else
        {
            audioSource.outputAudioMixerGroup = audioMixerGroup;
            audioSource.volume = 0f; // Start with volume at 0
        }

        if (audioMixerGroup == null)
        {
            Debug.LogError("AudioMixerGroup is not assigned in ProximityAudioController.");
        }
    }

    void Update()
    {
        if (playerTransform == null) return;

        float distance = Vector3.Distance(transform.position, playerTransform.position);
        if (distance <= triggerRadius)
        {
            if (!audioSource.isPlaying)
            {
                audioSource.loop = true;
                audioSource.Play();
            }
            AdjustAudioParameters(distance);
        }
        else
        {
            if (audioSource.isPlaying)
            {
                audioSource.Stop();
            }
        }
    }

    private void AdjustAudioParameters(float distance)
    {
        float proximityFactor = 1 - (distance / triggerRadius);

        audioSource.volume = Mathf.Lerp(0f, 1f, proximityFactor); // Smoothly fade in volume
        audioMixerGroup.audioMixer.SetFloat(volumeParameter, Mathf.Lerp(-80f, 0f, proximityFactor)); // Adjust volume
        audioMixerGroup.audioMixer.SetFloat(bassParameter, Mathf.Lerp(1000f, 22000f, proximityFactor)); // Adjust low-pass filter to increase bass
    }
}