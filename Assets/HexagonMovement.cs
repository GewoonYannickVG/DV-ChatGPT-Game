using System.Collections;
using System.Collections.Generic;
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

    [Header("Jump Settings")]
    [SerializeField] private int maxJumpCount = 2;

    private Rigidbody2D rb;
    private bool isGrounded = false;
    private int jumpCount = 0;
    private bool canDash = true;
    private bool dashTriggered = false;

    private Vector2 previousPosition;
    private float previousYPosition;
    private bool isMoving = false;

    private float moveCheckTimer = 0.2f; // Reduced interval for faster detection
    private float moveCheckInterval = 0.2f;

    // Reference to VolumeController
    private VolumeController volumeController;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 4.25f;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        previousPosition = rb.position;
        previousYPosition = rb.position.y;

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
        }

        // Get VolumeController reference
        volumeController = FindObjectOfType<VolumeController>();
    }

    void Update()
    {
        HandleRolling();
        HandleJump();
        HandleDashInput();

        previousPosition = rb.position;
    }

    void FixedUpdate()
    {
        GroundCheck();

        if (dashTriggered && canDash && !isGrounded)
        {
            PerformDash();
        }

        moveCheckTimer -= Time.fixedDeltaTime;

        if (moveCheckTimer <= 0f)
        {
            moveCheckTimer = moveCheckInterval;
            CheckMovement();
        }
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

        if (IsMovingAgainstWall(horizontalInput))
        {
            SetRollingAudioVolume(0f);
            return;
        }
    }

    private void CheckMovement()
    {
        float deltaX = Mathf.Abs(rb.position.x - previousPosition.x);
        float deltaY = Mathf.Abs(rb.position.y - previousYPosition);

        bool isPlayerMoving = Mathf.Abs(Input.GetAxis("Horizontal")) > 0.03f;
        bool hasMovedSignificantly = deltaX > 0.001f || deltaY > 0.003f;

        if (isPlayerMoving && isGrounded && hasMovedSignificantly)
        {
            if (!isMoving)
            {
                isMoving = true;
                PlayMoveAudio();
            }

            if (!rollAudioSource.isPlaying)
            {
                PlayRollingAudio();
            }
            else
            {
                SetRollingAudioVolume(1f);
            }
        }
        else
        {
            if (isMoving)
            {
                isMoving = false;
                SetRollingAudioVolume(0f);
            }
        }

        if (!hasMovedSignificantly)
        {
            SetRollingAudioVolume(0f);
        }

        previousYPosition = rb.position.y;
    }

    private bool IsMovingAgainstWall(float horizontalInput)
    {
        Vector2 direction = new Vector2(horizontalInput, 0);
        RaycastHit2D hit = Physics2D.Raycast(rb.position, direction, 0.1f, obstacleLayer);
        if (hit.collider != null)
        {
            SetRollingAudioVolume(0f);
            return true;
        }
        return false;
    }

    private void HandleJump()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (isGrounded || jumpCount < maxJumpCount)
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

            SetRollingAudioVolume(0f);
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

    private void PlayRollingAudio()
    {
        if (rollAudioSource != null && rollClip != null)
        {
            rollAudioSource.clip = rollClip;
            rollAudioSource.loop = true;
            rollAudioSource.volume = 1f;
            rollAudioSource.Play();
        }
    }

    private void SetRollingAudioVolume(float targetVolume)
    {
        if (rollAudioSource != null && rollAudioSource.isPlaying)
        {
            if (targetVolume == 0f)
            {
                rollAudioSource.Stop();
            }
            else
            {
                rollAudioSource.volume = targetVolume * GetCurrentVolume();
            }
        }
    }

    private void PlayMoveAudio()
    {
        if (moveAudioSource != null && movementClip != null && !moveAudioSource.isPlaying)
        {
            moveAudioSource.clip = movementClip;
            moveAudioSource.loop = true;
            moveAudioSource.volume = 1f;
            moveAudioSource.Play();
        }
    }

    private void StopMoveAudio()
    {
        if (moveAudioSource != null && moveAudioSource.isPlaying)
        {
            moveAudioSource.Stop();
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
            StartCoroutine(FadeToBlackAndSwitchScene("NextScene"));
        }
    }

    private IEnumerator FadeToBlackAndSwitchScene(string sceneName)
    {
        LightRadiusController lightController = FindObjectOfType<LightRadiusController>();
        if (lightController != null)
        {
            yield return StartCoroutine(lightController.FadeOutLights());
        }
        SceneTransitionManager.Instance.TransitionToScene(sceneName);
    }
}