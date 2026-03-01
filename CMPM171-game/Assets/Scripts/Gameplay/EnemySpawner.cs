using System.Collections;
using UnityEngine;

/// <summary>
/// Spawns enemies for a given wave. Wave flow and UI are handled by
/// <see cref="WaveFlowController"/> which calls <see cref="StartWave"/>.
/// </summary>
public class EnemySpawner : MonoBehaviour
{
    [Header("Prefabs")]
    [SerializeField] private GameObject[] enemyPrefabs;                                   // Enemy prefabs to spawn

    [Header("Spawn Position")]
    [SerializeField] private Transform mapSpawnBounds;                                     // Bounds object used for in-map spawning
    [SerializeField] private string mapSpawnBoundsName = "ground";                         // Auto-bind fallback when reference is empty
    [SerializeField] private float spawnBoundsPadding = 0.5f;                              // Inset from map edges to avoid clipping spawns
    [SerializeField] private Transform spawnExclusionZone;                                  // Object whose bounds enemies must not spawn inside
    [SerializeField] private float exclusionPadding = 0.5f;                                // Extra margin around each exclusion zone
    [SerializeField] private int maxSpawnRetries = 30;                                     // Max retries to find a valid spawn position

    [Header("Wave Scaling")]
    [SerializeField] private int startingWaveEnemyCount = 30;                              // Wave 1 enemy count
    [SerializeField] private int enemyCountIncreasePerWave = 10;                           // +10 enemies per new wave
    [SerializeField] private float startingSpawnInterval = 2f;                             // Wave 1 spawn interval
    [SerializeField] private float spawnIntervalDecreasePerWave = 0.1f;                    // Spawn interval decrease each wave
    [SerializeField] private float minimumSpawnInterval = 0.5f;                            // Minimum spawn interval cap
    [SerializeField] private float enemyHealthIncreasePercentPerWave = 30f;                // Enemy health increase per wave (%)
    [SerializeField] private int enemyDamageIncreasePerWave = 1;                           // Enemy damage increase per wave

    [Header("Debug")]
    [SerializeField] private bool logWaveDebug = true;                                     // Print spawn/death counts in Console

    /// <summary>Raised when all enemies in the current wave have been destroyed.
    /// Parameter is the wave number that was cleared.</summary>
    public event System.Action<int> OnWaveCleared;

    private int currentWaveNumber = 0;
    private int aliveEnemies = 0;
    private int enemiesLeftToSpawnInWave = 0;
    private bool spawning = false;
    private bool missingMapBoundsWarningShown = false;
    private Coroutine activeSpawnRoutine;

    private void Start()
    {
        TryAutoBindSpawnBounds();
    }

    /// <summary>Begin spawning enemies for the given wave number.</summary>
    public void StartWave(int waveNumber)
    {
        if (enemyPrefabs == null || enemyPrefabs.Length == 0) return;

        if (spawning)
        {
            LogWave($"StartWave({waveNumber}) called while wave {currentWaveNumber} is still spawning. Stopping previous routine.");
            if (activeSpawnRoutine != null)
                StopCoroutine(activeSpawnRoutine);
            spawning = false;
            enemiesLeftToSpawnInWave = 0;
        }

        currentWaveNumber = waveNumber;
        activeSpawnRoutine = StartCoroutine(SpawnWaveRoutine(waveNumber));
    }

    // Called by EnemyWaveMember when one spawned enemy gets destroyed.
    public void NotifyEnemyDestroyed()
    {
        if (aliveEnemies > 0)
            aliveEnemies--;

        LogWave($"Enemy destroyed. Alive: {aliveEnemies}, Left: {enemiesLeftToSpawnInWave}");
        CheckWaveCleared();
    }

    // ---- Spawn coroutine ----

    private IEnumerator SpawnWaveRoutine(int waveNumber)
    {
        spawning = true;

        int enemyCount = startingWaveEnemyCount + (waveNumber - 1) * enemyCountIncreasePerWave;
        float healthMultiplier = 1f + (waveNumber - 1) * (enemyHealthIncreasePercentPerWave / 100f);
        int damageBonus = (waveNumber - 1) * enemyDamageIncreasePerWave;
        float spawnInterval = Mathf.Max(
            minimumSpawnInterval,
            startingSpawnInterval - (waveNumber - 1) * spawnIntervalDecreasePerWave
        );
        enemiesLeftToSpawnInWave = enemyCount;

        LogWave(
            $"Wave {waveNumber} spawning. Count: {enemyCount}, " +
            $"Health x{healthMultiplier:0.00}, Damage +{damageBonus}, " +
            $"Interval {spawnInterval:0.00}s"
        );

        for (int i = 0; i < enemyCount; i++)
        {
            Vector3 spawnPos = GetRandomSpawnPosition();
            GameObject prefab = enemyPrefabs[Random.Range(0, enemyPrefabs.Length)];
            GameObject enemy = Instantiate(prefab, spawnPos, Quaternion.identity);
            ApplyWaveScalingToEnemy(enemy, healthMultiplier, damageBonus);

            aliveEnemies++;
            enemiesLeftToSpawnInWave--;

            EnemyWaveMember waveMember = enemy.GetComponent<EnemyWaveMember>();
            if (waveMember == null)
                waveMember = enemy.AddComponent<EnemyWaveMember>();
            waveMember.Init(this);

            LogWave($"Spawned {i + 1}/{enemyCount}. Alive: {aliveEnemies}, Left: {enemiesLeftToSpawnInWave}");

            if (spawnInterval > 0f)
                yield return new WaitForSeconds(spawnInterval);
        }

        spawning = false;
        activeSpawnRoutine = null;
        CheckWaveCleared();
    }

