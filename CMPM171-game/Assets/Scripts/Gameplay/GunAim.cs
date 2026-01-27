using UnityEngine;

public class GunAim : MonoBehaviour
{
    [SerializeField] private Camera cam;
    [SerializeField] private Transform player;   // Player's Transform (parent object)
    [SerializeField] private float radius = 0.6f; // Distance of gun around player

    void Awake()
    {
        if (cam == null) cam = Camera.main;
    }

    void Update()
    {
        Vector3 mouseWorld = cam.ScreenToWorldPoint(Input.mousePosition);
        mouseWorld.z = 0f;

        Vector3 dir = (mouseWorld - player.position);
        dir.z = 0f;

        // Let gun orbit around player (position around player in a circle)
        transform.position = player.position + dir.normalized * radius;

        // Rotate gun to face mouse
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);
    }
}
