using UnityEngine;

public class CompanionAI : MonoBehaviour
{
    [Header("=== AI SETTINGS ===")]
    [SerializeField] private float followDistance = 2.0f;
    [SerializeField] private float obstacleDetectionRange = 0.8f;
    [SerializeField] private float gapDetectionRange = 1.0f;

    private Transform target;
    private PlayerMovement playerMovement;
    private GolemBlue golemBlue;
    private Rigidbody2D rb;

    private bool isAIActive = false;

    private void Start()
    {
        playerMovement = GetComponent<PlayerMovement>();
        golemBlue = GetComponent<GolemBlue>();
        rb = GetComponent<Rigidbody2D>();

        // Set default state: AI is inactive initially, player controls manually
        SetAIActive(false);

        FindTarget();
    }

    private void Update()
    {
        // Toggle AI with 'C' Key
        if (Input.GetKeyDown(KeyCode.C))
        {
            SetAIActive(!isAIActive);
        }

        if (!isAIActive) return;

        if (target == null)
        {
            FindTarget();
            if (target == null) return;
        }

        float distanceX = target.position.x - transform.position.x;
        float moveInput = 0f;
        bool shouldJump = false;

        // 1. Follow horizontally if too far
        if (Mathf.Abs(distanceX) > followDistance)
        {
            moveInput = Mathf.Sign(distanceX);
        }

        // 2. Obstacle & Gap detection
        if (moveInput != 0)
        {
            // Raycast at multiple heights (foot and center) to detect walls/boxes
            Vector2 rayOriginFoot = (Vector2)transform.position + Vector2.down * 0.4f;
            Vector2 rayOriginCenter = (Vector2)transform.position;
            Vector2 direction = new Vector2(moveInput, 0);

            RaycastHit2D hitFoot = Physics2D.Raycast(rayOriginFoot, direction, obstacleDetectionRange);
            RaycastHit2D hitCenter = Physics2D.Raycast(rayOriginCenter, direction, obstacleDetectionRange);

            bool isGrounded = IsCharacterGrounded();

            if (isGrounded)
            {
                // If hitting an obstacle (that is not another player), jump
                if ((hitFoot.collider != null && !hitFoot.collider.CompareTag("Player")) ||
                    (hitCenter.collider != null && !hitCenter.collider.CompareTag("Player")))
                {
                    shouldJump = true;
                }

                // Gap (jurang) detection
                // Cast ray down from a point ahead of the character
                Vector2 gapCheckOrigin = (Vector2)transform.position + new Vector2(moveInput * gapDetectionRange, -0.5f);
                RaycastHit2D gapHit = Physics2D.Raycast(gapCheckOrigin, Vector2.down, 1.2f);
                
                // If there's no ground ahead, jump!
                if (gapHit.collider == null)
                {
                    shouldJump = true;
                }
            }
        }

        // 3. Vertical Jump to reach higher platform
        if (target.position.y - transform.position.y > 1.2f && Mathf.Abs(distanceX) < 4f && IsCharacterGrounded())
        {
            shouldJump = true;
        }

        // Send inputs to character controller
        SendInputs(moveInput, shouldJump);
    }

    private void FindTarget()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        foreach (GameObject player in players)
        {
            if (player != gameObject)
            {
                target = player.transform;
                break;
            }
        }
    }

    private bool IsCharacterGrounded()
    {
        // Simple raycast down from the bottom to check if grounded
        Vector2 rayOrigin = (Vector2)transform.position + Vector2.down * 0.5f;
        RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.down, 0.15f);
        return hit.collider != null;
    }

    private void SetAIActive(bool active)
    {
        isAIActive = active;
        if (playerMovement != null) playerMovement.isAIControlled = active;
        if (golemBlue != null) golemBlue.isAIControlled = active;

        Debug.Log($"{gameObject.name} AI Companion status: {active}");
    }

    private void SendInputs(float move, bool jump)
    {
        if (playerMovement != null) playerMovement.SetAIInputs(move, jump);
        if (golemBlue != null) golemBlue.SetAIInputs(move, jump);
    }

    private void OnGUI()
    {
        if (!isAIActive) return;

        // Draw visual label above head
        if (Camera.main != null)
        {
            Vector3 worldPos = transform.position + Vector3.up * 1.5f;
            Vector3 screenPos = Camera.main.WorldToScreenPoint(worldPos);

            // Check if behind camera
            if (screenPos.z > 0)
            {
                float x = screenPos.x;
                float y = Screen.height - screenPos.y;

                GUIStyle style = new GUIStyle();
                style.alignment = TextAnchor.MiddleCenter;
                style.normal.textColor = Color.cyan;
                style.fontSize = 14;
                style.fontStyle = FontStyle.Bold;

                // Add a drop shadow for premium readability
                GUIStyle shadowStyle = new GUIStyle(style);
                shadowStyle.normal.textColor = Color.black;

                GUI.Label(new Rect(x - 49, y - 9, 100, 20), "AI COMPANION", shadowStyle);
                GUI.Label(new Rect(x - 50, y - 10, 100, 20), "AI COMPANION", style);
            }
        }
    }
}
