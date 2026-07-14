using UnityEngine;
using System.Collections;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f;
    public float jumpForce = 7f;

    [Header("Combat & Health")]
    public int health = 30;
    // Kekuatan pentalan ke belakang
    [SerializeField] private float knockbackForceX = 8f;
    // Kekuatan pentalan sedikit ke atas agar pentalan terasa
    [SerializeField] private float knockbackForceY = 3f;
    // Durasi knockback di mana kontrol player dimatikan
    [SerializeField] private float knockbackDuration = 0.3f;

    [Header("Death")]
    public float deadAnimationDuration = 1.2f;

    [Header("Batas Jarak Antar Player (Multiplayer)")]
    public Transform otherPlayer;
    public float maxDistance = 30f;
    // Dihapus [HideInInspector] agar nilainya terlihat True/False di Inspector saat runtime
    public bool ignoreDistanceCheck = false;

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
    private bool isDead;
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
        // TAMBAHKAN KONDISI INI: Jika Rigidbody diset Static oleh portal, langsung hentikan fungsi Update
        if (rb != null && rb.bodyType == RigidbodyType2D.Static) return;

        // Jika mati atau sedang knockback, jangan baca input pergerakan
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
            // Baca Input Keyboard/Joystick
            if (Input.GetKey(KeyCode.RightArrow)) move = 1f;
            else if (Input.GetKey(KeyCode.LeftArrow)) move = -1f;
            else move = Input.GetAxis("Horizontal"); // Gunakan Axis untuk Joystick

            jumpPressed = Input.GetKeyDown(KeyCode.JoystickButton1) || Input.GetKeyDown(KeyCode.UpArrow);
        }

        MovePlayer(move);

        // Lompat: Mendukung Joystick Button 1 ATAU Panah Atas
        if (jumpPressed && isGrounded)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            isGrounded = false;
        }

        // Simpan titik tertinggi untuk fall damage
        if (!isGrounded)
        {
            if (transform.position.y > highestYPosition)
            {
                highestYPosition = transform.position.y;
            }
        }

        // Set Animasi Speed
        if (anim != null)
        {
            anim.SetFloat("Speed", Mathf.Abs(move));
        }

        // Flip Sprite
        if (move > 0.1f)
            transform.localScale = new Vector3(1, 1, 1);
        else if (move < -0.1f)
            transform.localScale = new Vector3(-1, 1, 1);
    }

    void MovePlayer(float move)
    {
        bool canMove = true;

        // Pengecekan hanya berjalan jika ignoreDistanceCheck bernilai FALSE
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

    // Fungsi utama dipanggil oleh musuh untuk damage & knockback
    public void TakeDamageFromEnemy(int damage, Vector2 enemyPosition)
    {
        if (isDead || isKnockback) return;

        health -= damage;
        damageNotice = $"-{damage} HP (Knockback!)";
        CancelInvoke("ClearNotice");
        Invoke("ClearNotice", 1.5f);

        // Hitung arah knockback (selalu menjauh dari posisi musuh)
        float knockbackDirX = transform.position.x - enemyPosition.x;
        // Gunakan Mathf.Sign agar nilainya hanya -1 (kiri) atau 1 (kanan)
        knockbackDirX = Mathf.Sign(knockbackDirX);

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

        // Reset velocity lama sebelum AddForce
        rb.linearVelocity = Vector2.zero;

        // Berikan gaya pentalan instan ke belakang dan sedikit ke atas
        Vector2 force = new Vector2(directionX * knockbackForceX, knockbackForceY);
        rb.AddForce(force, ForceMode2D.Impulse);

        // Picu animasi terluka (Hit)
        if (anim != null)
        {
            anim.ResetTrigger("Dead");
            anim.SetTrigger("Hit");
        }

        // Tunggu durasi pentalan selesai
        yield return new WaitForSeconds(knockbackDuration);

        // Reset velocity agar player tidak terus meluncur
        rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
        isKnockback = false;
    }

    // Fungsi Utility dsb... (Sama seperti sebelumnya)
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
            if (contact.normal.y > 0.5f) // Permukaan datar di kaki
            {
                if (!isGrounded)
                {
                    float fallDistance = highestYPosition - transform.position.y;
                    if (fallDistance >= fallDamageThreshold) { TakeFallDamage(); }
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

        foreach (ContactPoint2D contact in collision.contacts) { if (contact.normal.y > 0.5f) { isGrounded = true; break; } }
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
        if (health <= 0) { health = 0; StartCoroutine(DeadRoutine()); }
        else if (anim != null) { anim.ResetTrigger("Dead"); anim.SetTrigger("Hit"); }
    }

    void ClearNotice() { damageNotice = ""; }

    void OnGUI()
    {
        GUI.Box(new Rect(10, 10, 150, 30), "Skeleton HP: " + health);
        if (!string.IsNullOrEmpty(damageNotice))
        {
            GUI.color = Color.red;
            GUI.Label(new Rect(10, 45, 150, 30), damageNotice);
        }
    }

    IEnumerator DeadRoutine()
    {
        isDead = true;
        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
        if (anim != null) { anim.ResetTrigger("Hit"); anim.SetTrigger("Dead"); }
        yield return new WaitForSeconds(deadAnimationDuration);
        Time.timeScale = 0f;
    }
}