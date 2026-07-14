using UnityEngine;
using System.Collections;

public class MushroomEnemy : MonoBehaviour
{
    [Header("Movement (Chase)")]
    public float moveSpeed = 2.5f;

    [Header("Target & Range Settings")]
    public Transform target; // Karakter player aktif terdekat
    public float chaseRange = 10f; // Ditingkatkan sedikit agar lebih responsif mencari player
    public float attackRange = 1.3f;
    public float attackCooldown = 2.5f;

    [Header("Combat Settings")]
    public int damageAmount = 10;
    public Transform attackPoint;
    public float attackRadius = 0.6f;
    public LayerMask playerLayer;

    private float nextAttackTime;
    private Rigidbody2D rb;
    private Animator anim;
    private bool isDead = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();

        // Cari target pertama kali saat game dimulai
        FindClosestPlayer();
    }

    void Update()
    {
        if (isDead) return;

        // PERBAIKAN UTAMA: Selalu cari player terdekat secara dinamis setiap frame
        FindClosestPlayer();

        // Jika tidak ada satu pun player dengan tag "Player" di map
        if (target == null)
        {
            StopMoving();
            return;
        }

        float distanceToTarget = Vector2.Distance(transform.position, target.position);

        // State 1: Masuk jangkauan serang
        if (distanceToTarget <= attackRange)
        {
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
            if (anim != null) anim.SetFloat("Speed", 0f);

            if (Time.time >= nextAttackTime)
            {
                StartCoroutine(AttackSequence());
                nextAttackTime = Time.time + attackCooldown;
            }
        }
        // State 2: Masuk jangkauan kejar
        else if (distanceToTarget <= chaseRange)
        {
            ChaseTarget();
        }
        // State 3: Player terlalu jauh, diam
        else
        {
            StopMoving();
        }
    }

    void ChaseTarget()
    {
        if (target == null) return;

        float directionX = target.position.x - transform.position.x;
        float moveX = Mathf.Sign(directionX);

        // Gerakkan Rigidbody secara horizontal menuju target terdekat
        rb.linearVelocity = new Vector2(moveX * moveSpeed, rb.linearVelocity.y);

        if (anim != null) anim.SetFloat("Speed", Mathf.Abs(rb.linearVelocity.x));

        // Flip Sprite mengikuti arah target (Scale X = 1 default menghadap kiri)
        if (moveX > 0)
            transform.localScale = new Vector3(-1, 1, 1); // Hadap kanan
        else
            transform.localScale = new Vector3(1, 1, 1);  // Hadap kiri
    }

    void StopMoving()
    {
        rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
        if (anim != null) anim.SetFloat("Speed", 0f);
    }

    // Fungsi pencari player terdekat di seluruh map game
    void FindClosestPlayer()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        float closestDistance = Mathf.Infinity;
        GameObject closestPlayer = null;

        foreach (GameObject p in players)
        {
            float dist = Vector2.Distance(transform.position, p.transform.position);
            if (dist < closestDistance)
            {
                closestDistance = dist;
                closestPlayer = p;
            }
        }

        if (closestPlayer != null)
        {
            target = closestPlayer.transform;
        }
        else
        {
            target = null;
        }
    }

    IEnumerator AttackSequence()
    {
        if (anim != null) anim.SetTrigger("Attack");

        // Menunggu timing yang pas sesuai ayunan animasi gigit/pukul
        yield return new WaitForSeconds(0.4f);

        if (attackPoint == null) yield break;

        // Deteksi objek ber-layer Player di dalam area jangkauan serangan
        Collider2D[] hitPlayers = Physics2D.OverlapCircleAll(attackPoint.position, attackRadius, playerLayer);

        foreach (Collider2D player in hitPlayers)
        {
            // Kirim damage & pentalan ke Skeleton
            PlayerMovement pMove = player.GetComponent<PlayerMovement>();
            if (pMove != null)
            {
                pMove.TakeDamageFromEnemy(damageAmount, transform.position);
                continue;
            }

            // Kirim damage & pentalan ke Golem
            GolemBlue golem = player.GetComponent<GolemBlue>();
            if (golem != null)
            {
                golem.TakeDamageFromEnemy(damageAmount, transform.position);
                continue;
            }
        }
    }

    public void TakeDamage(int damage)
    {
        if (isDead) return;
        if (anim != null) anim.SetTrigger("Hit");
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, chaseRange);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        if (attackPoint != null)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawWireSphere(attackPoint.position, attackRadius);
        }
    }
}