using UnityEngine;

public class Ladder : MonoBehaviour
{
    [SerializeField] private float climbSpeed = 5f;

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            Rigidbody2D rb = collision.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                float verticalInput = Input.GetAxisRaw("Vertical");

                if (Mathf.Abs(verticalInput) > 0.1f)
                {
                    rb.gravityScale = 0f;
                    rb.velocity = new Vector2(rb.velocity.x, verticalInput * climbSpeed);
                }
                else
                {
                    rb.velocity = new Vector2(rb.velocity.x, 0f);
                }
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            Rigidbody2D rb = collision.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.gravityScale = 1f;
            }
        }
    }
}