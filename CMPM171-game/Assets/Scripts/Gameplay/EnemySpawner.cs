using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [Header("Prefabs")]
    public GameObject[] enemyPrefabs;

    [Header("Spawn Settings")]
    public float spawnInterval = 1f;
    public float spawnDistanceFromCamera = 8f; // Distance from camera edge to spawn enemies
    public Camera mainCamera;

    void Start()
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }
        StartCoroutine(SpawnEnemies());
    }

    System.Collections.IEnumerator SpawnEnemies()
    {
        if (enemyPrefabs == null || enemyPrefabs.Length == 0) yield break;
        if (mainCamera == null) yield break;

        // Infinite loop: spawn enemies continuously at random positions
        while (true)
        {
            Vector3 randomSpawnPosition = GetRandomSpawnPosition();
            var prefab = enemyPrefabs[Random.Range(0, enemyPrefabs.Length)];
            Instantiate(prefab, randomSpawnPosition, Quaternion.identity);
            
            yield return new WaitForSeconds(spawnInterval);
        }
    }

    Vector3 GetRandomSpawnPosition()
    {
        if (mainCamera == null) return Vector3.zero;

        // Get camera bounds
        float orthographicSize = mainCamera.orthographicSize;
        float aspect = mainCamera.aspect;
        float cameraWidth = orthographicSize * aspect;
        float cameraHeight = orthographicSize;

        Vector3 cameraPos = mainCamera.transform.position;

        // Randomly choose which edge to spawn from (0=top, 1=right, 2=bottom, 3=left)
        int edge = Random.Range(0, 4);
        Vector3 spawnPos = Vector3.zero;

        switch (edge)
        {
            case 0: // Top edge
                spawnPos = new Vector3(
                    cameraPos.x + Random.Range(-cameraWidth - spawnDistanceFromCamera, cameraWidth + spawnDistanceFromCamera),
                    cameraPos.y + cameraHeight + spawnDistanceFromCamera,
                    0f
                );
                break;
            case 1: // Right edge
                spawnPos = new Vector3(
                    cameraPos.x + cameraWidth + spawnDistanceFromCamera,
                    cameraPos.y + Random.Range(-cameraHeight - spawnDistanceFromCamera, cameraHeight + spawnDistanceFromCamera),
                    0f
                );
                break;
            case 2: // Bottom edge
                spawnPos = new Vector3(
                    cameraPos.x + Random.Range(-cameraWidth - spawnDistanceFromCamera, cameraWidth + spawnDistanceFromCamera),
                    cameraPos.y - cameraHeight - spawnDistanceFromCamera,
                    0f
                );
                break;
            case 3: // Left edge
                spawnPos = new Vector3(
                    cameraPos.x - cameraWidth - spawnDistanceFromCamera,
                    cameraPos.y + Random.Range(-cameraHeight - spawnDistanceFromCamera, cameraHeight + spawnDistanceFromCamera),
                    0f
                );
                break;
        }

        return spawnPos;
    }
}