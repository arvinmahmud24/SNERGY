using UnityEngine;

public class EnemyFollow : MonoBehaviour
{
    [Header("Movement & Detection")]
    public float speed = 2f;
    public float detectionRange = 10f;
    public float attackRange = 1.5f;

    [Header("Attack Settings")]
    public int damageAmount = 10;
    public float attackCooldown = 1f;
    private float nextAttackTime = 0f;

    [Header("Optimization")]
    [Tooltip("Seberapa sering musuh mencari player terdekat (detik). Lebih tinggi = lebih hemat CPU.")]
    public float targetSearchInterval = 0.2f;
    private float nextSearchTime = 0f;

    [Header("Sprite Setup")]
    [Tooltip("Centang ini jika secara default sprite musuhmu menghadap ke KIRI")]
    public bool spriteDefaultHadapKiri = true;

    private Transform targetPlayer;
    private Rigidbody2D rb;
    private Animator anim;
    private float originalScaleX;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        originalScaleX = Mathf.Abs(transform.localScale.x);
    }

    void FixedUpdate()
    {
        // Optimasi: Hanya mencari player terdekat pada interval tertentu, tidak tiap frame fisik
        if (Time.time >= nextSearchTime)
        {
            FindClosestPlayer();
            nextSearchTime = Time.time + targetSearchInterval;
        }

        if (targetPlayer == null)
        {
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
            anim.SetBool("IsRunning", false);
            anim.SetBool("IsAttacking", false);
            return;
        }

        float distance = Vector2.Distance(transform.position, targetPlayer.position);

        if (distance <= detectionRange)
        {
            float directionX = targetPlayer.position.x > transform.position.x ? 1f : -1f;

            if (distance <= attackRange)
            {
                rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
                anim.SetBool("IsRunning", false);

                Flip(directionX);

                if (Time.time >= nextAttackTime)
                {
                    Attack();
                    nextAttackTime = Time.time + attackCooldown;
                }
            }
            else
            {
                rb.linearVelocity = new Vector2(directionX * speed, rb.linearVelocity.y);
                anim.SetBool("IsRunning", true);
                anim.SetBool("IsAttacking", false);

                Flip(directionX);
            }
        }
        else
        {
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
            anim.SetBool("IsRunning", false);
            anim.SetBool("IsAttacking", false);
        }
    }

    void FindClosestPlayer()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        float closestDistance = Mathf.Infinity;
        Transform closestPlayer = null;

        foreach (GameObject p in players)
        {
            float distance = Vector2.Distance(transform.position, p.transform.position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestPlayer = p.transform;
            }
        }

        targetPlayer = closestPlayer;
    }

    void Flip(float directionX)
    {
        float finalScaleX = directionX * originalScaleX;

        if (spriteDefaultHadapKiri)
        {
            finalScaleX = -directionX * originalScaleX;
        }

        transform.localScale = new Vector3(finalScaleX, transform.localScale.y, transform.localScale.z);
    }

    void Attack()
    {
        anim.SetBool("IsAttacking", true);

        if (targetPlayer != null)
        {
            PlayerMovement pm = targetPlayer.GetComponent<PlayerMovement>();
            if (pm != null)
            {
                pm.TakeDamageFromEnemy(damageAmount, transform.position);
                return;
            }

            GolemBlue gb = targetPlayer.GetComponent<GolemBlue>();
            if (gb != null)
            {
                gb.TakeDamageFromEnemy(damageAmount, transform.position);
                return;
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}