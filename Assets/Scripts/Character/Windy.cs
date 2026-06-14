using UnityEngine;

public class Windy : CharacterBase
{
    [Header("=== WINDY ABILITIES ===")]
    [SerializeField] private float flyDuration = Constants.WINDY_FLY_DURATION;
    [SerializeField] private float flyCooldown = Constants.WINDY_FLY_COOLDOWN;
    [SerializeField] private float blowForce = 5f;
    [SerializeField] private float blowRange = 2f;

    private float moveInput = 0f;
    private bool isFlying = false;
    private float flyTimeRemaining = 0f;
    private float flyCooldownRemaining = 0f;

    protected override void Start()
    {
        base.Start();

        maxHP = Constants.WINDY_MAX_HP;
        moveSpeed = Constants.WINDY_SPEED;
        jumpForce = Constants.WINDY_JUMP;
        currentHP = maxHP;
    }

    protected override void HandleInput()
    {
        moveInput = InputManager.Instance.GetMoveInput();

        if (InputManager.Instance.GetJumpPressed())
        {
            if (isFlying)
                StopFlying();
            else
                Jump();
        }

        if (InputManager.Instance.GetAbilityPressed())
            TryFly();

        // Blowing (hold direction)
        if (moveInput != 0)
            TryBlowObjects();
    }

    protected override void UpdateAnimation()
    {
        if (animator != null)
        {
            animator.SetFloat("Speed", Mathf.Abs(moveInput));
            animator.SetBool("IsGrounded", isGrounded);
            animator.SetBool("IsFlying", isFlying);
        }
    }

    protected override void ApplyPhysics()
    {
        if (isFlying)
        {
            // No gravity saat terbang
            rb.velocity = new Vector2(moveInput * moveSpeed, 0);
        }
        else
        {
            Move(moveInput);
        }
    }

    // === WINDY ABILITY: Fly ===
    private void TryFly()
    {
        if (isFlying) return;
        if (flyCooldownRemaining > 0) return;

        isFlying = true;
        flyTimeRemaining = flyDuration;
        rb.velocity = new Vector2(rb.velocity.x, 0);
        rb.gravityScale = 0; // Disable gravity while flying
    }

    private void StopFlying()
    {
        isFlying = false;
        flyCooldownRemaining = flyCooldown;
        rb.gravityScale = 1; // Re-enable gravity
    }

    protected override void FixedUpdate()
    {
        base.FixedUpdate();

        if (isFlying)
        {
            flyTimeRemaining -= Time.fixedDeltaTime;
            if (flyTimeRemaining <= 0)
                StopFlying();
        }

        if (flyCooldownRemaining > 0)
            flyCooldownRemaining -= Time.fixedDeltaTime;
    }

    // === WINDY ABILITY: Blow Light Objects ===
    private void TryBlowObjects()
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(
            transform.position + Vector3.right * direction * blowRange,
            blowRange
        );

        foreach (Collider2D col in colliders)
        {
            if (col.CompareTag(Constants.TAG_BLOCK_LIGHT))
            {
                Rigidbody2D objRb = col.GetComponent<Rigidbody2D>();
                if (objRb != null)
                {
                    objRb.velocity = Vector2.zero;
                    objRb.AddForce(Vector2.right * direction * blowForce, ForceMode2D.Impulse);
                }
            }
        }
    }

    // === WINDY WEAKNESS: Fire damage ===
    private void OnTriggerStay2D(Collider2D collision)
    {
        if (!isActive || !isAlive) return;

        if (collision.CompareTag("Lava") || collision.CompareTag("Fire"))
        {
            TakeDamage(Constants.LAVA_DAMAGE_PER_SECOND);
        }
    }

    // === GET STATS ===
    public bool IsFlying()
    {
        return isFlying;
    }

    public float GetFlyCooldownPercent()
    {
        return 1f - (flyCooldownRemaining / flyCooldown);
    }
}