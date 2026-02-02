using UnityEngine;

// Spawns enemies at random positions around the player. Assign prefabs in Inspector.
public class EnemySpawner : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Transform player;

    [Header("Prefabs")]
    [SerializeField] private GameObject enemy1Prefab;
    [SerializeField] private GameObject enemy2Prefab;

    [Header("Spawn area (around player)")]
    [SerializeField] private float minDistance = 5f;
    [SerializeField] private float maxDistance = 12f;

    [Header("Timing")]
    [SerializeField] private float spawnInterval = 4f;
    [SerializeField] private float firstSpawnDelay = 2f;

    [Header("Limit (0 = no limit)")]
    [SerializeField] private int maxEnemies = 15;

    void Start()
    {
        if (player == null)
        {
            var go = GameObject.FindGameObjectWithTag("Player");
            if (go != null) player = go.transform;
        }
        InvokeRepeating(nameof(SpawnOne), firstSpawnDelay, spawnInterval);
    }

    void SpawnOne()
    {
        if (player == null) return;
        if (maxEnemies > 0 && CountEnemies() >= maxEnemies) return;

        GameObject prefab = Random.value < 0.5f ? enemy1Prefab : enemy2Prefab;
        if (prefab == null) return;

        float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
        float dist = Random.Range(minDistance, maxDistance);
        Vector2 offset = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * dist;
        Vector3 pos = player.position + new Vector3(offset.x, offset.y, 0f);

        Instantiate(prefab, pos, Quaternion.identity);
    }

    int CountEnemies()
    {
        return FindObjectsByType<EnemyHealth>(FindObjectsSortMode.None).Length;
    }
}
