using UnityEngine;

public class Terra : CharacterBase
{
    [Header("=== TERRA ABILITIES ===")]
    [SerializeField] private float pushForce = 8f;
    [SerializeField] private float pushRange = 1.5f;
    [SerializeField] private float climbSpeed = 2f;
    [SerializeField] private bool isClimbing = false;

    private float moveInput = 0f;
    //private Rigidbody2D heldBlock = null;

    protected override void Start()
    {
        base.Start();

        maxHP = Constants.TERRA_MAX_HP;
        moveSpeed = Constants.TERRA_SPEED;
        jumpForce = Constants.TERRA_JUMP;
        currentHP = maxHP;
    }

    protected override void HandleInput()
    {
        moveInput = InputManager.Instance.GetMoveInput();

        if (InputManager.Instance.GetJumpPressed())
        {
            if (isClimbing)
                StopClimbing();
            else
                Jump();
        }

        if (InputManager.Instance.GetInteractPressed())
            TryPushBlock();
    }

    protected override void UpdateAnimation()
    {
        if (animator != null)
        {
            animator.SetFloat("Speed", Mathf.Abs(moveInput));
            animator.SetBool("IsGrounded", isGrounded);
            animator.SetBool("IsClimbing", isClimbing);
        }
    }

    protected override void ApplyPhysics()
    {
        if (isClimbing)
        {
            Climb(moveInput);
        }
        else
        {
            Move(moveInput);
        }
    }

    // === TERRA ABILITY: Push Heavy Block ===
    private void TryPushBlock()
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(
            transform.position + Vector3.right * direction * pushRange,
            pushRange
        );

        foreach (Collider2D col in colliders)
        {
            if (col.CompareTag(Constants.TAG_BLOCK_HEAVY))
            {
                Rigidbody2D blockRb = col.GetComponent<Rigidbody2D>();
                if (blockRb != null)
                {
                    blockRb.velocity = Vector2.zero;
                    blockRb.AddForce(Vector2.right * direction * pushForce, ForceMode2D.Impulse);

                    if (animator != null)
                        animator.SetTrigger("Push");
                }
            }
        }
    }

    // === TERRA ABILITY: Climb Rocky Surface ===
    private void OnTriggerStay2D(Collider2D collision)
    {
        if (!isActive || !isAlive) return;

        if (collision.CompareTag("Rocky"))
        {
            if (InputManager.Instance.GetJumpPressed())
            {
                StartClimbing(collision);
            }
        }

        // Water damage
        if (collision.CompareTag("Water"))
        {
            TakeDamage(Constants.WATER_DAMAGE_PER_SECOND);
        }
    }

    private void StartClimbing(Collider2D surface)
    {
        isClimbing = true;
        // Jangan pakai RigidbodyConstraints2D
        // Just set velocity
        rb.velocity = Vector2.zero;
    }

    private void Climb(float input)
    {
        rb.velocity = new Vector2(0, input * climbSpeed);
    }

    private void StopClimbing()
    {
        isClimbing = false;
        rb.velocity = Vector2.zero;
    }

    // === TERRA ABILITY: Protect Pyro from Wind ===
    public bool IsProtectingPyro()
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(
            transform.position,
            Constants.TERRA_PROTECTION_RADIUS
        );

        foreach (Collider2D col in colliders)
        {
            if (col.CompareTag(Constants.TAG_PYRO))
                return IsActive();
        }

        return false;
    }
}