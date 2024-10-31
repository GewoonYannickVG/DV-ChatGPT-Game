using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class MoveAndTeleport : MonoBehaviour
{
    public enum Direction { Horizontal, Vertical }
    public Direction moveDirection = Direction.Horizontal;
    public float moveDistance = 5f;
    public float moveSpeed = 2f;
    public Vector3 teleportPosition; // New field for teleportation coordinates

    private Vector3 startPosition;
    private bool movingForward = true;

    private void Start()
    {
        startPosition = transform.position;
        BoxCollider2D boxCollider = gameObject.AddComponent<BoxCollider2D>();
        boxCollider.isTrigger = true;
    }

    private void Update()
    {
        MoveObject();
    }

    private void MoveObject()
    {
        float moveAmount = moveSpeed * Time.deltaTime;

        if (moveDirection == Direction.Horizontal)
        {
            if (movingForward)
                transform.Translate(Vector3.right * moveAmount);
            else
                transform.Translate(Vector3.left * moveAmount);

            if (Vector3.Distance(startPosition, transform.position) >= moveDistance)
                movingForward = !movingForward;
        }
        else // Vertical movement
        {
            if (movingForward)
                transform.Translate(Vector3.up * moveAmount);
            else
                transform.Translate(Vector3.down * moveAmount);

            if (Vector3.Distance(startPosition, transform.position) >= moveDistance)
                movingForward = !movingForward;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            collision.gameObject.transform.position = teleportPosition;
        }
    }
}