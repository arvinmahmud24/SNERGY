using UnityEngine;

public abstract class CharacterBase : MonoBehaviour
{
    [Header("=== MOVEMENT ===")]
    [SerializeField] protected float moveSpeed = 5f;
    [SerializeField] protected float jumpForce = 5f;
    [SerializeField] protected float groundDrag = 5f;
    [SerializeField] protected float airDrag = 1f;

    [Header("=== STATS ===")]
    [SerializeField] protected int maxHP = 100;
    protected int currentHP;
    protected bool isAlive = true;

    [Header("=== REFERENCES ===")]
    protected Rigidbody2D rb;
    protected SpriteRenderer spriteRenderer;
    protected Animator animator;
    protected CircleCollider2D circleCollider;

    [Header("=== STATE ===")]
    protected bool isGrounded = false;
    protected bool isActive = false; // Active = controllable, Standby = idle
    protected float lastJumpTime = -999f;
    protected int direction = 1; // 1 = right, -1 = left

    protected virtual void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
        circleCollider = GetComponent<CircleCollider2D>();

        currentHP = maxHP;
    }

    protected virtual void Update()
    {
        if (!isActive || !isAlive) return;

        HandleInput();
        UpdateAnimation();
        UpdateGravity();
    }

    protected virtual void FixedUpdate()
    {
        if (!isActive || !isAlive) return;

        CheckGrounded();
        ApplyPhysics();
    }

    // === ABSTRACT METHODS (Must override in child classes) ===
    protected abstract void HandleInput();
    protected abstract void UpdateAnimation();
    protected abstract void ApplyPhysics();

    // === MOVEMENT ===
    protected void Move(float input)
    {
        // Horizontal movement
        rb.linearVelocity = new Vector2(input * moveSpeed, rb.linearVelocity.y);

        // Update sprite direction
        if (input > 0.1f)
        {
            direction = 1;
            spriteRenderer.flipX = false;
        }
        else if (input < -0.1f)
        {
            direction = -1;
            spriteRenderer.flipX = true;
        }
    }

    protected void Jump()
    {
        if (!isGrounded) return;
        if (Time.time - lastJumpTime < Constants.JUMP_COOLDOWN) return;

        rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0);
        rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
        lastJumpTime = Time.time;

        if (animator != null)
            animator.SetTrigger("Jump");
    }

    protected void CheckGrounded()
    {
        // Raycast down from bottom of collider
        Vector2 rayOrigin = (Vector2)transform.position + Vector2.down * 0.5f;
        RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.down, 0.15f);

        isGrounded = hit.collider != null && hit.collider.CompareTag("Tile");
    }

    private void UpdateGravity()
    {
        // Apply different drag based on grounded state
        rb.linearDamping = isGrounded ? groundDrag : airDrag;
    }

    // === HEALTH & DEATH ===
    public virtual void TakeDamage(int damage)
    {
        if (!isAlive) return;

        currentHP -= damage;

        if (currentHP <= 0)
        {
            Die();
        }
    }

    public virtual void Heal(int amount)
    {
        currentHP = Mathf.Min(currentHP + amount, maxHP);
    }

    public virtual void Die()
    {
        isAlive = false;
        isActive = false;

        if (animator != null)
            animator.SetTrigger("Death");

        // Ragdoll atau fade out effect bisa ditambah di sini
        Invoke(nameof(Respawn), 2f);
    }

    protected virtual void Respawn()
    {
        // Override ini di specific character atau GameManager
        gameObject.SetActive(false);
    }

    // === SWAP SYSTEM ===
    public void SetActive(bool active)
    {
        isActive = active;

        // Disable animator saat standby (optimization)
        if (animator != null)
            animator.enabled = active;

        // Visual feedback: dimming saat standby
        UpdateVisualFeedback();
    }

    private void UpdateVisualFeedback()
    {
        Color color = isActive ? Color.white : new Color(1, 1, 1, 0.5f);
        spriteRenderer.color = color;
    }

    public bool IsActive()
    {
        return isActive;
    }

    public int GetCurrentHP()
    {
        return currentHP;
    }

    public int GetMaxHP()
    {
        return maxHP;
    }

    // === DIRECTION ===
    public int GetDirection()
    {
        return direction;
    }

    public void SetDirection(int dir)
    {
        direction = dir;
        spriteRenderer.flipX = direction == -1;
    }
}