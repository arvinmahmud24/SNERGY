using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class FinishPortal : MonoBehaviour
{
    [Header("Pengaturan Level")]
    [SerializeField] private string nextLevelName;
    [SerializeField] private float delayBeforeLoad = 2.5f;

    private bool skeletonInside = false;
    private bool golemInside = false;

    private GameObject savedSkeleton;
    private GameObject savedGolem;

    private Coroutine portalCoroutine;
    private bool isCountingDown = false;
    private bool isLevelCleared = false;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Pastikan objek yang masuk memiliki Tag "Player"
        if (collision.CompareTag("Player"))
        {
            // 1. Deteksi Skeleton (Mencari komponen PlayerMovement)
            PlayerMovement skeleton = collision.GetComponent<PlayerMovement>();
            if (skeleton != null && !skeletonInside)
            {
                skeletonInside = true;
                savedSkeleton = collision.gameObject;
                SembunyikanPlayer(savedSkeleton);
                Debug.Log("Skeleton masuk portal. Menunggu GolemBlue...");
            }

            // 2. Deteksi GolemBlue
            GolemBlue golem = collision.GetComponent<GolemBlue>();
            if (golem != null && !golemInside)
            {
                golemInside = true;
                savedGolem = collision.gameObject;
                SembunyikanPlayer(savedGolem);
                Debug.Log("GolemBlue masuk portal. Menunggu Skeleton...");
            }
        }

        // Mulai hitung mundur jika KEDUA player sudah berada di dalam portal
        if (skeletonInside && golemInside && !isCountingDown)
        {
            portalCoroutine = StartCoroutine(PindahLevelSequence());
        }
    }

    private IEnumerator PindahLevelSequence()
    {
        isCountingDown = true;
        Debug.Log("KEDUA PLAYER SUDAH MASUK! Menghitung mundur level...");

        yield return new WaitForSeconds(delayBeforeLoad);

        // Tandai bahwa level telah selesai dan hentikan waktu permainan
        isLevelCleared = true;
        Time.timeScale = 0f;

        // Mainkan musik kemenangan
        AudioManager.Instance.PlayWinMusic();
    }

    private void SembunyikanPlayer(GameObject player)
    {
        Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.bodyType = RigidbodyType2D.Static; // Dikunci agar tidak jatuh ke jurang
        }

        SpriteRenderer playerSprite = player.GetComponent<SpriteRenderer>();
        if (playerSprite != null) playerSprite.enabled = false; // Dihilangkan wujudnya

        // Matikan semua collider agar tidak menghalangi player lain yang ingin masuk portal
        Collider2D[] colliders = player.GetComponents<Collider2D>();
        foreach (Collider2D col in colliders)
        {
            col.enabled = false;
        }

        // ================= PERBAIKAN BUG UTAMA =================
        // 1. Matikan pembatas jarak pada diri sendiri yang masuk portal
        PlayerMovement skeletonScript = player.GetComponent<PlayerMovement>();
        if (skeletonScript != null) skeletonScript.ignoreDistanceCheck = true;

        GolemBlue golemScript = player.GetComponent<GolemBlue>();
        if (golemScript != null) golemScript.ignoreDistanceCheck = true;

        // 2. Matikan juga pembatas jarak pada rekan yang masih berjuang di luar portal
        if (skeletonScript != null)
        {
            // Jika Skeleton yang masuk, cari GolemBlue di map dan bebaskan jalannya
            GolemBlue partner = Object.FindFirstObjectByType<GolemBlue>();
            if (partner != null) partner.ignoreDistanceCheck = true;
        }
        else if (golemScript != null)
        {
            // Jika Golem yang masuk, cari Skeleton (PlayerMovement) di map dan bebaskan jalannya
            PlayerMovement partner = Object.FindFirstObjectByType<PlayerMovement>();
            if (partner != null) partner.ignoreDistanceCheck = true;
        }
        // =======================================================
    }

    private void OnGUI()
    {
        if (!isLevelCleared) return;

        // Gambar latar belakang hitam transparan menutupi seluruh layar
        Texture2D blackTexture = new Texture2D(1, 1);
        blackTexture.SetPixel(0, 0, new Color(0, 0, 0, 0.7f));
        blackTexture.Apply();
        
        GUIStyle bgStyle = new GUIStyle();
        bgStyle.normal.background = blackTexture;
        GUI.Box(new Rect(0, 0, Screen.width, Screen.height), GUIContent.none, bgStyle);

        // Ukuran jendela popup kemenangan
        float width = 450f;
        float height = 340f;
        float x = (Screen.width - width) / 2f;
        float y = (Screen.height - height) / 2f;

        // Penampilan jendela utama
        GUI.Box(new Rect(x, y, width, height), "");

        // Set style teks & tombol
        GUIStyle titleStyle = new GUIStyle();
        titleStyle.alignment = TextAnchor.MiddleCenter;
        titleStyle.fontSize = 32;
        titleStyle.fontStyle = FontStyle.Bold;
        titleStyle.normal.textColor = Color.yellow;

        GUIStyle subTitleStyle = new GUIStyle();
        subTitleStyle.alignment = TextAnchor.MiddleCenter;
        subTitleStyle.fontSize = 16;
        subTitleStyle.normal.textColor = Color.white;

        GUIStyle buttonStyle = new GUIStyle(GUI.skin.button);
        buttonStyle.fontSize = 16;
        buttonStyle.fontStyle = FontStyle.Bold;

        // Gambar Judul dan Pesan Kemenangan
        GUI.Label(new Rect(x, y + 40, width, 50), "LEVEL CLEARED!", titleStyle);
        GUI.Label(new Rect(x, y + 100, width, 30), "Kedua player berhasil mencapai portal akhir!", subTitleStyle);

        // Desain Tombol Aksi
        float btnWidth = 200f;
        float btnHeight = 45f;
        float btnX = x + (width - btnWidth) / 2f;

        if (GUI.Button(new Rect(btnX, y + 140, btnWidth, btnHeight), "Next Level", buttonStyle))
        {
            Time.timeScale = 1f; // Kembalikan waktu normal
            if (!string.IsNullOrEmpty(nextLevelName))
            {
                SceneManager.LoadScene(nextLevelName);
            }
            else
            {
                // Jika tidak ada scene baru, muat ulang scene pertama / tutorial
                SceneManager.LoadScene("World_1_Tutorial");
            }
        }

        if (GUI.Button(new Rect(btnX, y + 200, btnWidth, btnHeight), "Restart Level", buttonStyle))
        {
            Time.timeScale = 1f; // Kembalikan waktu normal
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        if (GUI.Button(new Rect(btnX, y + 260, btnWidth, btnHeight), "Main Menu", buttonStyle))
        {
            Time.timeScale = 1f; // Kembalikan waktu normal
            SceneManager.LoadScene("MainMenu");
        }
    }
}