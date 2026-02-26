using System.Collections;
using TMPro;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [Header("Prefabs")]
    [SerializeField] private GameObject[] enemyPrefabs;                                   // Enemy prefabs to spawn

    [Header("Spawn Position")]
    [SerializeField] private float spawnDistanceFromCamera = 2f;                          // Distance from camera edge to spawn enemies
    [SerializeField] private Camera mainCamera;                                            // Main camera reference
    [SerializeField] private float minSpawnDistanceFromPlayer = 6f;                        // Minimum distance from player
    [SerializeField] private float maxSpawnDistanceFromPlayer = 10f;                       // Maximum distance from player

    [Header("Wave Settings")]
    [SerializeField] private int startingWaveEnemyCount = 50;                             // Wave 1 enemy count
    [SerializeField] private int enemyCountIncreasePerWave = 10;                          // +10 enemies per new wave
    [SerializeField] private float spawnInterval = 0.08f;                                 // Delay between each enemy spawn in a wave
    [SerializeField] private float nextWaveDelay = 5f;                                    // Delay before next wave starts

    [Header("Debug")]
    [SerializeField] private bool logWaveDebug = true;                                     // Print spawn/death counts in Console

    [Header("UI")]
    [SerializeField] private TextMeshProUGUI waveNumberText;                              // UI text for current wave
    [SerializeField] private GameObject waveWarningRoot;                                   // Optional warning container (GameObject)
    [SerializeField] private TextMeshProUGUI waveWarningText;                              // Warning text content
    [SerializeField] private string warningMessage = "Warning: Next Wave coming";

    [Header("Map Transition")]
    [SerializeField] private int wavesPerMapTransition = 5;                                // Show next-map portal every N cleared waves
    [SerializeField] private GameObject nextMapObject;                                     // Scene object that handles map transition
    [SerializeField] private string nextMapObjectName = "NextMap";                         // Auto-bind fallback when reference is empty

    private int currentWave = 0;
    private int aliveEnemies = 0;                                                          // Alive enemies in current wave
    private int enemiesLeftToSpawnInWave = 0;                                              // Not spawned yet in current wave
    private bool preparingNextWave = false;

    private void Start()
    {
        if (mainCamera == null)
            mainCamera = Camera.main;

        TryAutoBindUI();
        TryAutoBindNextMap();

        if (GameManager.Instance != null)
            currentWave = GameManager.Instance.GetSavedWaveProgress();

        if (waveWarningText != null)
            waveWarningText.text = warningMessage;

        SetWarningVisible(false);
        SetNextMapVisible(false);
        UpdateWaveNumberUI();

        if (enemyPrefabs == null || enemyPrefabs.Length == 0 || mainCamera == null)
            return;

        StartCoroutine(BeginNextWaveRoutine());
    }

    private void TryAutoBindUI()
    {
        // Prefab assets cannot serialize scene references. If fields are empty,
        // bind by HUD object names at runtime.
        if (waveNumberText == null)
            waveNumberText = FindSceneTMPByName("Wave_Number");

        if (waveWarningText == null)
            waveWarningText = FindSceneTMPByName("Wave_Warning");

        if (waveWarningRoot == null)
        {
            if (waveWarningText != null)
                waveWarningRoot = waveWarningText.gameObject;
            else
                waveWarningRoot = FindSceneObjectByName("Wave_Warning");
        }
    }

    private void TryAutoBindNextMap()
    {
        if (nextMapObject == null && !string.IsNullOrEmpty(nextMapObjectName))
            nextMapObject = FindSceneObjectByName(nextMapObjectName);
    }

    private static TextMeshProUGUI FindSceneTMPByName(string targetName)
    {
        TextMeshProUGUI[] allTexts = Resources.FindObjectsOfTypeAll<TextMeshProUGUI>();
        for (int i = 0; i < allTexts.Length; i++)
        {
            TextMeshProUGUI text = allTexts[i];
            if (text == null) continue;
            if (!text.gameObject.scene.IsValid()) continue;                               // Exclude prefab assets
            if (text.name == targetName) return text;
        }
        return null;
    }

    private static GameObject FindSceneObjectByName(string targetName)
    {
        GameObject[] allObjects = Resources.FindObjectsOfTypeAll<GameObject>();
        for (int i = 0; i < allObjects.Length; i++)
        {
            GameObject go = allObjects[i];
            if (go == null) continue;
            if (!go.scene.IsValid()) continue;                                            // Exclude prefab assets
            if (go.name == targetName) return go;
        }
        return null;
    }

    private IEnumerator BeginNextWaveRoutine()
    {
        if (preparingNextWave)
            yield break;

        preparingNextWave = true;
        SetNextMapVisible(false);

        // Wave 2+ show warning first, then spawn.
        if (currentWave > 0)
        {
            SetWarningVisible(true);
            yield return new WaitForSeconds(nextWaveDelay);
            SetWarningVisible(false);
        }

        currentWave++;
        if (GameManager.Instance != null)
            GameManager.Instance.SaveWaveProgress(currentWave);
        UpdateWaveNumberUI();

        int enemyCountThisWave = startingWaveEnemyCount + (currentWave - 1) * enemyCountIncreasePerWave;
        enemiesLeftToSpawnInWave = enemyCountThisWave;

        LogWave($"Wave {currentWave} start. Target enemy count: {enemyCountThisWave}");

        for (int i = 0; i < enemyCountThisWave; i++)
        {
            Vector3 spawnPos = GetRandomSpawnPosition();
            GameObject prefab = enemyPrefabs[Random.Range(0, enemyPrefabs.Length)];
            GameObject enemy = Instantiate(prefab, spawnPos, Quaternion.identity);

            aliveEnemies++;
            enemiesLeftToSpawnInWave--;

            EnemyWaveMember waveMember = enemy.GetComponent<EnemyWaveMember>();
            if (waveMember == null)
                waveMember = enemy.AddComponent<EnemyWaveMember>();
            waveMember.Init(this);

            LogWave($"Spawned enemy {i + 1}/{enemyCountThisWave}. Alive: {aliveEnemies}, LeftToSpawn: {enemiesLeftToSpawnInWave}");

            if (spawnInterval > 0f)
                yield return new WaitForSeconds(spawnInterval);
        }

        preparingNextWave = false;
        TryStartNextWave();
    }

    // Called by EnemyWaveMember when one spawned enemy gets destroyed.
    public void NotifyEnemyDestroyed()
    {
        if (aliveEnemies > 0)
            aliveEnemies--;

        LogWave($"Enemy destroyed. Alive: {aliveEnemies}, LeftToSpawn: {enemiesLeftToSpawnInWave}");

        TryStartNextWave();
    }

    private void TryStartNextWave()
    {
        if (preparingNextWave)
            return;

        bool waveFullySpawned = enemiesLeftToSpawnInWave <= 0;
        bool waveFullyCleared = aliveEnemies <= 0;

        if (waveFullySpawned && waveFullyCleared)
        {
            if (ShouldWaitForMapTransition())
            {
                SetNextMapVisible(true);
                LogWave($"Wave {currentWave} cleared. Waiting for map transition.");
                return;
            }

            LogWave($"Wave {currentWave} cleared. Next wave in {nextWaveDelay:F1}s.");
            StartCoroutine(BeginNextWaveRoutine());
        }
    }

    private void UpdateWaveNumberUI()
    {
        if (waveNumberText != null)
            waveNumberText.text = $"Wave:{currentWave}";
    }

    private void SetWarningVisible(bool visible)
    {
        if (waveWarningRoot != null)
            waveWarningRoot.SetActive(visible);

        if (waveWarningText != null)
            waveWarningText.gameObject.SetActive(visible);
    }

    private void SetNextMapVisible(bool visible)
    {
        if (nextMapObject != null)
            nextMapObject.SetActive(visible);
    }

    private bool ShouldWaitForMapTransition()
    {
        return wavesPerMapTransition > 0 && currentWave > 0 && currentWave % wavesPerMapTransition == 0;
    }

    // Get a random spawn position around player. Fallback to camera edge if player is missing.
    private Vector3 GetRandomSpawnPosition()
    {
        Transform player = Player_settings.Instance != null ? Player_settings.Instance.PlayerTransform : null;
        if (player != null)
        {
            float minRadius = Mathf.Max(0f, minSpawnDistanceFromPlayer);
            float maxRadius = Mathf.Max(minRadius, maxSpawnDistanceFromPlayer);

            float angle = Random.Range(0f, Mathf.PI * 2f);
            float radius = Random.Range(minRadius, maxRadius);
            Vector2 offset = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius;
            Vector2 spawn2D = (Vector2)player.position + offset;

            return new Vector3(spawn2D.x, spawn2D.y, 0f);
        }

        if (mainCamera == null)
            return Vector3.zero;

        float orthographicSize = mainCamera.orthographicSize;
        float aspect = mainCamera.aspect;
        float cameraWidth = orthographicSize * aspect;
        float cameraHeight = orthographicSize;

        Vector3 cameraPos = mainCamera.transform.position;

        int edge = Random.Range(0, 4);                                                    // 0=top,1=right,2=bottom,3=left
        Vector3 spawnPos = Vector3.zero;

        switch (edge)
        {
            case 0:
                spawnPos = new Vector3(
                    cameraPos.x + Random.Range(-cameraWidth - spawnDistanceFromCamera, cameraWidth + spawnDistanceFromCamera),
                    cameraPos.y + cameraHeight + spawnDistanceFromCamera,
                    0f
                );
                break;
            case 1:
                spawnPos = new Vector3(
                    cameraPos.x + cameraWidth + spawnDistanceFromCamera,
                    cameraPos.y + Random.Range(-cameraHeight - spawnDistanceFromCamera, cameraHeight + spawnDistanceFromCamera),
                    0f
                );
                break;
            case 2:
                spawnPos = new Vector3(
                    cameraPos.x + Random.Range(-cameraWidth - spawnDistanceFromCamera, cameraWidth + spawnDistanceFromCamera),
                    cameraPos.y - cameraHeight - spawnDistanceFromCamera,
                    0f
                );
                break;
            default:
                spawnPos = new Vector3(
                    cameraPos.x - cameraWidth - spawnDistanceFromCamera,
                    cameraPos.y + Random.Range(-cameraHeight - spawnDistanceFromCamera, cameraHeight + spawnDistanceFromCamera),
                    0f
                );
                break;
        }

        return spawnPos;
    }

    private void LogWave(string message)
    {
        if (!logWaveDebug)
            return;

        Debug.Log($"[EnemySpawner] {message}", this);
    }
}