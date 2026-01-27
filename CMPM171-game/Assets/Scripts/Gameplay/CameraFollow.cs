using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [SerializeField] private Transform target;           // The target to follow (usually the player)
    [SerializeField] private float smoothSpeed = 0; // How smoothly the camera follows (lower = smoother)
    [SerializeField] private Vector3 offset = Vector3.zero; // Offset from the target position
    
    private Vector3 velocity = Vector3.zero;

    void Awake()
    {
        // If no target is assigned, try to find the player
        if (target == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                target = player.transform;
            }
        }
    }

    void LateUpdate()
    {
        if (target == null) return;

        // Calculate desired position
        Vector3 desiredPosition = target.position + offset;
        desiredPosition.z = transform.position.z; // Keep camera's z position unchanged

        // Smoothly move camera towards target
        transform.position = Vector3.SmoothDamp(transform.position, desiredPosition, ref velocity, smoothSpeed);
    }
}
