using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class FinishPortal : MonoBehaviour
{
    [Header("Pengaturan Level")]
    [SerializeField] private string nextLevelName;
    [SerializeField] private float delayBeforeLoad = 2.5f; // Beri waktu agak lama agar sempat cancel

    [Header("Nama Tombol Batal (Input Manager)")]
    // Secara default kita isi "Cancel" (di Unity, ini otomatis membaca tombol ESC di keyboard atau tombol B/Lingkaran di Gamepad)
    [SerializeField] private string cancelInputButton = "Cancel";

    private bool isPlayerInside = false;
    private Coroutine portalCoroutine;
    private GameObject savedPlayer;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && !isPlayerInside)
        {
            isPlayerInside = true;
            savedPlayer = collision.gameObject;

            portalCoroutine = StartCoroutine(PlayerEnterPortal(savedPlayer));
        }
    }

    private void Update()
    {
        // Mendeteksi tombol batal dari keyboard maupun gamepad
        if (isPlayerInside && Input.GetButtonDown(cancelInputButton))
        {
            CancelPortalSequence();
        }
    }

    private IEnumerator PlayerEnterPortal(GameObject player)
    {
        Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.velocity = Vector2.zero;
            rb.bodyType = RigidbodyType2D.Static;
        }

        SpriteRenderer playerSprite = player.GetComponent<SpriteRenderer>();
        if (playerSprite != null)
        {
            playerSprite.enabled = false;
        }

        Debug.Log("Player masuk portal. Tekan tombol BATAL (ESC / Gamepad B) untuk membatalkan!");

        yield return new WaitForSeconds(delayBeforeLoad);

        if (!string.IsNullOrEmpty(nextLevelName))
        {
            SceneManager.LoadScene(nextLevelName);
        }
    }

    private void CancelPortalSequence()
    {
        if (portalCoroutine != null && savedPlayer != null)
        {
            StopCoroutine(portalCoroutine);

            SpriteRenderer playerSprite = savedPlayer.GetComponent<SpriteRenderer>();
            if (playerSprite != null) playerSprite.enabled = true;

            Rigidbody2D rb = savedPlayer.GetComponent<Rigidbody2D>();
            if (rb != null) rb.bodyType = RigidbodyType2D.Dynamic;

            // Geser sedikit ke kiri agar keluar dari pusat portal
            savedPlayer.transform.position += new Vector3(-0.5f, 0f, 0f);

            isPlayerInside = false;
            portalCoroutine = null;

            Debug.Log("Teleportasi Dibatalkan via Gamepad/Keyboard!");
        }
    }
}