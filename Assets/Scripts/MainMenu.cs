using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [Header("Background Settings")]
    public Texture2D backgroundTexture;

    private void Start()
    {
        // Pastikan waktu game berjalan normal saat berada di main menu
        Time.timeScale = 1f;

        // Putar musik latar belakang main menu
        AudioManager.Instance.PlayMainMenuMusic();

        // Reset skor koin saat masuk main menu
        if (GameManager.Instance != null)
        {
            GameManager.Instance.ResetScore();
        }
    }

    private void OnGUI()
    {
        // 1. Gambar latar belakang (Gunakan texture jika dipasang di Inspector)
        if (backgroundTexture != null)
        {
            GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), backgroundTexture, ScaleMode.ScaleAndCrop);
        }
        else
        {
            // Fallback ke warna gelap transparan jika texture kosong
            Texture2D blackTexture = new Texture2D(1, 1);
            blackTexture.SetPixel(0, 0, new Color(0, 0, 0, 0.7f));
            blackTexture.Apply();
            
            GUIStyle bgStyle = new GUIStyle();
            bgStyle.normal.background = blackTexture;
            GUI.Box(new Rect(0, 0, Screen.width, Screen.height), GUIContent.none, bgStyle);
        }

        // 2. Ukuran jendela menu utama (disesuaikan dengan gaya panel kemenangan)
        float width = 450f;
        float height = 280f;
        float x = (Screen.width - width) / 2f;
        float y = (Screen.height - height) / 2f;

        // Gambar jendela utama
        GUI.Box(new Rect(x, y, width, height), "");

        // 3. Set style teks & tombol agar identik dengan panel kemenangan
        GUIStyle titleStyle = new GUIStyle();
        titleStyle.alignment = TextAnchor.MiddleCenter;
        titleStyle.fontSize = 32;
        titleStyle.fontStyle = FontStyle.Bold;
        titleStyle.normal.textColor = Color.yellow; // Warna Kuning Emas

        GUIStyle subTitleStyle = new GUIStyle();
        subTitleStyle.alignment = TextAnchor.MiddleCenter;
        subTitleStyle.fontSize = 16;
        subTitleStyle.normal.textColor = Color.white;

        GUIStyle buttonStyle = new GUIStyle(GUI.skin.button);
        buttonStyle.fontSize = 16;
        buttonStyle.fontStyle = FontStyle.Bold;

        // 4. Gambar Judul "SYNERGY" & Subtitle
        GUI.Label(new Rect(x, y + 40, width, 50), "SYNERGY", titleStyle);
        GUI.Label(new Rect(x, y + 95, width, 30), "A Co-op Elemental Adventure", subTitleStyle);

        // 5. Desain Tombol Aksi
        float btnWidth = 200f;
        float btnHeight = 45f;
        float btnX = x + (width - btnWidth) / 2f;

        // Tombol PLAY GAME
        if (GUI.Button(new Rect(btnX, y + 150, btnWidth, btnHeight), "Play Game", buttonStyle))
        {
            // Memuat level pertama (SampleScene)
            SceneManager.LoadScene("SampleScene");
        }

        // Tombol EXIT GAME
        if (GUI.Button(new Rect(btnX, y + 210, btnWidth, btnHeight), "Exit", buttonStyle))
        {
            Debug.Log("Keluar dari game...");
            Application.Quit();
        }
    }
}
