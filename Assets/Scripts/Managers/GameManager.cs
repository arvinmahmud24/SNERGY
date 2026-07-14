using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    private static GameManager _instance;
    public static GameManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindAnyObjectByType<GameManager>();

                if (_instance == null)
                {
                    GameObject go = new GameObject("GameManager");
                    _instance = go.AddComponent<GameManager>();
                    DontDestroyOnLoad(go);
                }
            }
            return _instance;
        }
    }

    [SerializeField] private SwapSystem swapSystem;
    [SerializeField] private int currentLevel = 1;

    public int skeletonScore { get; private set; } = 0;
    public int golemScore { get; private set; } = 0;

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (_instance != this)
        {
            Destroy(gameObject);
        }
    }

    public void AddSkeletonCoin(int amount)
    {
        skeletonScore += amount;
    }

    public void AddGolemCoin(int amount)
    {
        golemScore += amount;
    }

    public void ResetScore()
    {
        skeletonScore = 0;
        golemScore = 0;
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