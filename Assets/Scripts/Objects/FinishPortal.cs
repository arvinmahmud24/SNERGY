using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class FinishPortal : MonoBehaviour
{
    [Header("Pengaturan Level")]
    [SerializeField] private string nextLevelName;
    [SerializeField] private float delayBeforeLoad = 2.5f;

    [Header("Nama Tombol Batal (Input Manager)")]
    [SerializeField] private string cancelInputButton = "Cancel";

    private bool skeletonInside = false;
    private bool golemInside = false;

    private GameObject savedSkeleton;
    private GameObject savedGolem;

    private Coroutine portalCoroutine;
    private bool isCountingDown = false;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // 1. Deteksi Skeleton (PlayerMovement)
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

        // Mulai hitung mundur jika KEDUA player sudah berada di dalam portal
        if (skeletonInside && golemInside && !isCountingDown)
        {
            portalCoroutine = StartCoroutine(PindahLevelSequence());
        }
    }

    private void Update()
    {
        // Mendeteksi tombol Batal (ESC / Gamepad B)
        if ((skeletonInside || golemInside) && Input.GetButtonDown(cancelInputButton))
        {
            CancelPortalSequence();
        }
    }

    private IEnumerator PindahLevelSequence()
    {
        isCountingDown = true;
        Debug.Log("KEDUA PLAYER SUDAH MASUK! Pintu terkunci, menghitung mundur level...");

        yield return new WaitForSeconds(delayBeforeLoad);

        if (!string.IsNullOrEmpty(nextLevelName))
        {
            SceneManager.LoadScene(nextLevelName);
        }
        else
        {
            Debug.LogWarning("Nama scene tujuan kosong! Game di-pause sebagai tanda menang.");
            Time.timeScale = 0f;
        }
    }

    private void CancelPortalSequence()
    {
        // FIX UTAMA: Jika hitung mundur sudah berjalan (berarti KEDUA player sudah masuk), 
        // maka tombol batal tidak akan berfungsi sama sekali (terkunci).
        if (isCountingDown)
        {
            Debug.Log("Tidak bisa keluar! Kedua player sudah berada di dalam portal.");
            return;
        }

        // Jalur di bawah ini hanya akan dieksekusi jika BARU SALAH SATU player yang masuk
        if (portalCoroutine != null)
        {
            StopCoroutine(portalCoroutine);
            portalCoroutine = null;
        }

        // Keluarkan player mana saja yang sempat masuk sendirian
        if (skeletonInside && savedSkeleton != null)
        {
            TampilkanPlayer(savedSkeleton);
            skeletonInside = false;
        }

        if (golemInside && savedGolem != null)
        {
            TampilkanPlayer(savedGolem);
            golemInside = false;
        }

        Debug.Log("Salah satu player membatalkan teleportasi sebelum rekannya tiba.");
    }

    private void SembunyikanPlayer(GameObject player)
    {
        Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.velocity = Vector2.zero;
            rb.bodyType = RigidbodyType2D.Static;
        }

        SpriteRenderer playerSprite = player.GetComponent<SpriteRenderer>();
        if (playerSprite != null) playerSprite.enabled = false;

        PlayerMovement skeletonScript = player.GetComponent<PlayerMovement>();
        if (skeletonScript != null) skeletonScript.ignoreDistanceCheck = true;

        GolemBlue golemScript = player.GetComponent<GolemBlue>();
        if (golemScript != null) golemScript.ignoreDistanceCheck = true;
    }

    private void TampilkanPlayer(GameObject player)
    {
        Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.bodyType = RigidbodyType2D.Dynamic;
            rb.velocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }

        SpriteRenderer playerSprite = player.GetComponent<SpriteRenderer>();
        if (playerSprite != null) playerSprite.enabled = true;

        // Pental sedikit ke kiri atas agar keluar dari area pemicu portal
        player.transform.position += new Vector3(-1.5f, 0.2f, 0f);

        PlayerMovement skeletonScript = player.GetComponent<PlayerMovement>();
        if (skeletonScript != null)
        {
            skeletonScript.ResetStatusAfterPortal();
            skeletonScript.ignoreDistanceCheck = false;
        }

        GolemBlue golemScript = player.GetComponent<GolemBlue>();
        if (golemScript != null)
        {
            golemScript.ResetStatusAfterPortal();
            golemScript.ignoreDistanceCheck = false;
        }
    }
}