using UnityEngine;

public class Pyro : CharacterBase
{
    [Header("=== PYRO ABILITIES ===")]
    [SerializeField] private float torchInteractRange = 1.5f;

    private float moveInput = 0f;

    protected override void Start()
    {
        base.Start();

        maxHP = Constants.PYRO_MAX_HP;
        moveSpeed = Constants.PYRO_SPEED;
        jumpForce = Constants.PYRO_JUMP;
        currentHP = maxHP;
    }

    protected override void HandleInput()
    {
        moveInput = InputManager.Instance.GetMoveInput();

        if (InputManager.Instance.GetJumpPressed())
            Jump();

        if (InputManager.Instance.GetInteractPressed())
            TryLightTorch();
    }

    protected override void UpdateAnimation()
    {
        if (animator != null)
        {
            animator.SetFloat("Speed", Mathf.Abs(moveInput));
            animator.SetBool("IsGrounded", isGrounded);
        }
    }

    protected override void ApplyPhysics()
    {
        Move(moveInput);
    }

    // === PYRO ABILITY: Light Torch ===
    private void TryLightTorch()
    {
        // Find nearby torch
        Collider2D[] colliders = Physics2D.OverlapCircleAll(
            transform.position,
            torchInteractRange
        );

        foreach (Collider2D col in colliders)
        {
            if (col.CompareTag(Constants.TAG_TORCH))
            {
                LightTorch torch = col.GetComponent<LightTorch>();
                if (torch != null && !torch.IsLit())
                {
                    torch.Ignite();
                    break;
                }
            }
        }
    }

    // === PYRO WEAKNESS: Death on water ===
    private void OnTriggerStay2D(Collider2D collision)
    {
        if (!isActive || !isAlive) return;

        if (collision.CompareTag("Water"))
        {
            TakeDamage(Constants.WATER_DAMAGE_PER_SECOND);
        }
    }

    // === WIND WEAKNESS ===
    public void ApplyWindForce(Vector2 windForce)
    {
        if (!isActive) return;

        rb.AddForce(windForce, ForceMode2D.Force);
    }

    // === TERRA PROTECTION ===
    public bool IsProtectedFromWind()
    {
        // Check if Terra is nearby
        Collider2D[] colliders = Physics2D.OverlapCircleAll(
            transform.position,
            Constants.TERRA_PROTECTION_RADIUS
        );

        foreach (Collider2D col in colliders)
        {
            if (col.CompareTag(Constants.TAG_TERRA))
            {
                Terra terra = col.GetComponent<Terra>();
                if (terra != null && terra.IsActive())
                    return true;
            }
        }

        return false;
    }
}