using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [SerializeField] private Transform target;                                            // The target to follow (usually the player)
    [SerializeField] private float smoothSpeed = 0;                                      // How smoothly the camera follows (lower = smoother)
    [SerializeField] private Vector3 offset = Vector3.zero;                             // Offset from the target position
    
    private Vector3 velocity = Vector3.zero;                                             // Velocity reference for SmoothDamp

    // Awake method to find the player if no target is assigned
    void Awake()
    {
        // If no target is assigned, get player transform from singleton
        if (target == null && Player_settings.Instance != null)
        {
            target = Player_settings.Instance.PlayerTransform;
        }
    }

    // LateUpdate method to smoothly follow the target
    void LateUpdate()
    {
        if (target == null)
        {
            if (Player_settings.Instance != null)
                target = Player_settings.Instance.PlayerTransform;
            if (target == null) return;
        }

        // Calculate desired position
        Vector3 desiredPosition = target.position + offset;
        desiredPosition.z = transform.position.z; // Keep camera's z position unchanged

        // Smoothly move camera towards target
        transform.position = Vector3.SmoothDamp(transform.position, desiredPosition, ref velocity, smoothSpeed);
    }
}
