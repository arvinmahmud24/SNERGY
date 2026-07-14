using UnityEngine;

public class CameraFollowTwoPlayers : MonoBehaviour
{
    public Transform player1;
    public Transform player2;

    [Header("Movement")]
    public float smoothSpeed = 5f;

    [Header("Zoom")]
    public float minZoom = 5f;
    public float maxZoom = 10f;
    public float zoomLimiter = 10f;

    [Header("Batas Kamera")]
    public float maxPlayerDistance = 15f;

    private Camera cam;

    void Start()
    {
        cam = GetComponent<Camera>();
    }

    void LateUpdate()
    {
        if (player1 == null || player2 == null)
            return;

        Rigidbody2D rb1 = player1.GetComponent<Rigidbody2D>();
        Rigidbody2D rb2 = player2.GetComponent<Rigidbody2D>();

        bool p1Static = rb1 != null && rb1.bodyType == RigidbodyType2D.Static;
        bool p2Static = rb2 != null && rb2.bodyType == RigidbodyType2D.Static;

        if (!p1Static && !p2Static)
        {
            // Kedua player aktif, jalankan pembatasan jarak dan ikuti titik tengah
            LimitPlayersDistance();
            Move();
            Zoom();
        }
        else if (p1Static && !p2Static)
        {
            // Player 1 sudah di portal, ikuti Player 2 saja
            MoveToTarget(player2.position);
            ResetZoom();
        }
        else if (!p1Static && p2Static)
        {
            // Player 2 sudah di portal, ikuti Player 1 saja
            MoveToTarget(player1.position);
            ResetZoom();
        }
    }

    void Move()
    {
        Vector3 centerPoint = (player1.position + player2.position) / 2f;
        Vector3 newPosition = new Vector3(centerPoint.x, centerPoint.y, transform.position.z);

        transform.position = Vector3.Lerp(transform.position, newPosition, smoothSpeed * Time.deltaTime);
    }

    void MoveToTarget(Vector3 targetPosition)
    {
        Vector3 newPosition = new Vector3(targetPosition.x, targetPosition.y, transform.position.z);
        transform.position = Vector3.Lerp(transform.position, newPosition, smoothSpeed * Time.deltaTime);
    }

    void Zoom()
    {
        float distance = Vector2.Distance(player1.position, player2.position);
        float newZoom = Mathf.Lerp(minZoom, maxZoom, distance / zoomLimiter);

        newZoom = Mathf.Clamp(newZoom, minZoom, maxZoom);
        cam.orthographicSize = Mathf.Lerp(cam.orthographicSize, newZoom, smoothSpeed * Time.deltaTime);
    }

    void ResetZoom()
    {
        cam.orthographicSize = Mathf.Lerp(cam.orthographicSize, minZoom, smoothSpeed * Time.deltaTime);
    }

    void LimitPlayersDistance()
    {
        float distance = Vector2.Distance(player1.position, player2.position);

        if (distance > maxPlayerDistance)
        {
            Vector3 center = (player1.position + player2.position) / 2f;
            Vector3 dir1 = (player1.position - center).normalized;
            Vector3 dir2 = (player2.position - center).normalized;

            player1.position = center + dir1 * (maxPlayerDistance / 2f);
            player2.position = center + dir2 * (maxPlayerDistance / 2f);
        }
    }
}