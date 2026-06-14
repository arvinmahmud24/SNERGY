using UnityEngine;

// VERSION: Simple (No Light2D required)
// Bekerja di semua versi Unity 2020+

[RequireComponent(typeof(SpriteRenderer))]
public class LightTorch : MonoBehaviour
{
    [SerializeField] private float lightDuration = 5f;
    [SerializeField] private float lightRadius = 5f;

    private SpriteRenderer spriteRenderer;
    private bool isLit = false;
    private float lightTimeRemaining = 0f;

    // Untuk visual glow effect
    private GameObject glowEffect;

    private void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        isLit = false;
    }

    private void Update()
    {
        if (isLit)
        {
            lightTimeRemaining -= Time.deltaTime;
            if (lightTimeRemaining <= 0)
            {
                Extinguish();
            }
        }
    }

    public void Ignite()
    {
        if (isLit) return;

        isLit = true;
        lightTimeRemaining = lightDuration;

        // Visual feedback: ubah warna jadi kuning
        spriteRenderer.color = new Color(1f, 1f, 0.5f); // Yellow glow

        // Create glow effect (visual only)
        CreateGlowEffect();

        Debug.Log("🔥 Torch ignited! Duration: " + lightDuration + "s");
    }

    public void Extinguish()
    {
        if (!isLit) return;

        isLit = false;

        // Reset color
        spriteRenderer.color = Color.white;

        // Destroy glow
        if (glowEffect != null)
            Destroy(glowEffect);

        Debug.Log("❌ Torch extinguished!");
    }

    private void CreateGlowEffect()
    {
        // Buat child object untuk glow visual
        glowEffect = new GameObject("TorchGlow");
        glowEffect.transform.SetParent(transform);
        glowEffect.transform.localPosition = Vector3.zero;

        SpriteRenderer glowRenderer = glowEffect.AddComponent<SpriteRenderer>();
        glowRenderer.sprite = spriteRenderer.sprite;
        glowRenderer.color = new Color(1f, 1f, 0f, 0.3f); // Semi-transparent yellow
        glowRenderer.sortingOrder = -1;

        // Scale up sedikit untuk glow effect
        glowEffect.transform.localScale = new Vector3(1.3f, 1.3f, 1f);
    }

    public bool IsLit()
    {
        return isLit;
    }

    public Vector3 GetLightPosition()
    {
        return transform.position;
    }

    public float GetLightRadius()
    {
        return lightRadius;
    }
}