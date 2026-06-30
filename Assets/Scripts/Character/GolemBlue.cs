using UnityEngine;
using System.Collections;

public class GolemBlue : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f;
    public float jumpForce = 7f;

    [Header("Combat & Health")]
    public int health = 100;
    // Kekuatan pentalan ke belakang
    [SerializeField] private float knockbackForceX = 7f;
    // Kekuatan pentalan sedikit ke atas agar pentalan terasa
    [SerializeField] private float knockbackForceY = 3f;
    // Durasi knockback di mana kontrol Golem dimatikan sementara
    [SerializeField] private float knockbackDuration = 0.3f;

    [Header("Death")]
    public float deadAnimationDuration = 1.2f;

    [Header("Batas Jarak Antar Player (Multiplayer)")]
    public Transform otherPlayer;
    public float maxDistance = 30f;
    [HideInInspector] public bool ignoreDistanceCheck = false;

    [Header("Sistem Fall Damage")]
    public float fallDamageThreshold = 3f;

    private bool isGrounded;
    private bool isDead;
    private bool isKnockback; // Status apakah sedang terpental
    private float highestYPosition;
    private string damageNotice = "";

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
        // Jika mati atau sedang terpental (knockback), kunci input kontrol jalan
        if (isDead || isKnockback) return;

        float move = 0f;
        if (Input.GetKey(KeyCode.D)) move = 1f;
        else if (Input.GetKey(KeyCode.A)) move = -1f;

        MovePlayer(move);

        if (Input.GetKeyDown(KeyCode.W) && isGrounded)
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);
            isGrounded = false;
        }

        if (!isGrounded)
        {
            if (transform.position.y > highestYPosition)
            {
                highestYPosition = transform.position.y;
            }
        }

        if (anim != null)
        {
            anim.SetFloat("Speed", Mathf.Abs(move));
        }

        if (move > 0.1f)
            transform.localScale = new Vector3(1, 1, 1);
        else if (move < -0.1f)
            transform.localScale = new Vector3(-1, 1, 1);
    }

    void MovePlayer(float move)
    {
        bool canMove = true;

        if (otherPlayer != null && !ignoreDistanceCheck)
        {
            float distance = Vector2.Distance(transform.position, otherPlayer.position);
            if (distance >= maxDistance)
            {
                bool movingAway = (move > 0 && transform.position.x > otherPlayer.position.x) ||
                                  (move < 0 && transform.position.x < otherPlayer.position.x);
                if (movingAway) canMove = false;
            }
        }

        if (canMove)
            rb.velocity = new Vector2(move * moveSpeed, rb.velocity.y);
        else
            rb.velocity = new Vector2(0f, rb.velocity.y);
    }

    // Fungsi utama dipanggil oleh Mushroom saat Golem tergigit
    public void TakeDamageFromEnemy(int damage, Vector2 enemyPosition)
    {
        if (isDead || isKnockback) return;

        health -= damage;
        damageNotice = $"-{damage} HP (Knockback!)";
        CancelInvoke("ClearNotice");
        Invoke("ClearNotice", 1.5f);

        // Hitung arah pentalan (menjauh dari posisi musuh)
        float knockbackDirX = transform.position.x - enemyPosition.x;
        knockbackDirX = Mathf.Sign(knockbackDirX); // Menghasilkan 1 (Kanan) atau -1 (Kiri)

        if (health <= 0)
        {
            health = 0;
            StartCoroutine(DeadRoutine());
        }
        else
        {
            StartCoroutine(KnockbackRoutine(knockbackDirX));
        }
    }

    private IEnumerator KnockbackRoutine(float directionX)
    {
        isKnockback = true;

        // Reset sisa kecepatan lama sebelum melempar rigidBody
        rb.velocity = Vector2.zero;

        // Berikan dorongan pentalan instan ke belakang-atas
        Vector2 force = new Vector2(directionX * knockbackForceX, knockbackForceY);
        rb.AddForce(force, ForceMode2D.Impulse);

        // Jalankan animasi terluka Golem
        if (anim != null)
        {
            anim.ResetTrigger("Dead");
            anim.SetTrigger("Hit");
        }

        // Tunggu durasi knockback hingga kontrol dikembalikan ke player
        yield return new WaitForSeconds(knockbackDuration);

        rb.velocity = new Vector2(0f, rb.velocity.y);
        isKnockback = false;
    }

    public void ResetStatusAfterPortal()
    {
        isGrounded = true;
        highestYPosition = transform.position.y;
        if (anim != null) anim.SetFloat("Speed", 0f);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (isDead) return;

        if (collision.gameObject.CompareTag("Player"))
        {
            isGrounded = true;
            return;
        }

        foreach (ContactPoint2D contact in collision.contacts)
        {
            if (contact.normal.y > 0.5f)
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
                break;
            }
        }
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (isDead) return;
        foreach (ContactPoint2D contact in collision.contacts)
        {
            if (contact.normal.y > 0.5f)
            {
                isGrounded = true;
                break;
            }
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (isDead) return;
        isGrounded = false;
        highestYPosition = transform.position.y;
    }

    private void TakeFallDamage()
    {
        if (isDead) return;
        health -= 10;
        damageNotice = "-10 HP (Fall)";
        CancelInvoke("ClearNotice");
        Invoke("ClearNotice", 1.5f);

        if (health <= 0)
        {
            health = 0;
            StartCoroutine(DeadRoutine());
        }
        else if (anim != null)
        {
            anim.ResetTrigger("Dead");
            anim.SetTrigger("Hit");
        }
    }

    void ClearNotice() { damageNotice = ""; }

    void OnGUI()
    {
        // UI Golem di pojok kanan atas layar
        GUI.Box(new Rect(Screen.width - 160, 10, 150, 30), "Golem HP: " + health);
        if (!string.IsNullOrEmpty(damageNotice))
        {
            GUI.color = Color.red;
            GUI.Label(new Rect(Screen.width - 160, 45, 150, 30), damageNotice);
        }
    }

    IEnumerator DeadRoutine()
    {
        isDead = true;
        rb.velocity = new Vector2(0, rb.velocity.y);
        if (anim != null)
        {
            anim.SetTrigger("Dead");
        }
        yield return new WaitForSeconds(deadAnimationDuration);
        Time.timeScale = 0f;
    }
}