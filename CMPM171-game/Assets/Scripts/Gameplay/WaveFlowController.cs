using System.Collections;
using TMPro;
using UnityEngine;

// Controls wave progression, UI updates, and map-transition gates.
// Delegates actual enemy spawning to EnemySpawner.
public class WaveFlowController : MonoBehaviour
{
    [Header("Spawner")]
    [SerializeField] private EnemySpawner enemySpawner;

    [Header("Wave Timing")]
    [SerializeField] private float nextWaveDelay = 3f;                                    // Delay before next wave starts

    [Header("UI")]
    [SerializeField] private string warningMessage = "Warning: Next Wave coming";

    [SerializeField] private AudioClip nextWaveCountDownClip;
    [Range(0f, 1f)] [SerializeField] private float nextWaveCountDownVolume = 1f;

    private AudioSource _sfxSource;
    private TextMeshProUGUI waveNumberText;
    private TextMeshProUGUI enemyLeftText;
    private GameObject waveWarningRoot;
    private TextMeshProUGUI waveWarningText;

    [Header("Map Transition")]
    [SerializeField] private int wavesPerMapTransition = 2;                                // Show next-map portal every N cleared waves
    [SerializeField] private GameObject nextMapObject;                                     // Scene object that handles map transition
    [SerializeField] private string nextMapObjectName = "NextMap";                         // Auto-bind fallback when reference is empty

    private int currentWave = 0;
    private bool transitioning = false;

    private void Start()
    {
        EnsureSFXSource();
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
        UpdateEnemyLeftUI();

        if (enemySpawner != null)
        {
            enemySpawner.OnWaveCleared += HandleWaveCleared;
            enemySpawner.OnEnemyCountChanged += UpdateEnemyLeftUI;
            StartCoroutine(BeginNextWaveRoutine());
        }
    }

    private void OnDestroy()
    {
        if (enemySpawner != null)
        {
            enemySpawner.OnWaveCleared -= HandleWaveCleared;
            enemySpawner.OnEnemyCountChanged -= UpdateEnemyLeftUI;
        }
    }

    private void HandleWaveCleared(int wave)
    {
        if (wave != currentWave)
            Debug.LogWarning($"[WaveFlowController] Wave mismatch: spawner reported wave {wave}, controller is on wave {currentWave}.", this);

        if (ShouldWaitForMapTransition())
        {
            SetNextMapVisible(true);
            return;
        }

        StartCoroutine(BeginNextWaveRoutine());
    }

    // Call this after the player finishes a map-transition portal to resume wave flow.
    public void ResumeAfterMapTransition()
    {
        SetNextMapVisible(false);
        StartCoroutine(BeginNextWaveRoutine());
    }

    // Debug: treat the current wave as completed and advance to the following wave.
    public void DebugForceCompleteCurrentWaveAndAdvance()
    {
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
            if (nextWaveCountDownClip != null && _sfxSource != null)
                _sfxSource.PlayOneShot(nextWaveCountDownClip, nextWaveCountDownVolume);
            yield return new WaitForSeconds(nextWaveDelay);
            SetWarningVisible(false);
        }

        currentWave++;
        if (GameManager.Instance != null)
            GameManager.Instance.SaveWaveProgress(currentWave);
        UpdateWaveNumberUI();

        transitioning = false;

        enemySpawner.StartWave(currentWave);
    }

    // ---- UI helpers ----

    private void UpdateWaveNumberUI()
    {
        if (waveNumberText != null)
            waveNumberText.text = $"{currentWave}";
        UpdateEnemyLeftUI();
    }

    private void UpdateEnemyLeftUI()
    {
        if (enemyLeftText == null) return;
        if (enemySpawner == null)
        {
            enemyLeftText.text = "0/0";
            return;
        }
        int remaining = enemySpawner.RemainingEnemies;
        int total = enemySpawner.TotalWaveEnemyCount;
        enemyLeftText.text = $"{remaining}/{total}";
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
            waveNumberText = SceneSearchHelper.FindSceneTMPByName("Wave_num");

        if (enemyLeftText == null)
            enemyLeftText = SceneSearchHelper.FindSceneTMPByName("Enemy_num");

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

    private void EnsureSFXSource()
    {
        if (_sfxSource != null) return;
        _sfxSource = GetComponent<AudioSource>();
        if (_sfxSource == null) _sfxSource = gameObject.AddComponent<AudioSource>();
        _sfxSource.spatialBlend = 0f;
        _sfxSource.playOnAwake = false;
    }
}
