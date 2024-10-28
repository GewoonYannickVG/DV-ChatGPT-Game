using UnityEngine;

public class DoubleJumpZone : MonoBehaviour
{
    private HexagonMovement playerMovement;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerMovement = other.GetComponent<HexagonMovement>();
            if (playerMovement != null)
            {
                playerMovement.DisableDoubleJump();
            }
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if (playerMovement != null)
            {
                playerMovement.EnableDoubleJump();
            }
        }
    }
}