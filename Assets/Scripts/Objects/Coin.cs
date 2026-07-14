using UnityEngine;

public class Coin : MonoBehaviour
{
    [Header("Coin Settings")]
    [SerializeField] private int scoreValue = 1;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Pastikan objek yang menyentuh koin adalah Player
        if (collision.CompareTag("Player"))
        {
            // Tambah skor koin ke GameManager berdasarkan karakter yang mengambil koin
            if (GameManager.Instance != null)
            {
                if (collision.GetComponent<PlayerMovement>() != null)
                {
                    GameManager.Instance.AddSkeletonCoin(scoreValue);
                }
                else if (collision.GetComponent<GolemBlue>() != null)
                {
                    GameManager.Instance.AddGolemCoin(scoreValue);
                }
            }

            // Putar suara koin
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayCoinSFX();
            }

            // Munculkan partikel koin
            SpawnParticles();

            // Hancurkan objek koin
            Destroy(gameObject);
        }
    }

    private void SpawnParticles()
    {
        // Membuat GameObject penampung partikel baru
        GameObject particleGO = new GameObject("CoinCollectParticles");
        particleGO.transform.position = transform.position;

        // Tambahkan komponen Particle System
        ParticleSystem ps = particleGO.AddComponent<ParticleSystem>();
        
        // Hentikan pemutaran otomatis bawaan Unity sebelum dikonfigurasi
        ps.Stop();

        // 1. Konfigurasi Modul Utama (Main Module)
        var main = ps.main;
        main.startColor = new Color(1f, 0.85f, 0f); // Warna Emas/Kuning
        main.startSize = new ParticleSystem.MinMaxCurve(0.1f, 0.25f);
        main.startSpeed = new ParticleSystem.MinMaxCurve(1f, 3f);
        main.startLifetime = new ParticleSystem.MinMaxCurve(0.3f, 0.6f);
        main.duration = 0.5f;
        main.loop = false;
        main.stopAction = ParticleSystemStopAction.Destroy; // Otomatis hancurkan GameObject setelah selesai

        // 2. Konfigurasi Emisi (Emission Module - Bursts)
        var emission = ps.emission;
        emission.rateOverTime = 0;
        emission.burstCount = 1;
        
        // Picu burst berisi 15 partikel secara instan
        emission.SetBurst(0, new ParticleSystem.Burst(0f, 15));

        // 3. Konfigurasi Bentuk (Shape Module - Sphere)
        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = 0.15f;

        // 4. Konfigurasi Warna Seiring Waktu (Color over Lifetime - Fade out)
        var colorOverLifetime = ps.colorOverLifetime;
        colorOverLifetime.enabled = true;
        Gradient grad = new Gradient();
        grad.SetKeys(
            new GradientColorKey[] { new GradientColorKey(new Color(1f, 0.85f, 0f), 0.0f), new GradientColorKey(new Color(1f, 0.6f, 0f), 1.0f) },
            new GradientAlphaKey[] { new GradientAlphaKey(1.0f, 0.0f), new GradientAlphaKey(0.0f, 1.0f) } // Memudar hingga transparan
        );
        colorOverLifetime.color = grad;

        // 5. Material Renderer agar dapat digambar oleh Unity
        var psRenderer = particleGO.GetComponent<ParticleSystemRenderer>();
        if (psRenderer != null)
        {
            // Gunakan shader default untuk rendering 2D tanpa tekstur rumit
            psRenderer.material = new Material(Shader.Find("Sprites/Default"));
        }

        // Jalankan efek partikel
        ps.Play();
    }
}
