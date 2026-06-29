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
    public float maxPlayerDistance = 12f;

    private Camera cam;

    void Start()
    {
        cam = GetComponent<Camera>();
    }

    void LateUpdate()
    {
        if (player1 == null || player2 == null)
            return;

        LimitPlayersDistance();
        Move();
        Zoom();
    }

    void Move()
    {
        Vector3 centerPoint = (player1.position + player2.position) / 2f;
        Vector3 newPosition = new Vector3(centerPoint.x, centerPoint.y, transform.position.z);

        transform.position = Vector3.Lerp(transform.position, newPosition, smoothSpeed * Time.deltaTime);
    }

    void Zoom()
    {
        float distance = Vector2.Distance(player1.position, player2.position);
        float newZoom = Mathf.Lerp(minZoom, maxZoom, distance / zoomLimiter);

        newZoom = Mathf.Clamp(newZoom, minZoom, maxZoom);
        cam.orthographicSize = Mathf.Lerp(cam.orthographicSize, newZoom, smoothSpeed * Time.deltaTime);
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