using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class Level3 : MonoBehaviour
{
    public Transform focusObject;
    public Transform cubePrefab;
    public Vector3 cubeSpawnPosition;
    public AudioClip backgroundMusic;
    public Image displayImage;

    private Camera mainCamera;
    private Transform player;
    private AudioSource audioSource;
    private bool cutscenePlayed = false;

    void Start()
    {
        mainCamera = GameObject.Find("Main Camera").GetComponent<Camera>();
        player = GameObject.Find("Player").transform;
        audioSource = GetComponent<AudioSource>();
        displayImage.enabled = false;
    }

    void Update()
    {
        if (!cutscenePlayed)
        {
            StartCoroutine(PlayCutscene());
            cutscenePlayed = true;
        }
    }

    IEnumerator PlayCutscene()
    {
        // Step 1: Smoothly zoom in on the object
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

        // Step 2: Wait for 3 seconds
        yield return new WaitForSeconds(3f);

        // Step 3: Shake the camera for 2 seconds
        float shakeDuration = 2f;
        float shakeMagnitude = 0.1f;
        elapsedTime = 0f;

        while (elapsedTime < shakeDuration)
        {
            float xOffset = Random.Range(-1f, 1f) * shakeMagnitude * (elapsedTime / shakeDuration);
            float yOffset = Random.Range(-1f, 1f) * shakeMagnitude * (elapsedTime / shakeDuration);
            mainCamera.transform.position = new Vector3(zoomPosition.x + xOffset, zoomPosition.y + yOffset, zoomPosition.z);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        mainCamera.transform.position = zoomPosition;

        // Step 4: Make the object disappear and spawn a cube
        focusObject.gameObject.SetActive(false);
        Transform cube = Instantiate(cubePrefab, cubeSpawnPosition, Quaternion.identity);

        // Step 5: Return the camera to the player and play music
        elapsedTime = 0f;

        while (elapsedTime < zoomDuration)
        {
            mainCamera.transform.position = Vector3.Lerp(zoomPosition, originalPosition, elapsedTime / zoomDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        mainCamera.transform.position = originalPosition;
        audioSource.clip = backgroundMusic;
        audioSource.Play();

        // Step 6: Move the cube to the right
        float moveSpeed = 2f;

        while (true)
        {
            cube.Translate(Vector3.right * moveSpeed * Time.deltaTime);

            // Step 7: Check for collision with player
            if (Vector3.Distance(cube.position, player.position) < 0.5f)
            {
                displayImage.enabled = true;
                break;
            }

            yield return null;
        }
    }
}