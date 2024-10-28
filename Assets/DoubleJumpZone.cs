using UnityEngine;

public class DoubleJumpZone : MonoBehaviour
{
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            HexagonMovement playerMovement = other.GetComponent<HexagonMovement>();
            if (playerMovement != null)
            {
                playerMovement.isInNoDoubleJumpZone = true; // Entering no double jump zone
            }
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            HexagonMovement playerMovement = other.GetComponent<HexagonMovement>();
            if (playerMovement != null)
            {
                playerMovement.isInNoDoubleJumpZone = false; // Exiting no double jump zone
            }
        }
    }
}