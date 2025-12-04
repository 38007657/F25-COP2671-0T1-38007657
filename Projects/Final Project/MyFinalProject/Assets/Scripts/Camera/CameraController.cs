using UnityEngine;

public class PixelPerfectCameraFollow : MonoBehaviour
{
    public Transform target;

    [Header("World Boundaries")]
    public float leftBoundary = -23f;
    public float rightBoundary = 13f;
    public float bottomBoundary = -5f;
    public float topBoundary = 13f;

    private float cameraHalfWidth;
    private float cameraHalfHeight;

    void Start()
    {
        // Calculate camera half-extents (for 320x180 at 16 PPU)
        cameraHalfWidth = 320f / 2f / 16f;  // 10 units
        cameraHalfHeight = 180f / 2f / 16f; // 5.625 units
    }

    void LateUpdate()
    {
        if (target == null) return;

        // Start by centering on the player
        Vector3 desiredPosition = target.position;
        desiredPosition.z = transform.position.z;

        // Stop camera from showing past world boundaries
        // Left boundary check
        if (desiredPosition.x - cameraHalfWidth < leftBoundary)
            desiredPosition.x = leftBoundary + cameraHalfWidth;

        // Right boundary check
        if (desiredPosition.x + cameraHalfWidth > rightBoundary)
            desiredPosition.x = rightBoundary - cameraHalfWidth;

        // Bottom boundary check
        if (desiredPosition.y - cameraHalfHeight < bottomBoundary)
            desiredPosition.y = bottomBoundary + cameraHalfHeight;

        // Top boundary check
        if (desiredPosition.y + cameraHalfHeight > topBoundary)
            desiredPosition.y = topBoundary - cameraHalfHeight;

        transform.position = desiredPosition;
    }
}