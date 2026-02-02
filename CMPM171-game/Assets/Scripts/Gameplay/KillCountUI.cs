using UnityEngine;
using TMPro;

// Shows kill count from KillCounter. Assign counter or leave null to auto-find.
public class KillCountUI : MonoBehaviour
{
    [Header("Source")]
    [SerializeField] private KillCounter counter;

    [Header("Optional UI")]
    [SerializeField] private TMP_Text killText;

    [Header("Format")]
    [SerializeField] private string format = "Kills: {0}";

    void Start()
    {
        if (counter == null)
            counter = FindFirstObjectByType<KillCounter>();
        if (killText == null)
            killText = GetComponentInChildren<TMP_Text>();
    }

    void Update()
    {
        if (counter == null || killText == null) return;
        killText.text = string.Format(format, counter.TotalKills);
    }
}
