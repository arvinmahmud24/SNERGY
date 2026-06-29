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

    // --- PERBAIKAN SISTEM GROUNDED & FALL DAMAGE ---
    private void OnCollisionStay2D(Collision2D collision)
    {
        // Mengecek semua titik sentuh tumbukan
        foreach (ContactPoint2D contact in collision.contacts)
        {
            // Jika arah sentuhan mengarah ke atas (Y > 0.5), berarti itu adalah lantai
            if (contact.normal.y > 0.5f)
            {
                // Cek Fall Damage HANYA saat karakter baru mendarat
                if (!isGrounded)
                {
                    float fallDistance = highestYPosition - transform.position.y;
                    if (fallDistance > fallDamageThreshold)
                    {
                        TakeFallDamage();
                    }
                }

                isGrounded = true;
                highestYPosition = transform.position.y; // Update posisi tertinggi saat di tanah
                return; // Selesai mengecek, karakter terbukti ada di tanah
            }
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        // Saat karakter melayang (lompat atau jatuh dari pinggiran)
        isGrounded = false;
        highestYPosition = transform.position.y; // Reset titik tertinggi sebagai awal mula jatuh
    }
    // ------------------------------------------------

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