using UnityEngine;
using UnityEngine.Audio;

public class ProximityAudioController : MonoBehaviour
{
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioMixerGroup audioMixerGroup;
    [SerializeField] private float triggerRadius = 10f;
    [SerializeField] private string volumeParameter = "Volume";
    [SerializeField] private string bassParameter = "LowPassFreq"; // Use a low-pass filter to simulate bass increase
    private float maxVolume = 0.6f; // Add this line to make the volume adjustable in the Unity editor

    private Transform playerTransform;
    private VolumeController volumeController;

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

        volumeController = VolumeController.Instance;
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
        float adjustedVolume = Mathf.Lerp(0f, maxVolume, proximityFactor) * volumeController.GetCurrentVolume();

        audioSource.volume = adjustedVolume;
        audioMixerGroup.audioMixer.SetFloat(volumeParameter, Mathf.Lerp(-80f, 0f, proximityFactor)); // Adjust volume
        audioMixerGroup.audioMixer.SetFloat(bassParameter, Mathf.Lerp(1000f, 22000f, proximityFactor)); // Adjust low-pass filter to increase bass

        // Calculate stereo pan based on the player's position relative to the audio source
        Vector3 direction = playerTransform.position - transform.position;
        float angle = Vector3.SignedAngle(transform.forward, direction, Vector3.up);
        audioSource.panStereo = Mathf.Clamp(angle / 90f, 0.75f, -0.75f); // Pan based on angle (-0.75 is left, 0.75 is right)
    }
}