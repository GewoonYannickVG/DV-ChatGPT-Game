using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
    private bool isMoving = false; // Flag to check if the player is moving
    private float rollVolume = 1f; // Volume level for rolling audio
    private float fadeDuration = 0.25f; // Fade duration for audio (slightly shorter)

    // Reference to VolumeController
    private VolumeController volumeController;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 4.25f;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        previousPosition = rb.position;

        // Initialize audio sources
        if (moveAudioSource != null)
        {
            moveAudioSource.clip = movementClip;
            moveAudioSource.loop = true; // Loop movement audio
            moveAudioSource.volume = 0f; // Start volume at 0
        }

        if (jumpAudioSource != null)
        {
            jumpAudioSource.clip = jumpClip; // Set jump audio clip
        }

        if (dashAudioSource != null)
        {
            dashAudioSource.clip = dashClip; // Set dash audio clip
        }

        if (rollAudioSource != null)
        {
            rollAudioSource.clip = rollClip; // Set roll audio clip
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

        // Check if the player is moving against a wall
        if (IsMovingAgainstWall(horizontalInput))
        {
            SetRollingAudioVolume(0f); // Set volume to 0 when against a wall
            return; // Prevent further processing if stuck against wall
        }

        // Rolling logic to play sound smoothly
        if (Mathf.Abs(horizontalInput) > 0.1f && isGrounded)
        {
            float distanceMoved = Vector2.Distance(previousPosition, rb.position);

            // Check if movement has occurred
            if (distanceMoved > 0.01f)
            {
                // Only play movement audio once when the player starts moving
                if (!isMoving)
                {
                    isMoving = true;
                    PlayMoveAudio(); // Movement audio (plays once when moving starts)
                }

                // Manage rolling audio
                if (!rollAudioSource.isPlaying)
                {
                    PlayRollingAudio(); // Start rolling audio when moving
                }
                else
                {
                    // Ensure rolling audio is at the correct volume
                    SetRollingAudioVolume(rollVolume);
                }
            }
        }
        else
        {
            // If the player stops moving, set rolling audio volume to 0
            if (isMoving)
            {
                isMoving = false;
                SetRollingAudioVolume(0f); // Stop rolling audio smoothly
            }
        }
    }

    private bool IsMovingAgainstWall(float horizontalInput)
    {
        // Check if moving horizontally against a wall
        Vector2 direction = new Vector2(horizontalInput, 0);
        RaycastHit2D hit = Physics2D.Raycast(rb.position, direction, 0.1f, obstacleLayer);
        if (hit.collider != null)
        {
            SetRollingAudioVolume(0f); // Stop rolling audio when hitting a wall
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

            // Stop rolling audio if jumping
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

    // Rolling Audio Logic
    private void PlayRollingAudio()
    {
        if (rollAudioSource != null && rollClip != null)
        {
            rollAudioSource.clip = rollClip;
            rollAudioSource.loop = true; // Ensure rolling audio is looped
            rollAudioSource.volume = 0f; // Start volume at 0 for fade-in
            rollAudioSource.Play();
            StartCoroutine(FadeInAudio(rollAudioSource, fadeDuration, rollVolume * GetCurrentVolume())); // Fade in over fadeDuration

            // Set the volume right after starting the audio to ensure it respects the global volume
            rollAudioSource.volume = rollVolume * GetCurrentVolume();
        }
    }

    private void SetRollingAudioVolume(float targetVolume)
    {
        if (rollAudioSource != null && rollAudioSource.isPlaying)
        {
            if (targetVolume == 0f)
            {
                StartCoroutine(FadeOutAudio(rollAudioSource, fadeDuration)); // Fade out over fadeDuration
            }
            else
            {
                // Set volume based on global settings whenever it’s being set
                rollAudioSource.volume = targetVolume * GetCurrentVolume();
            }
        }
    }

    // Movement Audio Logic
    private void PlayMoveAudio()
    {
        if (moveAudioSource != null && movementClip != null && !moveAudioSource.isPlaying)
        {
            moveAudioSource.clip = movementClip;
            moveAudioSource.loop = true; // Loop movement audio
            moveAudioSource.volume = 0f; // Start volume at 0 for fade-in
            moveAudioSource.Play();
            StartCoroutine(FadeInAudio(moveAudioSource, fadeDuration, 1f)); // Fade in over fadeDuration
        }
    }

    private void StopMoveAudio()
    {
        if (moveAudioSource != null && moveAudioSource.isPlaying)
        {
            StartCoroutine(FadeOutAudio(moveAudioSource, fadeDuration)); // Fade out over fadeDuration
        }
    }

    // Fade in/out coroutine
    private IEnumerator FadeInAudio(AudioSource audioSource, float duration, float targetVolume)
    {
        float startVolume = audioSource.volume;
        audioSource.volume = 0f;

        while (audioSource.volume < targetVolume)
        {
            audioSource.volume += startVolume * Time.deltaTime / duration;
            yield return null;
        }

        audioSource.volume = targetVolume; // Ensure final volume is set
    }

    private IEnumerator FadeOutAudio(AudioSource audioSource, float duration)
    {
        float startVolume = audioSource.volume;

        while (audioSource.volume > 0f)
        {
            audioSource.volume -= startVolume * Time.deltaTime / duration;
            yield return null;
        }

        audioSource.Stop();
    }

    private void PlayJumpAudio()
    {
        if (jumpAudioSource != null && jumpClip != null)
        {
            jumpAudioSource.pitch = Random.Range(0.9f, 1.1f); // Random pitch variation
            jumpAudioSource.PlayOneShot(jumpClip);
        }
    }

    private void PlayDashAudio()
    {
        if (dashAudioSource != null && dashClip != null)
        {
            dashAudioSource.pitch = Random.Range(0.9f, 1.1f); // Random pitch variation
            dashAudioSource.PlayOneShot(dashClip);
        }
    }

    private void PlayDashParticles(Vector2 position, float dashDirection)
    {
        GameObject dashParticles = Instantiate(dashParticlePrefab, position, Quaternion.identity);
        dashParticles.transform.localScale = new Vector3(dashDirection > 0 ? 1 : -1, 1, 1);
        Destroy(dashParticles, 1f); // Destroy particles after 1 second
    }

    private IEnumerator ShakeCamera()
    {
        // Camera shake logic here
        yield return new WaitForSeconds(0.2f);
    }

    // Method to get current audio volume settings (can be customized)
    private float GetCurrentVolume()
    {
        // Use global volume/mute setting from VolumeController
        if (volumeController != null)
        {
            return volumeController.GetCurrentVolume();
        }
        return 1f; // Default volume if VolumeController is not found
    }
}