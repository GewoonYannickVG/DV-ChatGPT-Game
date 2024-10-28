using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class HexagonMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float torqueAmount = 100f;
    [SerializeField] private float movementForce = 5f;
    [SerializeField] private float jumpForce = 10f;
    [SerializeField] private float doubleJumpForce = 15f;
    [SerializeField] private float dashForce = 300f;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private LayerMask obstacleLayer;
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckRadius = 0.1f;
    [SerializeField] private float dashRayDistance = 1f;

    [Header("Audio Settings")]
    [SerializeField] private AudioSource moveAudioSource;
    [SerializeField] private AudioClip movementClip;
    [SerializeField] private AudioSource jumpAudioSource;
    [SerializeField] private AudioClip jumpClip;
    [SerializeField] private AudioSource dashAudioSource;
    [SerializeField] private AudioClip dashClip;
    [SerializeField] private AudioSource rollAudioSource;
    [SerializeField] private AudioClip rollClip;
    [SerializeField] private GameObject dashParticlePrefab;
    [SerializeField] private float audioFadeDuration = 0.5f;

    [Header("Jump Settings")]
    [SerializeField] private int maxJumpCount = 2;

    private Rigidbody2D rb;
    private bool isGrounded = false;
    private int jumpCount = 0;
    private bool canDash = true;
    private bool dashTriggered = false;
    private Coroutine fadeCoroutine;
    public bool isInNoDoubleJumpZone = false; // Changed to public

    // Reference to VolumeController
    private VolumeController volumeController;
    private bool isTouchingWall = false;

    private void Awake()
    {
        DontDestroyOnLoad(gameObject); // Persist across scenes
    }

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 4.25f;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        // Initialize audio sources
        if (moveAudioSource != null)
        {
            moveAudioSource.clip = movementClip;
            moveAudioSource.loop = true;
            moveAudioSource.volume = 0f;
        }

        if (jumpAudioSource != null)
        {
            jumpAudioSource.clip = jumpClip;
        }

        if (dashAudioSource != null)
        {
            dashAudioSource.clip = dashClip;
        }

        if (rollAudioSource != null)
        {
            rollAudioSource.clip = rollClip;
            rollAudioSource.volume = 0.2f; // Set default volume to 0.2
        }

        // Get VolumeController reference
        volumeController = FindObjectOfType<VolumeController>();
    }

    void Update()
    {
        HandleRolling();
        HandleJump();
        HandleDashInput();
    }

    void FixedUpdate()
    {
        GroundCheck();

        if (dashTriggered && canDash && !isGrounded)
        {
            PerformDash();
        }

        HandlePlatformCollision();
    }

    private void GroundCheck()
    {
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

        if (isGrounded)
        {
            jumpCount = 0;
            canDash = true;
            dashTriggered = false;
        }
    }

    private void HandleRolling()
    {
        float horizontalInput = Input.GetAxis("Horizontal");

        rb.velocity = new Vector2(horizontalInput * movementForce, rb.velocity.y);

        float speedFactor = Mathf.Abs(rb.velocity.x);
        float torqueMultiplier = 10f;

        if (horizontalInput < 0)
        {
            rb.AddTorque(torqueMultiplier * torqueAmount * speedFactor * Time.fixedDeltaTime);
        }
        else if (horizontalInput > 0)
        {
            rb.AddTorque(-torqueMultiplier * torqueAmount * speedFactor * Time.fixedDeltaTime);
        }

        float maxAngularVelocity = 50f;
        rb.angularVelocity = Mathf.Clamp(rb.angularVelocity, -maxAngularVelocity, maxAngularVelocity);

        if (isGrounded && Mathf.Abs(horizontalInput) > 0.03f && !isTouchingWall)
        {
            PlayRollingAudio();
        }
        else
        {
            FadeOutRollingAudio();
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Wall"))
        {
            isTouchingWall = true;
            FadeOutRollingAudio();
        }
    }

    void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Wall"))
        {
            isTouchingWall = false;
        }
    }

    private void HandleJump()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (isGrounded || (jumpCount < maxJumpCount && !isInNoDoubleJumpZone))
            {
                if (isGrounded)
                {
                    rb.velocity = new Vector2(rb.velocity.x, jumpForce);
                    jumpCount++;
                }
                else
                {
                    rb.velocity = new Vector2(rb.velocity.x, doubleJumpForce);
                    jumpCount++;
                }

                PlayJumpAudio();
            }

            FadeOutRollingAudio();
        }
    }

    private void HandleDashInput()
    {
        if (Input.GetKey(KeyCode.W) && !isGrounded)
        {
            if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.D))
            {
                dashTriggered = true;
            }
        }
    }

    private void PerformDash()
    {
        float dashDirection = Input.GetKey(KeyCode.A) ? -1 : 1;

        Vector2 rayOrigin = rb.position;
        Vector2 dashDirectionVector = new Vector2(dashDirection, 0);

        RaycastHit2D hit = Physics2D.Raycast(rayOrigin, dashDirectionVector, dashRayDistance, obstacleLayer);

        if (!hit)
        {
            rb.AddForce(dashDirectionVector * dashForce, ForceMode2D.Impulse);
            PlayDashAudio();
            PlayDashParticles(rb.position, dashDirection);
            canDash = false;
        }

        StartCoroutine(ShakeCamera());
    }

    private void HandlePlatformCollision()
    {
        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, 1f, groundLayer);
        if (hit.collider != null && hit.collider.CompareTag("OneWayPlatform"))
        {
            Physics2D.IgnoreCollision(GetComponent<Collider2D>(), hit.collider, true);
        }
        else
        {
            Physics2D.IgnoreCollision(GetComponent<Collider2D>(), hit.collider, false);
        }
    }

    private void PlayRollingAudio()
    {
        if (rollAudioSource != null && rollClip != null && !rollAudioSource.isPlaying)
        {
            rollAudioSource.clip = rollClip;
            rollAudioSource.loop = true;
            rollAudioSource.volume = 0.2f * GetCurrentVolume(); // Default volume set to 0.2 and respects volume changer
            rollAudioSource.Play();
            FadeInAudio(rollAudioSource);
        }
    }

    private void FadeInAudio(AudioSource audioSource)
    {
        if (fadeCoroutine != null)
        {
            StopCoroutine(fadeCoroutine);
        }
        fadeCoroutine = StartCoroutine(FadeAudio(audioSource, audioSource.volume, 1f * GetCurrentVolume(), audioFadeDuration));
    }

    private void FadeOutRollingAudio()
    {
        if (rollAudioSource != null && rollAudioSource.isPlaying)
        {
            if (fadeCoroutine != null)
            {
                StopCoroutine(fadeCoroutine);
            }
            fadeCoroutine = StartCoroutine(FadeAudio(rollAudioSource, rollAudioSource.volume, 0f, audioFadeDuration / 2));
        }
    }

    private IEnumerator FadeAudio(AudioSource audioSource, float startVolume, float targetVolume, float duration)
    {
        float currentTime = 0f;
        while (currentTime < duration)
        {
            currentTime += Time.deltaTime;
            audioSource.volume = Mathf.Lerp(startVolume, targetVolume, currentTime / duration);
            yield return null;
        }
        if (targetVolume == 0f)
        {
            audioSource.Stop();
        }
    }

    private void PlayJumpAudio()
    {
        if (jumpAudioSource != null && jumpClip != null)
        {
            jumpAudioSource.pitch = Random.Range(0.9f, 1.1f);
            jumpAudioSource.PlayOneShot(jumpClip);
        }
    }

    private void PlayDashAudio()
    {
        if (dashAudioSource != null && dashClip != null)
        {
            dashAudioSource.pitch = Random.Range(0.9f, 1.1f);
            dashAudioSource.PlayOneShot(dashClip);
        }
    }

    private void PlayDashParticles(Vector2 position, float dashDirection)
    {
        GameObject dashParticles = Instantiate(dashParticlePrefab, position, Quaternion.identity);
        dashParticles.transform.localScale = new Vector3(dashDirection > 0 ? 1 : -1, 1, 1);
        Destroy(dashParticles, 1f);
    }

    private IEnumerator ShakeCamera()
    {
        yield return new WaitForSeconds(0.2f);
    }

    private float GetCurrentVolume()
    {
        if (volumeController != null)
        {
            return volumeController.GetCurrentVolume();
        }
        return 1f;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Diamond"))
        {
            StartCoroutine(FadeToBlackAndSwitchScene());
        }
        else if (other.CompareTag("NoDoubleJumpZone"))
        {
            isInNoDoubleJumpZone = true; // Entering no double jump zone
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("NoDoubleJumpZone"))
        {
            isInNoDoubleJumpZone = false; // Exiting no double jump zone
        }
    }

    private IEnumerator FadeToBlackAndSwitchScene()
    {
        yield return new WaitForSeconds(0.2f); // Add a delay if needed

        SceneTransitionManager.Instance.TransitionToNextScene();
    }
}