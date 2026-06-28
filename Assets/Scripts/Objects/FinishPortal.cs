using UnityEngine;
using UnityEngine.SceneManagement;

public class FinishPortal : MonoBehaviour
{
    [SerializeField] private string nextLevelName;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            SceneManager.LoadScene(nextLevelName);
        }
    }
}