using UnityEngine;
using System.Collections;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f;
    public float jumpForce = 7f;

    [Header("Batas Jarak Antar Player")]
    public Transform otherPlayer;
    public float maxDistance = 30f;

    [Header("Sistem Fall Damage")]
    public float fallDamageThreshold = 3f;
    public int health = 30;

    [Header("Death")]
    public float deadAnimationDuration = 1.2f;

    private bool isGrounded;
    private bool isDead;
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
        // Jika mati, tidak bisa bergerak lagi
        if (isDead)
            return;

        float move = Input.GetAxis("Horizontal");

        MovePlayer(move);

        // Lompat (Pastikan kamu menggunakan Gamepad/Joystick. Jika ingin pakai keyboard, tambahkan Input.GetKeyDown(KeyCode.W))
        if (Input.GetKeyDown(KeyCode.JoystickButton1) && isGrounded)
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);
            isGrounded = false;
        }

        // Menyimpan titik tertinggi saat di udara
        if (!isGrounded)
        {
            if (transform.position.y > highestYPosition)
            {
                highestYPosition = transform.position.y;
            }
        }

        // Animasi Jalan
        if (anim != null)
        {
            anim.SetFloat("Speed", Mathf.Abs(move));
        }

        // Flip Sprite
        if (move > 0.1f)
        {
            transform.localScale = new Vector3(1, 1, 1);
        }
        else if (move < -0.1f)
        {
            transform.localScale = new Vector3(-1, 1, 1);
        }
    }

    void MovePlayer(float move)
    {
        bool canMove = true;

        if (otherPlayer != null)
        {
            float distance = Vector2.Distance(transform.position, otherPlayer.position);

            if (distance >= maxDistance)
            {
                bool movingAway =
                    (move > 0 && transform.position.x > otherPlayer.position.x) ||
                    (move < 0 && transform.position.x < otherPlayer.position.x);

                if (movingAway)
                    canMove = false;
            }
        }

        if (canMove)
        {
            rb.velocity = new Vector2(move * moveSpeed, rb.velocity.y);
        }
        else
        {
            rb.velocity = new Vector2(0, rb.velocity.y);
        }
    }

    // --- PERBAIKAN SISTEM GROUNDED & FALL DAMAGE ---
    private void OnCollisionStay2D(Collision2D collision)
    {
        if (isDead)
            return;

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

                    Debug.Log("Tinggi jatuh : " + fallDistance);

                    if (fallDistance >= fallDamageThreshold)
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
        if (isDead)
            return;

        // Saat karakter melayang (lompat atau jatuh dari pinggiran)
        isGrounded = false;
        highestYPosition = transform.position.y; // Reset titik tertinggi sebagai awal mula jatuh
    }
    // ------------------------------------------------

    void TakeFallDamage()
    {
        health -= 10;

        Debug.Log("Health : " + health);

        if (health <= 0)
        {
            health = 0;

            StartCoroutine(DeadRoutine());
        }
        else
        {
            anim.ResetTrigger("Dead");
            anim.SetTrigger("Hit");
        }
    }

    IEnumerator DeadRoutine()
    {
        isDead = true;

        Debug.Log("KARAKTER MATI!");

        // Hentikan gerakan
        rb.velocity = Vector2.zero;
        rb.gravityScale = 0;

        // Reset trigger lain
        anim.ResetTrigger("Hit");

        // Jalankan animasi Dead
        anim.SetTrigger("Dead");

        // Tunggu animasi selesai
        yield return new WaitForSeconds(deadAnimationDuration);

        Debug.Log("GAME OVER");

        // Pause game
        Time.timeScale = 0f;
    }
}