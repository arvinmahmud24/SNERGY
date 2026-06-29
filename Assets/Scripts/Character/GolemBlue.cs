using UnityEngine;

public class GolemBlue : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f;
    public float jumpForce = 7f;

    [Header("Batas Jarak Antar Player")]
    public Transform otherPlayer;
    public float maxDistance = 30f;

    [Header("Sistem Fall Damage")]
    public float fallDamageThreshold = 3f;
    public int health = 100;

    private bool isGrounded;
    private float highestYPosition;

    private Rigidbody2D rb;
    private Animator anim;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        highestYPosition = transform.position.y;
    }

    void Update()
    {
        // Input khusus GolemBlue
        float move = 0f;

        if (Input.GetKey(KeyCode.D))
        {
            move = 1f;
        }
        else if (Input.GetKey(KeyCode.A))
        {
            move = -1f;
        }

        MovePlayer(move);

        // Lompat
        if (Input.GetKeyDown(KeyCode.W) && isGrounded)
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);
            isGrounded = false;
        }

        // Menghitung titik tertinggi saat di udara
        if (!isGrounded)
        {
            if (transform.position.y > highestYPosition)
            {
                highestYPosition = transform.position.y;
            }
        }

        // Animasi
        anim.SetFloat("Speed", Mathf.Abs(move));

        // Flip karakter
        if (move > 0.1f)
            transform.localScale = new Vector3(1, 1, 1);
        else if (move < -0.1f)
            transform.localScale = new Vector3(-1, 1, 1);
    }

    void MovePlayer(float move)
    {
        bool canMove = true;

        if (otherPlayer != null)
        {
            float distance = Vector2.Distance(transform.position, otherPlayer.position);

            // Jika jarak sudah maksimum
            if (distance >= maxDistance)
            {
                bool movingAway = (move > 0 && transform.position.x > otherPlayer.position.x) ||
                                  (move < 0 && transform.position.x < otherPlayer.position.x);

                if (movingAway)
                {
                    canMove = false;
                }
            }
        }

        if (canMove)
            rb.velocity = new Vector2(move * moveSpeed, rb.velocity.y);
        else
            rb.velocity = new Vector2(0f, rb.velocity.y);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!isGrounded)
        {
            float fallDistance = highestYPosition - transform.position.y;

            if (fallDistance > fallDamageThreshold)
            {
                TakeFallDamage();
            }
        }

        isGrounded = true;
        highestYPosition = transform.position.y;
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        isGrounded = false;
        highestYPosition = transform.position.y;
    }

    private void TakeFallDamage()
    {
        health -= 10;
        Debug.LogWarning("GolemBlue jatuh terlalu tinggi! HP: " + health);

        if (health <= 0)
        {
            Debug.LogError("GOLEMBLUE MATI!");
        }
    }
}