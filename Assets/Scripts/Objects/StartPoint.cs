using UnityEngine;

public class StartPoint : MonoBehaviour
{
    void Start()
    {
        // Cari objek dengan tag "Player" di dalam game
        GameObject player = GameObject.FindGameObjectWithTag("Player");

        if (player != null)
        {
            // Pindahkan posisi player tepat ke posisi portal ini
            player.transform.position = transform.position;
        }
    }
}