    private void CheckWaveCleared()
    {
        if (spawning) return;
        if (enemiesLeftToSpawnInWave > 0 || aliveEnemies > 0) return;

        LogWave($"Wave {currentWaveNumber} cleared.");
        OnWaveCleared?.Invoke(currentWaveNumber);
    }

    // ---- Spawn position ----

    private Vector3 GetRandomSpawnPosition()
    {
        if (TryGetMapSpawnBounds(out Bounds bounds))
        {
            float minX = bounds.min.x + spawnBoundsPadding;
            float maxX = bounds.max.x - spawnBoundsPadding;
            float minY = bounds.min.y + spawnBoundsPadding;
            float maxY = bounds.max.y - spawnBoundsPadding;

            if (minX > maxX) { float c = bounds.center.x; minX = c; maxX = c; }
            if (minY > maxY) { float c = bounds.center.y; minY = c; maxY = c; }

            for (int attempt = 0; attempt < maxSpawnRetries; attempt++)
            {
                Vector3 candidate = new Vector3(Random.Range(minX, maxX), Random.Range(minY, maxY), 0f);
                if (!IsInsideExclusionZone(candidate))
                    return candidate;
            }

            return new Vector3(Random.Range(minX, maxX), Random.Range(minY, maxY), 0f);
        }

        if (!missingMapBoundsWarningShown)
        {
            Debug.LogWarning("[EnemySpawner] mapSpawnBounds not set. Falling back to spawner position.", this);
            missingMapBoundsWarningShown = true;
        }
        return transform.position;
    }

    private bool IsInsideExclusionZone(Vector3 point)
    {
        if (spawnExclusionZone == null) return false;
        if (!TryGetBounds(spawnExclusionZone, out Bounds b)) return false;

        Bounds expanded = b;
        expanded.Expand(exclusionPadding * 2f);
        return expanded.Contains(point);
    }

    private static bool TryGetBounds(Transform target, out Bounds bounds)
    {
        bounds = default;
        if (target == null) return false;

        SpriteRenderer sr = target.GetComponent<SpriteRenderer>();
        if (sr != null) { bounds = sr.bounds; return true; }

        Collider2D col = target.GetComponent<Collider2D>();
        if (col != null) { bounds = col.bounds; return true; }

        return false;
    }

    private bool TryGetMapSpawnBounds(out Bounds bounds)
    {
        return TryGetBounds(mapSpawnBounds, out bounds);
    }

    // ---- Auto-bind ----

    private void TryAutoBindSpawnBounds()
    {
        if (mapSpawnBounds != null || string.IsNullOrEmpty(mapSpawnBoundsName))
            return;

        GameObject boundObject = SceneSearchHelper.FindSceneObjectByName(mapSpawnBoundsName);
        if (boundObject != null)
            mapSpawnBounds = boundObject.transform;
    }

    // ---- Utility ----

    private void LogWave(string message)
    {
        if (!logWaveDebug) return;
        Debug.Log($"[EnemySpawner] {message}", this);
    }

    private static void ApplyWaveScalingToEnemy(GameObject enemy, float healthMultiplier, int damageBonus)
    {
        if (enemy == null) return;

        Enemy1 enemy1 = enemy.GetComponent<Enemy1>();
        if (enemy1 != null) { enemy1.ApplyWaveScaling(healthMultiplier, damageBonus); return; }

        Enemy_shooter enemyShooter = enemy.GetComponent<Enemy_shooter>();
        if (enemyShooter != null) { enemyShooter.ApplyWaveScaling(healthMultiplier, damageBonus); return; }

        Enemy_Charge enemyCharge = enemy.GetComponent<Enemy_Charge>();
        if (enemyCharge != null)
            enemyCharge.ApplyWaveScaling(healthMultiplier, damageBonus);
    }
}
