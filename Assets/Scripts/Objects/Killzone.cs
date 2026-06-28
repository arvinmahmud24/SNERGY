using UnityEngine;
using UnityEngine.SceneManagement; // Dibutuhkan untuk merestart game

public class Killzone : MonoBehaviour
{
    // Fungsi ini otomatis berjalan saat objek lain masuk ke area Trigger
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Periksa apakah objek yang jatuh memiliki tag "Player"
        if (collision.CompareTag("Player"))
        {
            // Merestart atau memuat ulang scene (level) yang sedang aktif
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }
}