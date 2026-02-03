using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [Header("Prefabs")]
    public GameObject[] enemyPrefabs;

    [Header("Spawn Settings")]
    public Transform[] spawnPoints;
    public float spawnInterval = 1f;

    void Start()
    {
        StartCoroutine(SpawnEnemies());
    }

    System.Collections.IEnumerator SpawnEnemies()
    {
        if (spawnPoints == null || spawnPoints.Length == 0) yield break;
        if (enemyPrefabs == null || enemyPrefabs.Length == 0) yield break;

        // Infinite loop: spawn enemies continuously
        while (true)
        {
            var sp = spawnPoints[Random.Range(0, spawnPoints.Length)];
            var prefab = enemyPrefabs[Random.Range(0, enemyPrefabs.Length)];
            Instantiate(prefab, sp.position, Quaternion.identity);
            
            yield return new WaitForSeconds(spawnInterval);
        }
    }
}
