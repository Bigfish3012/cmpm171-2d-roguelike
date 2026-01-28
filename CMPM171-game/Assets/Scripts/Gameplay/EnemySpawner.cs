using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [Header("Prefabs")]
    public GameObject[] enemyPrefabs;

    [Header("Spawn Settings")]
    public Transform[] spawnPoints;
    public int spawnCount = 5;
    public float spawnInterval = 0.7f;

    void Start()
    {
        StartCoroutine(SpawnEnemies());
    }

    System.Collections.IEnumerator SpawnEnemies()
    {
        if (spawnPoints == null || spawnPoints.Length == 0) yield break;
        if (enemyPrefabs == null || enemyPrefabs.Length == 0) yield break;

        for (int i = 0; i < spawnCount; i++)
        {
            var sp = spawnPoints[Random.Range(0, spawnPoints.Length)];
            var prefab = enemyPrefabs[Random.Range(0, enemyPrefabs.Length)];
            Instantiate(prefab, sp.position, Quaternion.identity);
            
            yield return new WaitForSeconds(spawnInterval);
        }
    }
}
