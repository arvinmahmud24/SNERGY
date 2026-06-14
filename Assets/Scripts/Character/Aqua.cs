using UnityEngine;

public class Aqua : CharacterBase
{
    [Header("=== AQUA ABILITIES ===")]
    [SerializeField] private float waterSpeedMultiplier = Constants.AQUA_WATER_SPEED_MULTIPLIER;

    private float moveInput = 0f;
    private bool isInWater = false;

    protected override void Start()
    {
        base.Start();

        maxHP = Constants.AQUA_MAX_HP;
        moveSpeed = Constants.AQUA_SPEED;
        jumpForce = Constants.AQUA_JUMP;
        currentHP = maxHP;
    }

    protected override void HandleInput()
    {
        moveInput = InputManager.Instance.GetMoveInput();

        if (InputManager.Instance.GetJumpPressed())
            Jump();
    }

    protected override void UpdateAnimation()
    {
        if (animator != null)
        {
            animator.SetFloat("Speed", Mathf.Abs(moveInput));
            animator.SetBool("IsGrounded", isGrounded);
            animator.SetBool("IsInWater", isInWater);
        }
    }

    protected override void ApplyPhysics()
    {
        // Aqua 2x faster in water
        float currentSpeed = isInWater ? moveSpeed * waterSpeedMultiplier : moveSpeed;
        rb.velocity = new Vector2(moveInput * currentSpeed, rb.velocity.y);
    }

    // === AQUA ABILITY: Extinguish Torch + Speed in Water ===
    private void OnTriggerStay2D(Collider2D collision)
    {
        if (!isActive || !isAlive) return;

        if (collision.CompareTag(Constants.TAG_TORCH))
        {
            LightTorch torch = collision.GetComponent<LightTorch>();
            if (torch != null && torch.IsLit())
            {
                torch.Extinguish();
            }
        }

        if (collision.CompareTag("Water"))
        {
            isInWater = true;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Water"))
        {
            isInWater = false;
        }
    }

    // === AQUA WEAKNESS: Death on contact with Pyro ===
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!isActive || !isAlive) return;

        if (collision.CompareTag(Constants.TAG_PYRO))
        {
            TakeDamage(maxHP); // Instant death
        }

        if (collision.CompareTag("Lava"))
        {
            TakeDamage(Constants.LAVA_INSTANT_DAMAGE);
        }
    }
}