using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.SceneManagement;

public class Level3 : MonoBehaviour
{
    public Transform focusObject;
    public Transform cubePrefab;
    public Vector3 cubeSpawnPosition;
    public AudioSource backgroundMusicSource;
    public AudioSource touchSoundSource;
    public Image displayImage;
    public Transform wall;
    public Image background;
    public float backgroundMusicSpeed = 1f; // New variable for setting audio speed
    public float cubeMoveSpeed = 2f;

    private Camera mainCamera;
    private Transform player;
    private bool cutscenePlayed = false;
    private Transform cube;
    private bool cameraReturned = false;

    private float[] audioSamples = new float[64];
    private float redIntensity = 0f;

    void Start()
    {
        mainCamera = Camera.main;
        player = GameObject.FindWithTag("Player").transform;
        displayImage.enabled = false;

        // Set the initial audio speed and match the pitch
        if (backgroundMusicSource != null && backgroundMusicSource.clip != null)
        {
            backgroundMusicSource.pitch = backgroundMusicSpeed;
            backgroundMusicSource.volume = 1f; // Ensure volume is set
        }

        Renderer wallRenderer = wall.GetComponent<Renderer>();
        if (wallRenderer != null)
        {
            wallRenderer.material.color = Color.white;  // Ensure wall is not red
        }
    }

    void Update()
    {
        if (!cutscenePlayed)
        {
            StartCoroutine(PlayCutscene());
            cutscenePlayed = true;
        }
        else if (cube != null && cameraReturned)
        {
            MoveCubeTowardsPlayer();
        }

        UpdateBackgroundColor();
    }

    IEnumerator PlayCutscene()
    {
        yield return new WaitForSeconds(3f);

        float zoomDuration = 2f;
        Vector3 originalPosition = mainCamera.transform.position;
        Vector3 zoomPosition = new Vector3(focusObject.position.x, focusObject.position.y, mainCamera.transform.position.z);
        float elapsedTime = 0f;

        while (elapsedTime < zoomDuration)
        {
            mainCamera.transform.position = Vector3.Lerp(originalPosition, zoomPosition, elapsedTime / zoomDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        mainCamera.transform.position = zoomPosition;

        float shakeDuration = 1.5f;
        float shakeMagnitude = 0.1f;
        elapsedTime = 0f;

        StartCoroutine(FadeInAudio(shakeDuration));

        while (elapsedTime < shakeDuration)
        {
            float xOffset = Random.Range(-1f, 1f) * shakeMagnitude;
            float yOffset = Random.Range(-1f, 1f) * shakeMagnitude;
            mainCamera.transform.position = new Vector3(zoomPosition.x + xOffset, zoomPosition.y + yOffset, zoomPosition.z);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        mainCamera.transform.position = zoomPosition;

        focusObject.gameObject.SetActive(false);
        cube = Instantiate(cubePrefab, cubeSpawnPosition, Quaternion.identity);

        Rigidbody rb = cube.GetComponent<Rigidbody>();
        if (rb != null)
        {
            Destroy(rb);
        }

        // Ensure the cube has a BoxCollider only if it doesn't already have one

        StartCoroutine(DropWallSize());  // Ensure wall size smoothly transitions

        elapsedTime = 0f;

        while (elapsedTime < zoomDuration)
        {
            mainCamera.transform.position = Vector3.Lerp(zoomPosition, originalPosition, elapsedTime / zoomDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        mainCamera.transform.position = originalPosition;
        cameraReturned = true;
        StartCoroutine(IncreaseCubeSpeed());
    }

    void MoveCubeTowardsPlayer()
    {
        if (cube == null || player == null) return;

        Vector3 direction = (player.position - cube.position).normalized;
        direction.y = 0;
        cube.position += direction * cubeMoveSpeed * Time.deltaTime;
    }

    IEnumerator DropWallSize()
    {
        float duration = 1f;
        float elapsedTime = 0f;
        Vector3 originalScale = wall.localScale;
        Vector3 targetScale = new Vector3(originalScale.x, 0f, originalScale.z);

        while (elapsedTime < duration)
        {
            wall.localScale = Vector3.Lerp(originalScale, targetScale, elapsedTime / duration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        wall.localScale = targetScale;
    }

    IEnumerator IncreaseCubeSpeed()
    {
        float duration = 3f;
        float elapsedTime = 0f;
        float initialSpeed = 2f;
        float targetSpeed = cubeMoveSpeed;

        while (elapsedTime < duration)
        {
            cubeMoveSpeed = Mathf.Lerp(initialSpeed, targetSpeed, elapsedTime / duration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        cubeMoveSpeed = targetSpeed;
    }

    IEnumerator FadeInAudio(float duration)
    {
        backgroundMusicSource.volume = 0f;
        backgroundMusicSource.Play();

        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            backgroundMusicSource.volume = Mathf.Lerp(0f, 1f, elapsedTime / duration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        backgroundMusicSource.volume = 1f;
    }

    IEnumerator FadeOutAudio(float duration)
    {
        float startVolume = backgroundMusicSource.volume;
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            backgroundMusicSource.volume = Mathf.Lerp(startVolume, 0f, elapsedTime / duration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        backgroundMusicSource.volume = 0f;
        backgroundMusicSource.Stop();
    }

    IEnumerator OnCubeTouched()
    {
        yield return StartCoroutine(FadeOutAudio(1f));

        if (touchSoundSource != null)
        {
            touchSoundSource.Play();
        }

        displayImage.enabled = true;
        Color imgColor = displayImage.color;
        imgColor.a = 0;
        displayImage.color = imgColor;

        float fadeDuration = 1f;
        float elapsedTime = 0f;

        while (elapsedTime < fadeDuration)
        {
            imgColor.a = Mathf.Lerp(0, 1, elapsedTime / fadeDuration);
            displayImage.color = imgColor;
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        imgColor.a = 1;
        displayImage.color = imgColor;

        yield return new WaitForSeconds(5f);

        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.CompareTag("Player"))
        {
            StartCoroutine(OnCubeTouched());
        }
    }

    void UpdateBackgroundColor()
    {
        backgroundMusicSource.GetSpectrumData(audioSamples, 0, FFTWindow.BlackmanHarris);
        float averageAmplitude = 0f;

        for (int i = 0; i < audioSamples.Length; i++)
        {
            averageAmplitude += audioSamples[i];
        }

        averageAmplitude /= audioSamples.Length;
        redIntensity = Mathf.Lerp(redIntensity, averageAmplitude * 25f, Time.deltaTime * 15f);  // Adjusted threshold and smoothness
        redIntensity = Mathf.Clamp01(redIntensity);

        Color currentColor = background.color;
        currentColor.r = redIntensity;
        background.color = currentColor;
    }
}