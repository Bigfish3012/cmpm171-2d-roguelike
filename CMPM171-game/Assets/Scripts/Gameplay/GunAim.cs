using UnityEngine;

public class GunAim : MonoBehaviour
{
    [SerializeField] private Transform player;                                           // Player's Transform (parent object)
    [SerializeField] private float radius = 0.6f;                                        // Distance of gun around player
    [SerializeField] private string enemyTag = "Enemies";                               // Tag of enemy objects
    [SerializeField] private float maxTargetRange = 50f;                                 // Maximum range to target enemies
    [SerializeField] private SpriteRenderer gunSpriteRenderer;                            // Sprite renderer for gun visibility

    public bool HasTarget { get; private set; }                                           // Whether a valid enemy target exists

    void Awake()
    {
        if (gunSpriteRenderer == null)
            gunSpriteRenderer = GetComponent<SpriteRenderer>();
    }

    // Update method to handle aiming and gun positioning (always auto-aim at nearest enemy)
    void Update()
    {
        Vector3 targetPosition = FindNearestEnemyPosition();
        HasTarget = targetPosition != Vector3.zero;
        if (gunSpriteRenderer != null)
            gunSpriteRenderer.enabled = HasTarget;

        // If no target exists, keep gun hidden and skip reposition/rotation updates.
        if (!HasTarget) return;

        Vector3 dir = (targetPosition - player.position);
        dir.z = 0f;

        // Let gun orbit around player (position around player in a circle)
        transform.position = player.position + dir.normalized * radius;

        // Rotate gun to face target
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);
    }

    // FindNearestEnemyPosition method to find the nearest enemy within range
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