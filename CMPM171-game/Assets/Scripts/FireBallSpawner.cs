using UnityEngine;

public class FireBallSpawner : MonoBehaviour
{
    public GameObject fireBallPrefab;     // 火球 prefab
    public BoxCollider2D spawnArea;       // 生成区域

    public float spawnInterval = 1f;      // 生成间隔
    public float spawnHeight = 0.2f;      // 生成高度（可在 Inspector 调整）

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
        if (spawnArea == null || fireBallPrefab == null)
            return;

        // 获取生成区域中心和大小
        Vector2 size = spawnArea.size;
        Vector3 center = spawnArea.transform.position;

        // 随机 X 位置
        float randomX = UnityEngine.Random.Range(
            center.x - size.x / 2f,
            center.x + size.x / 2f
        );

        // 计算生成高度
        float spawnY = center.y + size.y / 2f + spawnHeight;

        Vector3 spawnPos = new Vector3(randomX, spawnY, 0f);

        Instantiate(fireBallPrefab, spawnPos, Quaternion.identity);
    }
}