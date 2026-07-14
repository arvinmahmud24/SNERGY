using UnityEngine;
using System.Collections;

public class GolemBlue : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f;
    public float jumpForce = 7f;

    [Header("Combat & Health")]
    public int health = 100;
    [SerializeField] private float knockbackForceX = 7f;
    [SerializeField] private float knockbackForceY = 3f;
    [SerializeField] private float knockbackDuration = 0.3f;

    [Header("Death")]
    public float deadAnimationDuration = 1.2f;

    [Header("Batas Jarak Antar Player (Multiplayer)")]
    public Transform otherPlayer;
    public float maxDistance = 30f;
    /* [HideInInspector] dihapus sementara agar Anda bisa memantau status true/false */
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
    private bool isKnockback;
    private float highestYPosition;
    private string damageNotice = "";

    public void SetAIInputs(float moveInput, bool jumpTrigger)
    {
        aiMoveInput = moveInput;
        if (jumpTrigger) aiJumpTrigger = true;
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
            aiJumpTrigger = false;
        }
        else
        {
            if (Input.GetKey(KeyCode.D)) move = 1f;
            else if (Input.GetKey(KeyCode.A)) move = -1f;
            else
            {
                move = Input.GetAxis("Horizontal_P2");
            }

            jumpPressed = Input.GetKeyDown(KeyCode.Joystick2Button0) || 
                          Input.GetKeyDown(KeyCode.Joystick2Button1) || 
                          Input.GetKeyDown(KeyCode.W);
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

        if (anim != null) anim.SetFloat("Speed", Mathf.Abs(move));

        if (move > 0.1f)
            transform.localScale = new Vector3(1, 1, 1);
        else if (move < -0.1f)
            transform.localScale = new Vector3(-1, 1, 1);
    }

    void MovePlayer(float move)
    {
        bool canMove = true;

        // Mengecek jarak hanya jika ignoreDistanceCheck bernilai FALSE
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

        float currentSpeed = passengerRigidbody != null ? moveSpeed * carryingSpeedMultiplier : moveSpeed;
        float targetHorizontalVelocity = move * currentSpeed;

        if (carrierRigidbody != null)
        {
            targetHorizontalVelocity += carrierRigidbody.linearVelocity.x;
        }

        if (canMove)
            rb.linearVelocity = new Vector2(targetHorizontalVelocity, rb.linearVelocity.y);
        else
            rb.linearVelocity = new Vector2(carrierRigidbody != null ? carrierRigidbody.linearVelocity.x : 0f, rb.linearVelocity.y);
    }

    public void TakeDamageFromEnemy(int damage, Vector2 enemyPosition)
    {
        if (isDead) return;

        health -= damage;
        damageNotice = $"-{damage} HP (Knockback!)";
        CancelInvoke("ClearNotice");
        Invoke("ClearNotice", 1.5f);

        float knockbackDirX = Mathf.Sign(transform.position.x - enemyPosition.x);

        if (health <= 0)
        {
            health = 0;
            StartCoroutine(DeadRoutine());
        }
        else
        {
            if (!isKnockback) StartCoroutine(KnockbackRoutine(knockbackDirX));
        }
    }

    private IEnumerator KnockbackRoutine(float directionX)
    {
        isKnockback = true;
        rb.linearVelocity = Vector2.zero;
        Vector2 force = new Vector2(directionX * knockbackForceX, knockbackForceY);
        rb.AddForce(force, ForceMode2D.Impulse);

        if (anim != null)
        {
            anim.ResetTrigger("Dead");
            anim.SetTrigger("Hit");
        }

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
                if (!isGrounded)
                {
                    float fallDistance = highestYPosition - transform.position.y;
                    if (fallDistance > fallDamageThreshold) TakeFallDamage();
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
                if (carrierRigidbody == otherPM.GetComponent<Rigidbody2D>()) carrierRigidbody = null;
                if (passengerRigidbody == otherPM.GetComponent<Rigidbody2D>()) passengerRigidbody = null;
                if (otherPM.carrierRigidbody == rb) otherPM.carrierRigidbody = null;
                if (otherPM.passengerRigidbody == rb) otherPM.passengerRigidbody = null;
            }
            else
            {
                GolemBlue otherGB = collision.gameObject.GetComponent<GolemBlue>();
                if (otherGB != null)
                {
                    if (carrierRigidbody == otherGB.GetComponent<Rigidbody2D>()) carrierRigidbody = null;
                    if (passengerRigidbody == otherGB.GetComponent<Rigidbody2D>()) passengerRigidbody = null;
                    if (otherGB.carrierRigidbody == rb) otherGB.carrierRigidbody = null;
                    if (otherGB.passengerRigidbody == rb) otherGB.passengerRigidbody = null;
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


    IEnumerator DeadRoutine()
    {
        isDead = true;
        this.enabled = false;

        if (anim != null)
        {
            anim.ResetTrigger("Hit");
            anim.SetTrigger("Dead");
        }

        if (rb != null)
        {
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
            rb.constraints = RigidbodyConstraints2D.FreezeRotation | RigidbodyConstraints2D.FreezePositionX;
        }

        BoxCollider2D col = GetComponent<BoxCollider2D>();
        if (col != null)
        {
            col.size = new Vector2(col.size.x, 0.25f);
            col.offset = new Vector2(col.offset.x, -0.35f);
        }

        yield return new WaitForSeconds(deadAnimationDuration);

        if (rb != null) rb.bodyType = RigidbodyType2D.Static;
        Time.timeScale = 0f;
    }
}