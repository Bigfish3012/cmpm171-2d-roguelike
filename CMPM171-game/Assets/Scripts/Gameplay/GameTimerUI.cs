using UnityEngine;
using TMPro;

// Displays elapsed time from EnemySpawner. Assign spawner or leave null to auto-find.
public class GameTimerUI : MonoBehaviour
{
    [Header("Source")]
    [SerializeField] private EnemySpawner spawner;

    [Header("Optional UI")]
    [SerializeField] private TMP_Text timeText;

    void Start()
    {
        if (spawner == null)
            spawner = FindFirstObjectByType<EnemySpawner>();
        if (timeText == null)
            timeText = GetComponentInChildren<TMP_Text>();
    }

    void Update()
    {
        if (spawner == null || timeText == null) return;
        timeText.text = spawner.GetTimeString();
    }
}
