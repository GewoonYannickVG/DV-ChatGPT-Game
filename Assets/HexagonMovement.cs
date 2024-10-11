using UnityEngine;
using System.Collections; // Make sure to include this for IEnumerator

public class HexagonMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float torqueAmount = 100f;         // Amount of torque to apply when moving
    [SerializeField] private float movementForce = 5f;          // Force to apply to move the hexagon forward/backward
    [SerializeField] private float jumpForce = 10f;             // Jump force applied as a direct velocity change
    [SerializeField] private float doubleJumpForce = 15f;       // Extra jump force applied for the second jump
    [SerializeField] private float dashForce = 20f;             // Force applied when dashing
    [SerializeField] private LayerMask groundLayer;             // Layer to detect ground collisions
    [SerializeField] private LayerMask obstacleLayer;           // Layer for detecting obstacles before dashing
    [SerializeField] private Transform groundCheck;             // Reference to the GroundCheck object
    [SerializeField] private float groundCheckRadius = 0.1f;    // Radius of the ground check circle
    [SerializeField] private float dashRayDistance = 1f;        // Distance for dash obstacle detection

    // Add a reference for the particle system prefab
    [SerializeField] private GameObject dashParticlePrefab;     // Prefab for dash particle effect

    [Header("Jump Settings")]
    [SerializeField] private int maxJumpCount = 2;              // Maximum number of jumps (1 jump + 1 double jump)

    private Rigidbody2D rb;                                     // Reference to the Rigidbody2D component
    private bool isGrounded = false;                            // Track if the hexagon is on the ground
    private int jumpCount = 0;                                  // Counter for jumps (1 for normal, +1 for each double jump)
    private bool canDash = true;                                // Flag to track if dashing is allowed
    private bool dashTriggered = false;                         // Flag to check if dash input was triggered

    void Start()
    {
        // Get the Rigidbody2D component attached to the hexagon
        rb = GetComponent<Rigidbody2D>();

        // Set gravity scale to desired value
        rb.gravityScale = 4.25f;

        // Enable continuous collision detection to prevent tunneling
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
    }

    void Update()
    {
        // Handle ground check
        GroundCheck();

        // Check for player input to apply torque (for rolling effect)
        HandleRolling();

        // Check for jump input
        HandleJump();

        // Check if dash input is triggered (A + W or D + W)
        HandleDashInput();
    }

    void FixedUpdate()
    {
        // Perform the dash if dash input was triggered and conditions are met
        if (dashTriggered && canDash && !isGrounded)
        {
            PerformDash();
        }
    }

    private void GroundCheck()
    {
        // Check if the player is grounded using a small circle at the GroundCheck position
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

        if (isGrounded)
        {
            // Reset jump count when grounded
            jumpCount = 0; // Reset jump count to allow jumping again
            canDash = true; // Enable dashing again
            dashTriggered = false; // Reset dash trigger when grounded
        }
    }

    private void HandleRolling()
    {
        // Get horizontal input
        float horizontalInput = Input.GetAxis("Horizontal");

        // Apply horizontal movement
        rb.velocity = new Vector2(horizontalInput * movementForce, rb.velocity.y); // Maintain vertical velocity

        if (Input.GetKey(KeyCode.A))
        {
            // Apply counter-clockwise torque to simulate rolling left
            rb.AddTorque(torqueAmount);
        }
        else if (Input.GetKey(KeyCode.D))
        {
            // Apply clockwise torque to simulate rolling right
            rb.AddTorque(-torqueAmount);
        }
    }

    private void HandleJump()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (isGrounded)
            {
                // Normal jump
                rb.velocity = new Vector2(rb.velocity.x, jumpForce);
                jumpCount++;
                Debug.Log("Jump performed");
            }
            else if (jumpCount < maxJumpCount) // Allow double jump based on the max jump count
            {
                rb.velocity = new Vector2(rb.velocity.x, doubleJumpForce);
                jumpCount++;
                Debug.Log("Double Jump performed");
            }
        }
    }

    private void HandleDashInput()
    {
        // Check for dash input: pressing W while holding left (A) or right (D) and in the air
        if (Input.GetKey(KeyCode.W) && !isGrounded)
        {
            if (Input.GetKey(KeyCode.A)) // Dash left
            {
                dashTriggered = true;
                Debug.Log("Dash input detected: Left");
            }
            else if (Input.GetKey(KeyCode.D)) // Dash right
            {
                dashTriggered = true;
                Debug.Log("Dash input detected: Right");
            }
        }
    }

    private void PerformDash()
    {
        float dashDirection = Input.GetKey(KeyCode.A) ? -1 : 1;

        // Check for obstacles in the dash direction using a raycast
        Vector2 rayOrigin = rb.position;
        Vector2 rayDirection = new Vector2(dashDirection, 0);
        RaycastHit2D hit = Physics2D.Raycast(rayOrigin, rayDirection, dashRayDistance, obstacleLayer);

        if (hit.collider != null)
        {
            // Obstacle detected, do not dash
            Debug.Log("Dash blocked by obstacle: " + hit.collider.name);
            dashTriggered = false;
            return;
        }

        // No obstacle detected, perform the dash
        rb.velocity = new Vector2(dashDirection * dashForce, rb.velocity.y);

        // Capture the new position after the dash
        Vector3 newPosition = transform.position + new Vector3(dashDirection * dashForce * Time.fixedDeltaTime, 0, 0);

        // Play the dash particle effect at the new position
        PlayDashParticles(newPosition, dashDirection);

        canDash = false; // Disable further dashes until grounded again
        dashTriggered = false; // Reset dash trigger after performing dash
        Debug.Log("Dash performed!");
    }

    private void PlayDashParticles(Vector3 position, float dashDirection)
    {
        if (dashParticlePrefab != null)
        {
            // Instantiate the particle effect at the new position after the dash
            GameObject particles = Instantiate(dashParticlePrefab, position, Quaternion.identity);

            // Set the rotation based on the dash direction
            if (dashDirection == 1) // Dash right
            {
                particles.transform.rotation = Quaternion.Euler(0, -90, 0); // Facing right with Y rotation of -90
            }
            else if (dashDirection == -1) // Dash left
            {
                particles.transform.rotation = Quaternion.Euler(0, 90, 0); // Facing left with Y rotation of 90
            }

            // Start the fade coroutine
            StartCoroutine(FadeOutParticles(particles));
        }
    }

    private IEnumerator FadeOutParticles(GameObject particles)
    {
        // Get the ParticleSystem component
        ParticleSystem ps = particles.GetComponent<ParticleSystem>();
        var main = ps.main;

        // Enable the particle system
        ps.Play();

        // Fade in
        float fadeDuration = 0.5f; // Adjust duration for fade in
        float elapsedTime = 0f;

        // Fade in particles
        while (elapsedTime < fadeDuration)
        {
            float alpha = elapsedTime / fadeDuration;
            main.startLifetime = Mathf.Lerp(0, 1, alpha); // Fade in
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Allow the particles to play for a short while
        yield return new WaitForSeconds(0.5f);

        // Fade out
        elapsedTime = 0f;

        // Fade out particles
        while (elapsedTime < fadeDuration)
        {
            float alpha = 1 - (elapsedTime / fadeDuration);
            main.startLifetime = Mathf.Lerp(1, 0, alpha); // Fade out
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Destroy the particle object after fading out
        Destroy(particles);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Check if the hexagon is colliding with the ground layer
        if (((1 << collision.gameObject.layer) & groundLayer) != 0)
        {
            isGrounded = true;
            jumpCount = 0; // Reset jump count when touching the ground
            Debug.Log("Grounded");
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        // Check if the hexagon has left the ground layer
        if (((1 << collision.gameObject.layer) & groundLayer) != 0)
        {
            isGrounded = false;
            Debug.Log("Not Grounded");
        }
    }

    private void OnDrawGizmos()
    {
        // Check if rb is assigned before accessing its position
        if (rb != null)
        {
            // Draw ground check gizmo for debugging
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);

            // Draw the raycast for dash obstacle detection
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(rb.position, rb.position + new Vector2(dashRayDistance * (Input.GetKey(KeyCode.A) ? -1 : 1), 0));
        }
    }
}
