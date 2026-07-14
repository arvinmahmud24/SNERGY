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

    [Header("AI Companion Settings")]
    public bool isAIControlled = false;
    private float aiMoveInput = 0f;
    private bool aiJumpTrigger = false;

    [Header("Stacking Settings")]
    [SerializeField] private float carryingSpeedMultiplier = 0.6f;
    [HideInInspector] public Rigidbody2D carrierRigidbody = null;
    [HideInInspector] public Rigidbody2D passengerRigidbody = null;

    private bool isGrounded;
    private bool isDead; // Menandai apakah player sudah mati
    private bool isKnockback; // Status apakah sedang terpental
    private float highestYPosition;
    private string damageNotice = "";

    public void SetAIInputs(float moveInput, bool jumpTrigger)
    {
        aiMoveInput = moveInput;
        if (jumpTrigger)
        {
            aiJumpTrigger = true;
        }
    }

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
        bool jumpPressed = false;

        if (isAIControlled)
        {
            move = aiMoveInput;
            jumpPressed = aiJumpTrigger;
            aiJumpTrigger = false; // Consume jump trigger
        }
        else
        {
            if (Input.GetKey(KeyCode.D)) move = 1f;
            else if (Input.GetKey(KeyCode.A)) move = -1f;

            jumpPressed = Input.GetKeyDown(KeyCode.W);
        }

        MovePlayer(move);

        if (jumpPressed && isGrounded)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
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

        // Hitung kecepatan dasar, kurangi jika membawa penumpang
        float currentSpeed = passengerRigidbody != null ? moveSpeed * carryingSpeedMultiplier : moveSpeed;
        float targetHorizontalVelocity = move * currentSpeed;

        // Jika kita sedang digendong, tambahkan kecepatan carrier ke pergerakan kita
        if (carrierRigidbody != null)
        {
            targetHorizontalVelocity += carrierRigidbody.linearVelocity.x;
        }

        if (canMove)
            rb.linearVelocity = new Vector2(targetHorizontalVelocity, rb.linearVelocity.y);
        else
            rb.linearVelocity = new Vector2(carrierRigidbody != null ? carrierRigidbody.linearVelocity.x : 0f, rb.linearVelocity.y);
    }

    // Fungsi utama dipanggil oleh musuh saat Golem terkena serangan
    public void TakeDamageFromEnemy(int damage, Vector2 enemyPosition)
    {
        // PERBAIKAN 1: Jika sudah mati, abaikan semua damage dan hit trigger selanjutnya
        if (isDead) return;

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
            // Pentalan hanya terjadi jika belum mati
            if (!isKnockback)
            {
                StartCoroutine(KnockbackRoutine(knockbackDirX));
            }
        }
    }

    private IEnumerator KnockbackRoutine(float directionX)
    {
        isKnockback = true;

        // Reset sisa kecepatan lama sebelum melempar rigidBody
        rb.linearVelocity = Vector2.zero;

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

        rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
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
            foreach (ContactPoint2D contact in collision.contacts)
            {
                if (contact.normal.y > 0.5f) // Kita berdiri di atas player lain
                {
                    PlayerMovement otherPM = collision.gameObject.GetComponent<PlayerMovement>();
                    if (otherPM != null)
                    {
                        carrierRigidbody = otherPM.GetComponent<Rigidbody2D>();
                        otherPM.passengerRigidbody = rb;
                    }
                    else
                    {
                        GolemBlue otherGB = collision.gameObject.GetComponent<GolemBlue>();
                        if (otherGB != null)
                        {
                            carrierRigidbody = otherGB.GetComponent<Rigidbody2D>();
                            otherGB.passengerRigidbody = rb;
                        }
                    }
                    break;
                }
                else if (contact.normal.y < -0.5f) // Player lain berdiri di atas kita
                {
                    PlayerMovement otherPM = collision.gameObject.GetComponent<PlayerMovement>();
                    if (otherPM != null)
                    {
                        passengerRigidbody = otherPM.GetComponent<Rigidbody2D>();
                        otherPM.carrierRigidbody = rb;
                    }
                    else
                    {
                        GolemBlue otherGB = collision.gameObject.GetComponent<GolemBlue>();
                        if (otherGB != null)
                        {
                            passengerRigidbody = otherGB.GetComponent<Rigidbody2D>();
                            otherGB.carrierRigidbody = rb;
                        }
                    }
                    break;
                }
            }
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

        if (collision.gameObject.CompareTag("Player"))
        {
            isGrounded = true;
            foreach (ContactPoint2D contact in collision.contacts)
            {
                if (contact.normal.y > 0.5f)
                {
                    PlayerMovement otherPM = collision.gameObject.GetComponent<PlayerMovement>();
                    if (otherPM != null)
                    {
                        carrierRigidbody = otherPM.GetComponent<Rigidbody2D>();
                        otherPM.passengerRigidbody = rb;
                    }
                    else
                    {
                        GolemBlue otherGB = collision.gameObject.GetComponent<GolemBlue>();
                        if (otherGB != null)
                        {
                            carrierRigidbody = otherGB.GetComponent<Rigidbody2D>();
                            otherGB.passengerRigidbody = rb;
                        }
                    }
                    break;
                }
                else if (contact.normal.y < -0.5f)
                {
                    PlayerMovement otherPM = collision.gameObject.GetComponent<PlayerMovement>();
                    if (otherPM != null)
                    {
                        passengerRigidbody = otherPM.GetComponent<Rigidbody2D>();
                        otherPM.carrierRigidbody = rb;
                    }
                    else
                    {
                        GolemBlue otherGB = collision.gameObject.GetComponent<GolemBlue>();
                        if (otherGB != null)
                        {
                            passengerRigidbody = otherGB.GetComponent<Rigidbody2D>();
                            otherGB.carrierRigidbody = rb;
                        }
                    }
                    break;
                }
            }
            return;
        }

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

        if (collision.gameObject.CompareTag("Player"))
        {
            PlayerMovement otherPM = collision.gameObject.GetComponent<PlayerMovement>();
            if (otherPM != null)
            {
                if (carrierRigidbody == otherPM.GetComponent<Rigidbody2D>())
                {
                    carrierRigidbody = null;
                }
                if (passengerRigidbody == otherPM.GetComponent<Rigidbody2D>())
                {
                    passengerRigidbody = null;
                }
                if (otherPM.carrierRigidbody == rb)
                {
                    otherPM.carrierRigidbody = null;
                }
                if (otherPM.passengerRigidbody == rb)
                {
                    otherPM.passengerRigidbody = null;
                }
            }
            else
            {
                GolemBlue otherGB = collision.gameObject.GetComponent<GolemBlue>();
                if (otherGB != null)
                {
                    if (carrierRigidbody == otherGB.GetComponent<Rigidbody2D>())
                    {
                        carrierRigidbody = null;
                    }
                    if (passengerRigidbody == otherGB.GetComponent<Rigidbody2D>())
                    {
                        passengerRigidbody = null;
                    }
                    if (otherGB.carrierRigidbody == rb)
                    {
                        otherGB.carrierRigidbody = null;
                    }
                    if (otherGB.passengerRigidbody == rb)
                    {
                        otherGB.passengerRigidbody = null;
                    }
                }
            }
        }

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
        GUI.Box(new Rect(Screen.width - 160, 10, 150, 30), "Golem HP: " + health);
        if (!string.IsNullOrEmpty(damageNotice))
        {
            GUI.color = Color.red;
            GUI.Label(new Rect(Screen.width - 160, 45, 150, 30), damageNotice);
        }
    }

    // PERBAIKAN UTAMA: Penanganan kematian Golem yang rapi
    IEnumerator DeadRoutine()
    {
        isDead = true;

        // 1. Matikan script input agar pemain tidak bisa bergerak
        this.enabled = false;

        // 2. Mainkan Animasi Mati
        if (anim != null)
        {
            anim.ResetTrigger("Hit");
            anim.SetTrigger("Dead");
        }

        // 3. Matikan gesekan/pergerakan horizontal tapi pertahankan gravitasi agar tidak melayang/tembus
        if (rb != null)
        {
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
            // Kunci rotasi dan pergerakan X agar jasadnya tetap stabil
            rb.constraints = RigidbodyConstraints2D.FreezeRotation | RigidbodyConstraints2D.FreezePositionX;
        }

        // 4. Ubah Collider agar jasad tidak melayang ataupun amblas menembus tanah
        BoxCollider2D col = GetComponent<BoxCollider2D>();
        if (col != null)
        {
            // Memendekkan tinggi collider agar pas dengan sprite jasad Golem yang ceper
            col.size = new Vector2(col.size.x, 0.25f);
            // Menggeser posisi collider ke bagian bawah tubuh agar menapak di tanah
            col.offset = new Vector2(col.offset.x, -0.35f);
        }

        // 5. Tunggu hingga durasi animasi mati selesai
        yield return new WaitForSeconds(deadAnimationDuration);

        // (Opsional) Hentikan sistem fisika sepenuhnya setelah animasi selesai agar performa lancar
        if (rb != null)
        {
            rb.bodyType = RigidbodyType2D.Static;
        }

        // Hentikan waktu game (sesuai gameplay asli Anda)
        Time.timeScale = 0f;
    }
}