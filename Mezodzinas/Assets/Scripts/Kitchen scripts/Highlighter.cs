using UnityEngine;

public class Highlighter : MonoBehaviour
{
    private LineRenderer lineRenderer; // Global LineRenderer
    private BoxCollider2D currentCollider; // The currently highlighted BoxCollider2D
    private Camera mainCamera;

    // Minimum and maximum line width
    private const float minLineWidth = 0.01f;
    private const float maxLineWidth = 0.05f;

    void Awake()
    {
        mainCamera = Camera.main;

        // Create and configure the global LineRenderer
        GameObject lineRendererObject = new GameObject("HighlightLineRenderer");
        lineRenderer = lineRendererObject.AddComponent<LineRenderer>();

        // Set general properties for the LineRenderer
        lineRenderer.positionCount = 5; // 4 corners + 1 to close the loop
        lineRenderer.useWorldSpace = true;

        // Material setup for proper visibility
        Material lineMaterial = new Material(Shader.Find("Sprites/Default"));
        lineMaterial.color = Color.yellow; // Highlight color
        lineRenderer.material = lineMaterial;

        // Ensure it's drawn in front of everything
        lineRenderer.sortingLayerName = "Foreground";
        lineRenderer.sortingOrder = 1000;

        // LineRenderer starts disabled
        lineRenderer.enabled = false;
    }

    void Update()
    {
        // Perform 2D raycast to detect which BoxCollider2D the mouse is over
        Vector2 mousePosition = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        Collider2D hit = Physics2D.OverlapPoint(mousePosition);

        if (hit != null && hit is BoxCollider2D)
        {
            BoxCollider2D collider = (BoxCollider2D)hit;

            // Check if this is a new collider
            if (collider != currentCollider)
            {
                currentCollider = collider;
                ShowHighlight(collider);
            }
        }
        else if (currentCollider != null)
        {
            // No collider under the mouse anymore
            HideHighlight();
        }
    }

    private void ShowHighlight(BoxCollider2D collider)
    {
        // Enable the LineRenderer
        lineRenderer.enabled = true;

        // Get the world space dimensions and corners of the BoxCollider2D
        Vector3[] corners = GetColliderWorldCorners(collider);

        // Set the LineRenderer positions
        lineRenderer.SetPositions(corners);

        // Dynamically adjust line thickness based on the collider's size
        float averageSize = (collider.size.x + collider.size.y) / 2f; // Average the width and height
        float dynamicLineWidth = Mathf.Clamp(averageSize * 0.1f, minLineWidth, maxLineWidth);
        lineRenderer.startWidth = dynamicLineWidth;
        lineRenderer.endWidth = dynamicLineWidth;
    }

    private void HideHighlight()
    {
        // Disable the highlight
        lineRenderer.enabled = false;
        currentCollider = null;
    }

    private Vector3[] GetColliderWorldCorners(BoxCollider2D collider)
    {
        // Get the world space center and size of the collider
        Vector3 center = collider.transform.TransformPoint(collider.offset);
        Vector3 right = collider.transform.right * collider.size.x * 0.5f * collider.transform.localScale.x;
        Vector3 up = collider.transform.up * collider.size.y * 0.5f * collider.transform.localScale.y;

        // Calculate world corners, accounting for scale and rotation
        Vector3[] corners = new Vector3[5]
        {
            center - right - up,    // Bottom-left
            center + right - up,    // Bottom-right
            center + right + up,    // Top-right
            center - right + up,    // Top-left
            center - right - up     // Close the loop
        };

        return corners;
    }
}