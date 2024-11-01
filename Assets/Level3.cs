using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class Level3 : MonoBehaviour
{
    public Transform focusObject;
    public Transform cubePrefab;
    public Vector3 cubeSpawnPosition;
    public AudioSource backgroundMusicSource; // Changed to AudioSource
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
        // Step 1: Wait for 3 seconds while camera stays on the player
        yield return new WaitForSeconds(3f);

        // Step 2: Smoothly zoom in on the object for shaking
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

        // Step 3: Shake the camera for 2 seconds and start audio fade-in
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

        // Step 4: Make the object disappear and spawn a cube
        focusObject.gameObject.SetActive(false);
        cube = Instantiate(cubePrefab, cubeSpawnPosition, Quaternion.identity);

        // Remove cube's physics
        Rigidbody rb = cube.GetComponent<Rigidbody>();
        if (rb != null)
        {
            Destroy(rb);
        }

        // Drop the wall's Y size to 0 smoothly
        StartCoroutine(DropWallSize());

        // Change background color from black to red smoothly but quickly
        StartCoroutine(ChangeBackgroundColor());

        // Step 5: Smoothly return the camera to the player
        elapsedTime = 0f;

        while (elapsedTime < zoomDuration)
        {
            mainCamera.transform.position = Vector3.Lerp(zoomPosition, originalPosition, elapsedTime / zoomDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        mainCamera.transform.position = originalPosition;
        cameraReturned = true; // Set the flag to true when the camera has returned
        StartCoroutine(IncreaseCubeSpeed()); // Start increasing the cube's speed
    }

    void MoveCubeTowardsPlayer()
    {
        Vector3 direction = (player.position - cube.position).normalized;
        direction.y = 0; // Lock movement on the Y axis
        cube.position += direction * cubeMoveSpeed * Time.deltaTime;

        // Check for collision with player
        if (Vector3.Distance(cube.position, player.position) < 0.5f)
        {
            displayImage.enabled = true;
            cube = null; // Stop moving the cube
        }
    }

    IEnumerator DropWallSize()
    {
        float duration = 1f; // Duration to drop the wall size
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
        float duration = 0.5f; // Duration to change the color
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
        float duration = 3f; // Duration to increase speed
        float elapsedTime = 0f;
        float initialSpeed = 2f;
        float targetSpeed = cubeMoveSpeed; // Use the cubeMoveSpeed variable

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
}