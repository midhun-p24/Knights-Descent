using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraController : MonoBehaviour
{
    [Header("Follow Target")]
    [Tooltip("The target for the camera to follow. If empty, it will try to find the Player automatically.")]
    public Transform target;
    [Tooltip("How smoothly the camera follows the target. Lower values are slower and smoother.")]
    public float smoothSpeed = 0.125f;
    [Tooltip("The offset from the target's position (useful for 3D, can be kept at 0 for 2D).")]
    public Vector3 offset;

    [Header("Zoom Controls (Z-Axis)")]
    [Tooltip("How fast the camera zooms in and out by moving along the Z-axis.")]
    public float zoomSpeed = 4f;
    [Tooltip("The most 'zoomed out' the camera can be (e.g., -20).")]
    public float minZ = -20f;
    [Tooltip("The most 'zoomed in' the camera can be (e.g., -5).")]
    public float maxZ = -5f;

    // LateUpdate is called after all Update functions have been called.
    // This is the best place for camera movement to avoid jittery visuals.
    void LateUpdate()
    {
        // If the target is not set, try to find the PlayerController instance
        if (target == null)
        {
            if (PlayerController.Instance != null)
            {
                target = PlayerController.Instance.transform;
            }
            else
            {
                // If the player can't be found, do nothing.
                Debug.LogWarning("Camera cannot find a target to follow.");
                return;
            }
        }

        // --- Handle Camera Zoom ---
        // Get input from the mouse scroll wheel.
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        // Calculate the new desired Z position. Scrolling up (positive) moves the camera closer to the target.
        float desiredZ = transform.position.z + scroll * zoomSpeed;
        // Clamp the zoom level to stay within our min and max Z bounds.
        desiredZ = Mathf.Clamp(desiredZ, minZ, maxZ);


        // --- Handle Camera Following ---
        // Define the camera's desired X and Y position based on the target's position.
        Vector3 desiredXY = target.position + offset;

        // Combine the desired X, Y, and the new Z into one target position.
        Vector3 desiredPosition = new Vector3(desiredXY.x, desiredXY.y, desiredZ);

        // Smoothly move the camera from its current position to the desired position.
        // Vector3.Lerp will handle smoothing all three axes at once.
        transform.position = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);
    }
}
