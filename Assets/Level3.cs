using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.SceneManagement; // Add this line to access SceneManager

public class Level3 : MonoBehaviour
{
    public Transform focusObject;
    public Transform cubePrefab;
    public Vector3 cubeSpawnPosition;
    public AudioSource backgroundMusicSource; // Changed to AudioSource
    public AudioSource touchSoundSource; // Add this AudioSource for touch sound
    public Image displayImage;
    public Transform wall;
    public Image background;
    public Color wallColor = Color.red; // Expose the wall color as a public field

    public float cubeMoveSpeed = 2f; // Define the initial cubeMoveSpeed variable

    private Camera mainCamera;
    private Transform player;
    private bool cutscenePlayed = false;
    private Transform cube;
    private bool cameraReturned = false;

    void Start()
    {
        mainCamera = Camera.main;
        player = GameObject.FindWithTag("Player").transform;
        displayImage.enabled = false;

        // Apply the material color to the wall's renderer
        Renderer wallRenderer = wall.GetComponent<Renderer>();
        if (wallRenderer != null)
        {
            wallRenderer.material.color = wallColor;
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

        float shakeDuration = 2f;
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

        StartCoroutine(DropWallSize());
        StartCoroutine(ChangeBackgroundColor());

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
        Vector3 direction = (player.position - cube.position).normalized;
        direction.y = 0;
        cube.position += direction * cubeMoveSpeed * Time.deltaTime;

        // Check for collision with player
        if (Vector3.Distance(cube.position, player.position) < 0.5f)
        {
            StartCoroutine(OnCubeTouched()); // Call the new method when the cube is touched
            cube = null; // Stop moving the cube
        }
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

    IEnumerator ChangeBackgroundColor()
    {
        float duration = 0.5f;
        float elapsedTime = 0f;
        Color originalColor = Color.black;
        Color targetColor = Color.red;

        while (elapsedTime < duration)
        {
            background.color = Color.Lerp(originalColor, targetColor, elapsedTime / duration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        background.color = targetColor;
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

        backgroundMusicSource.volume = 1.5f;
    }

    IEnumerator OnCubeTouched()
    {
        // Play the touch sound
        if (touchSoundSource != null)
        {
            touchSoundSource.Play();
        }

        // Fade in the display image
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

        // Wait for 5 seconds before reloading the scene
        yield return new WaitForSeconds(5f);

        // Reload the scene
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}