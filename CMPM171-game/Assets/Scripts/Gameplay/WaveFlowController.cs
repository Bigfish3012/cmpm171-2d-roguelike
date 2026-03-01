using System.Collections;
using TMPro;
using UnityEngine;

/// <summary>
/// Controls wave progression, UI updates, and map-transition gates.
/// Delegates actual enemy spawning to <see cref="EnemySpawner"/>.
/// </summary>
public class WaveFlowController : MonoBehaviour
{
    [Header("Spawner")]
    [SerializeField] private EnemySpawner enemySpawner;

    [Header("Wave Timing")]
    [SerializeField] private float nextWaveDelay = 2f;                                    // Delay before next wave starts

    [Header("UI")]
    [SerializeField] private string warningMessage = "Warning: Next Wave coming";

    private TextMeshProUGUI waveNumberText;
    private GameObject waveWarningRoot;
    private TextMeshProUGUI waveWarningText;

    [Header("Map Transition")]
    [SerializeField] private int wavesPerMapTransition = 5;                                // Show next-map portal every N cleared waves
    [SerializeField] private GameObject nextMapObject;                                     // Scene object that handles map transition
    [SerializeField] private string nextMapObjectName = "NextMap";                         // Auto-bind fallback when reference is empty

    [Header("Debug")]
    [SerializeField] private bool logDebug = true;

    private int currentWave = 0;
    private bool transitioning = false;

    private void Start()
    {
        TryAutoBindSpawner();
        TryAutoBindUI();
        TryAutoBindNextMap();

        if (GameManager.Instance != null)
            currentWave = GameManager.Instance.GetSavedWaveProgress();

        if (waveWarningText != null)
            waveWarningText.text = warningMessage;

        SetWarningVisible(false);
        SetNextMapVisible(false);
        UpdateWaveNumberUI();

        if (enemySpawner != null)
        {
            enemySpawner.OnWaveCleared += HandleWaveCleared;
            StartCoroutine(BeginNextWaveRoutine());
        }
    }

    private void OnDestroy()
    {
        if (enemySpawner != null)
            enemySpawner.OnWaveCleared -= HandleWaveCleared;
    }

    private void HandleWaveCleared(int wave)
    {
        if (wave != currentWave)
            Debug.LogWarning($"[WaveFlowController] Wave mismatch: spawner reported wave {wave}, controller is on wave {currentWave}.", this);

        if (ShouldWaitForMapTransition())
        {
            SetNextMapVisible(true);
            Log($"Wave {currentWave} cleared. Waiting for map transition.");
            return;
        }

        Log($"Wave {currentWave} cleared. Next wave in {nextWaveDelay:F1}s.");
        StartCoroutine(BeginNextWaveRoutine());
    }

    /// <summary>
    /// Call this after the player finishes a map-transition portal to resume wave flow.
    /// </summary>
    public void ResumeAfterMapTransition()
    {
        SetNextMapVisible(false);
        StartCoroutine(BeginNextWaveRoutine());
    }

    private IEnumerator BeginNextWaveRoutine()
    {
        if (transitioning)
            yield break;

        transitioning = true;
        SetNextMapVisible(false);

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

        Log($"Starting wave {currentWave}.");
        transitioning = false;

        enemySpawner.StartWave(currentWave);
    }

    // ---- UI helpers ----

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

    // ---- Auto-bind ----

    private void TryAutoBindSpawner()
    {
        if (enemySpawner == null)
            enemySpawner = GetComponent<EnemySpawner>();
    }

    private void TryAutoBindUI()
    {
        if (waveNumberText == null)
            waveNumberText = SceneSearchHelper.FindSceneTMPByName("Wave_Number");

        if (waveWarningText == null)
            waveWarningText = SceneSearchHelper.FindSceneTMPByName("Wave_Warning");

        if (waveWarningRoot == null)
        {
            if (waveWarningText != null)
                waveWarningRoot = waveWarningText.gameObject;
            else
                waveWarningRoot = SceneSearchHelper.FindSceneObjectByName("Wave_Warning");
        }
    }

    private void TryAutoBindNextMap()
    {
        if (nextMapObject == null && !string.IsNullOrEmpty(nextMapObjectName))
            nextMapObject = SceneSearchHelper.FindSceneObjectByName(nextMapObjectName);
    }

    private void Log(string message)
    {
        if (!logDebug) return;
        Debug.Log($"[WaveFlowController] {message}", this);
    }
}
