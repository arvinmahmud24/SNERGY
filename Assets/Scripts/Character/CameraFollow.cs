using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target; // Objek yang akan diikuti (Player)
    public float smoothSpeed = 5f; // Seberapa mulus pergerakan kamera
    public Vector3 offset = new Vector3(0f, 1f, -10f); // Jarak kamera dari Player

    void LateUpdate()
    {
        // Pastikan target tidak kosong agar tidak error
        if (target != null)
        {
            // Menghitung posisi tujuan kamera (Posisi player + offset)
            Vector3 desiredPosition = target.position + offset;

            // Membuat pergerakan menyusul lebih mulus menggunakan Lerp
            Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);

            // Mengubah posisi kamera ke posisi yang sudah dihaluskan
            transform.position = smoothedPosition;
        }
    }
}