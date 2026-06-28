using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float jumpForce = 7f;

    [Header("Sistem Fall Damage")]
    public float fallDamageThreshold = 3f; // Jarak jatuh minimal untuk kena damage (3 blok)
    public int health = 100; // Nyawa awal karakter

    private bool isGrounded;
    private float highestYPosition; // Menyimpan titik tertinggi saat karakter melayang

    private Rigidbody2D rb;
    private Animator anim;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        highestYPosition = transform.position.y; // Simpan posisi saat awal mulai
    }

    void Update()
    {
        float move = Input.GetAxisRaw("Horizontal");

        // Pergerakan Kiri-Kanan
        rb.velocity = new Vector2(move * moveSpeed, rb.velocity.y);

        // Logika Lompat
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);
            isGrounded = false; // Langsung ubah status agar hitungan tinggi dimulai
        }

        // Jika karakter tidak menginjak tanah (sedang lompat atau jatuh)
        if (!isGrounded)
        {
            // Update posisi tertinggi jika karakter naik lebih tinggi
            if (transform.position.y > highestYPosition)
            {
                highestYPosition = transform.position.y;
            }
        }

        // Animasi Run
        anim.SetFloat("Speed", Mathf.Abs(move));

        // Flip karakter
        if (move > 0)
            transform.localScale = new Vector3(1, 1, 1);
        else if (move < 0)
            transform.localScale = new Vector3(-1, 1, 1);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Jika sebelumnya di udara, momen ini adalah saat mendarat
        if (!isGrounded)
        {
            // Hitung jarak jatuh: (Titik tertinggi di udara) dikurangi (Titik saat mendarat)
            float fallDistance = highestYPosition - transform.position.y;

            // Jika jarak jatuh lebih dari batas 3 blok dan karakter sedang bergerak ke bawah
            if (fallDistance > fallDamageThreshold)
            {
                TakeFallDamage();
            }
        }

        isGrounded = true;
        highestYPosition = transform.position.y; // Reset titik tertinggi setelah mendarat
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        isGrounded = false;
        highestYPosition = transform.position.y; // Mulai ukur ketinggian dari saat kaki lepas dari tanah
    }

    // Fungsi khusus untuk menangani damage
    private void TakeFallDamage()
    {
        health -= 10; // Kurangi nyawa 10 (bisa kamu ubah)

        // Memunculkan teks merah di tab Console agar kamu tahu sistemnya berjalan
        Debug.LogWarning("Aduh! Jatuh terlalu tinggi! Darah sekarang: " + health);

        if (health <= 0)
        {
            Debug.LogError("KARAKTER MATI!");
            // Nanti kamu bisa tambahkan animasi mati atau restart level di sini
        }
    }
}