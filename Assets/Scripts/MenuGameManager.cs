using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuGameManager : MonoBehaviour
{
    [Header("Background Settings")]
    public Texture2D backgroundTexture;

    private bool isGameStarted = false;
    private bool isPaused = false;

    void Start()
    {
        // Saat game pertama kali dibuka, hentikan waktu permainan (Freeze)
        Time.timeScale = 0f;
        isGameStarted = false;
        isPaused = false;

        // Putar musik main menu saat overlay main menu aktif
        AudioManager.Instance.PlayMainMenuMusic();
    }

    void Update()
    {
        // Fitur Pause Menu menggunakan tombol ESC (Hanya aktif setelah PLAY diklik)
        if (isGameStarted && Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused)
            {
                ResumeGame();
            }
            else
            {
                PauseGame();
            }
        }
    }

    // --- FUNGSI UTAMA UNTUK TOMBOL ---

    // 1. Fungsi saat tombol PLAY diklik
    public void PlayGame()
    {
        Time.timeScale = 1f; // Jalankan kembali waktu game
        isGameStarted = true;
        isPaused = false;
    }

    // 2. Fungsi saat tombol RESUME diklik (Di Pause Menu)
    public void ResumeGame()
    {
        Time.timeScale = 1f;
        isPaused = false;
    }

    // 3. Fungsi memunculkan jeda permainan
    public void PauseGame()
    {
        Time.timeScale = 0f;
        isPaused = true;
    }

    // 4. Fungsi saat tombol EXIT diklik
    public void ExitGame()
    {
        Debug.Log("Game ditutup!");
        Application.Quit(); // Menutup game (aktif di build executable)
    }

    private void OnGUI()
    {
        // 1. TAMPILAN AWAL / MAIN MENU (Jika game belum dimulai)
        if (!isGameStarted)
        {
            DrawMenuOverlay("SYNERGY", "A Co-op Elemental Adventure", "Play Game", "Exit", true);
        }
        // 2. PAUSE MENU (Jika game sedang di-pause)
        else if (isPaused)
        {
            DrawMenuOverlay("GAME PAUSED", "Game sedang di-pause", "Resume", "Restart Level", false);
        }
    }

    private void DrawMenuOverlay(string title, string subtitle, string btn1Text, string btn2Text, bool isMainMenu)
    {
        // Gambar latar belakang (Gunakan texture jika dipasang di Inspector dan ini adalah Main Menu)
        if (isMainMenu && backgroundTexture != null)
        {
            GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), backgroundTexture, ScaleMode.ScaleAndCrop);
        }
        else
        {
            // Gambar latar belakang hitam transparan menutupi seluruh layar
            Texture2D blackTexture = new Texture2D(1, 1);
            blackTexture.SetPixel(0, 0, new Color(0, 0, 0, 0.75f));
            blackTexture.Apply();
            
            GUIStyle bgStyle = new GUIStyle();
            bgStyle.normal.background = blackTexture;
            GUI.Box(new Rect(0, 0, Screen.width, Screen.height), GUIContent.none, bgStyle);
        }

        // Ukuran jendela popup menu
        float width = 450f;
        float height = isMainMenu ? 280f : 340f; // Main menu memiliki 2 tombol (280px), Pause menu memiliki 3 tombol (340px)
        float x = (Screen.width - width) / 2f;
        float y = (Screen.height - height) / 2f;

        // Jendela utama
        GUI.Box(new Rect(x, y, width, height), "");

        // Set style teks & tombol agar identik dengan panel kemenangan
        GUIStyle titleStyle = new GUIStyle();
        titleStyle.alignment = TextAnchor.MiddleCenter;
        titleStyle.fontSize = 32;
        titleStyle.fontStyle = FontStyle.Bold;
        titleStyle.normal.textColor = Color.yellow; // Warna kuning emas

        GUIStyle subTitleStyle = new GUIStyle();
        subTitleStyle.alignment = TextAnchor.MiddleCenter;
        subTitleStyle.fontSize = 16;
        subTitleStyle.normal.textColor = Color.white;

        GUIStyle buttonStyle = new GUIStyle(GUI.skin.button);
        buttonStyle.fontSize = 16;
        buttonStyle.fontStyle = FontStyle.Bold;

        // Gambar Judul dan Subtitle
        GUI.Label(new Rect(x, y + 40, width, 50), title, titleStyle);
        GUI.Label(new Rect(x, y + 95, width, 30), subtitle, subTitleStyle);

        // Desain Tombol Aksi
        float btnWidth = 200f;
        float btnHeight = 45f;
        float btnX = x + (width - btnWidth) / 2f;

        if (isMainMenu)
        {
            // Tombol 1: Play Game
            if (GUI.Button(new Rect(btnX, y + 150, btnWidth, btnHeight), btn1Text, buttonStyle))
            {
                PlayGame();
            }

            // Tombol 2: Exit
            if (GUI.Button(new Rect(btnX, y + 210, btnWidth, btnHeight), btn2Text, buttonStyle))
            {
                ExitGame();
            }
        }
        else
        {
            // Tombol 1: Resume
            if (GUI.Button(new Rect(btnX, y + 140, btnWidth, btnHeight), btn1Text, buttonStyle))
            {
                ResumeGame();
            }

            // Tombol 2: Restart Level
            if (GUI.Button(new Rect(btnX, y + 200, btnWidth, btnHeight), btn2Text, buttonStyle))
            {
                Time.timeScale = 1f; // Kembalikan waktu normal sebelum reload
                SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            }

            // Tombol 3: Exit
            if (GUI.Button(new Rect(btnX, y + 260, btnWidth, btnHeight), "Exit", buttonStyle))
            {
                ExitGame();
            }
        }
    }
}