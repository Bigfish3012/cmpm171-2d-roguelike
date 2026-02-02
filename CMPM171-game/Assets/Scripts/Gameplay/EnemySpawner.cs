using UnityEngine;

// Spawns enemies around the player. Spawn interval shortens every N seconds (e.g. every 5 min).
// Exposes elapsed time for UI via GetTimeString().
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
    [SerializeField] private float secondsPerStep = 300f;

    [Header("Limit (0 = no limit)")]
    [SerializeField] private int maxEnemies = 15;

    private float elapsedSeconds;
    private int lastStep = -1;

    public float ElapsedSeconds => elapsedSeconds;

    int FiveMinuteStep => secondsPerStep > 0 ? Mathf.FloorToInt(elapsedSeconds / secondsPerStep) : 0;

    float SpawnIntervalMultiplier
    {
        get
        {
            int step = FiveMinuteStep;
            if (step <= 0) return 1f;
            return 1f / (1f + step * 0.25f);
        }
    }

    void Start()
    {
        if (player == null)
        {
            var go = GameObject.FindGameObjectWithTag("Player");
            if (go != null) player = go.transform;
        }
        ApplySpawnInterval(0);
    }

    void Update()
    {
        elapsedSeconds += Time.deltaTime;
        int step = FiveMinuteStep;
        if (step != lastStep)
        {
            lastStep = step;
            ApplySpawnInterval(step);
        }
    }

    void ApplySpawnInterval(int fiveMinuteStep)
    {
        CancelInvoke(nameof(SpawnOne));
        float interval = Mathf.Max(0.5f, spawnInterval * SpawnIntervalMultiplier);
        float delay = fiveMinuteStep > 0 ? interval : firstSpawnDelay;
        InvokeRepeating(nameof(SpawnOne), delay, interval);
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

    // For UI: MM:SS or HH:MM:SS
    public string GetTimeString()
    {
        int totalSeconds = Mathf.FloorToInt(elapsedSeconds);
        int hours = totalSeconds / 3600;
        int minutes = (totalSeconds % 3600) / 60;
        int seconds = totalSeconds % 60;
        if (hours > 0)
            return $"{hours:D2}:{minutes:D2}:{seconds:D2}";
        return $"{minutes:D2}:{seconds:D2}";
    }
}
