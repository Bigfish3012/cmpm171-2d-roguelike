using UnityEngine;

public class FireBallSpawner : MonoBehaviour
{
    public GameObject fireBallPrefab;

    // Spawn Area
    public BoxCollider2D spawnArea;

    public float spawnInterval = 30f;

    float timer = 0f;

    void Update()
    {
        timer += Time.deltaTime;

        if (timer >= spawnInterval)
        {
            SpawnFireBall();
            timer = 0f;
        }
    }

    void SpawnFireBall()
    {
        if (spawnArea == null) return;

        Bounds bounds = spawnArea.bounds;

        float randomX = UnityEngine.Random.Range(bounds.min.x, bounds.max.x);

        float spawnY = bounds.max.y + 2f;

        Vector3 spawnPos = new Vector3(randomX, spawnY, 0);

        Instantiate(fireBallPrefab, spawnPos, Quaternion.identity);
    }
}