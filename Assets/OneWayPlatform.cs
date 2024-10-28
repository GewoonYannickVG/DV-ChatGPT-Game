using UnityEngine;

public class OneWayPlatform : MonoBehaviour
{
    private Collider2D platformCollider;

    void Start()
    {
        platformCollider = GetComponent<Collider2D>();
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            if (collision.relativeVelocity.y > 0)
            {
                PlatformPassThrough(collision.gameObject);
            }
        }
    }

    void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            PlatformSolid(collision.gameObject);
        }
    }

    private void PlatformPassThrough(GameObject player)
    {
        Physics2D.IgnoreCollision(player.GetComponent<Collider2D>(), platformCollider, true);
    }

    private void PlatformSolid(GameObject player)
    {
        Physics2D.IgnoreCollision(player.GetComponent<Collider2D>(), platformCollider, false);
    }
}