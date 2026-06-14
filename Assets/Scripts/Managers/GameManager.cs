using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [SerializeField] private SwapSystem swapSystem;
    [SerializeField] private int currentLevel = 1;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

        DontDestroyOnLoad(gameObject);
    }

    public void RestartLevel()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void LoadLevel(int levelNumber)
    {
        currentLevel = levelNumber;
        SceneManager.LoadScene($"World_{levelNumber}_Level");
    }

    public void GameOver()
    {
        Time.timeScale = 0; // Pause game
        Debug.Log("GAME OVER!");
    }

    public void Resume()
    {
        Time.timeScale = 1;
    }
}