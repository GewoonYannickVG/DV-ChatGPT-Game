using UnityEngine;

public class OneWayPlatformControl : MonoBehaviour
{
    private BoxCollider2D platformCollider;      // Main platform collider
    public BoxCollider2D detectionTrigger;       // Trigger collider below the platform
    private bool isIgnoringCollision = false;    // Track if collision is currently ignored

    private void Start()
    {
        // Get the main platform collider (non-trigger)
        platformCollider = GetComponent<BoxCollider2D>();

        if (detectionTrigger == null)
        {
            Debug.LogError("Please assign a detection trigger collider in the Inspector.");
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            Rigidbody2D playerRb = collision.GetComponent<Rigidbody2D>();
            if (playerRb != null && playerRb.velocity.y > 0)
            {
                // If player is moving upwards, ignore collision temporarily
                Physics2D.IgnoreCollision(platformCollider, collision, true);
                isIgnoringCollision = true;
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            StartCoroutine(ResetCollision(collision));
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.collider.CompareTag("Player"))
        {
            StartCoroutine(ResetCollision(collision.collider));
        }
    }

    private System.Collections.IEnumerator ResetCollision(Collider2D collision)
    {
        if (isIgnoringCollision)
        {
            yield return new WaitForSeconds(0.1f);  // Short delay to allow player to fully exit platform
            Physics2D.IgnoreCollision(platformCollider, collision, false);
            isIgnoringCollision = false;
        }
    }
}
