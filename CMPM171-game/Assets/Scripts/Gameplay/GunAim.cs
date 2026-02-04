using UnityEngine;

public enum AimMode
{
    Auto,
    Manual
}

public class GunAim : MonoBehaviour
{
    [SerializeField] private Camera cam;
    [SerializeField] private Transform player;   // Player's Transform (parent object)
    [SerializeField] private float radius = 0.6f; // Distance of gun around player
    [SerializeField] private string enemyTag = "Enemies";
    [SerializeField] private float maxTargetRange = 50f; // Maximum range to target enemies
    [SerializeField] private AimMode currentAimMode = AimMode.Auto; // Current aiming mode

    void Awake()
    {
        if (cam == null) cam = Camera.main;
    }

    void Update()
    {
        // Toggle aim mode when R key is pressed
        if (Input.GetKeyDown(KeyCode.R))
        {
            currentAimMode = currentAimMode == AimMode.Auto ? AimMode.Manual : AimMode.Auto;
        }

        Vector3 targetPosition;

        // Choose target based on current aim mode
        if (currentAimMode == AimMode.Auto)
        {
            targetPosition = FindNearestEnemyPosition();
            
            // If no enemy found, use player's forward direction or default direction
            if (targetPosition == Vector3.zero)
            {
                targetPosition = player.position + Vector3.right;
            }
        }
        else // Manual mode
        {
            // Aim at mouse position in world space
            Vector3 mouseWorldPos = cam.ScreenToWorldPoint(Input.mousePosition);
            mouseWorldPos.z = 0f;
            targetPosition = mouseWorldPos;
        }

        Vector3 dir = (targetPosition - player.position);
        dir.z = 0f;

        // Let gun orbit around player (position around player in a circle)
        transform.position = player.position + dir.normalized * radius;

        // Rotate gun to face target
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);
    }

    private Vector3 FindNearestEnemyPosition()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag(enemyTag);
        if (enemies == null || enemies.Length == 0) return Vector3.zero;

        Transform nearestEnemy = null;
        float nearestDistance = float.MaxValue;

        foreach (GameObject enemy in enemies)
        {
            if (enemy == null || !enemy.activeInHierarchy) continue;

            float distance = Vector3.Distance(player.position, enemy.transform.position);
            if (distance < nearestDistance && distance <= maxTargetRange)
            {
                nearestDistance = distance;
                nearestEnemy = enemy.transform;
            }
        }

        return nearestEnemy != null ? nearestEnemy.position : Vector3.zero;
    }
}