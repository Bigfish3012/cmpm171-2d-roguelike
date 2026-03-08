using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SettingAudioController : MonoBehaviour
{
    [Header("UI")]
    public TextMeshProUGUI percentText;
    public GameObject mutedBadge;
    public Image barFill;

    [Header("Mute Labels (Localization)")]
    public GameObject labelMute;
    public GameObject labelUnmute;

    [Header("Buttons")]
    public Button btnMinus;
    public Button btnPlus;
    public Button btnMute;

    [Header("Step")]
    public int stepPercent = 5;

    private float lastNonZero = 0.1f;

    void Start()
    {
        if (btnMinus) btnMinus.onClick.AddListener(Decrease);
        if (btnPlus)  btnPlus.onClick.AddListener(Increase);
        if (btnMute)  btnMute.onClick.AddListener(ToggleMute);

        float v = GetVol();
        if (v > 0.0001f) lastNonZero = v;

        RefreshUI();
    }

    void Decrease()
    {
        float v = Mathf.Clamp01(GetVol() - stepPercent / 100f);
        SetVol(v);
        if (v > 0.0001f) lastNonZero = v;
        RefreshUI();
    }

    void Increase()
    {
        float v = Mathf.Clamp01(GetVol() + stepPercent / 100f);
        SetVol(v);
        if (v > 0.0001f) lastNonZero = v;
        RefreshUI();
    }

    void ToggleMute()
    {
        if (BGMManager.Instance == null) return;

        bool muted = BGMManager.Instance.IsMuted();

        if (!muted)
        {
            float v = GetVol();
            if (v > 0.0001f) lastNonZero = v;

            BGMManager.Instance.SetMute(true);
            BGMManager.Instance.SetMasterVolume01(0f);
        }
        else
        {
            BGMManager.Instance.SetMute(false);
            float restore = Mathf.Clamp01(lastNonZero <= 0.0001f ? 0.1f : lastNonZero);
            BGMManager.Instance.SetMasterVolume01(restore);
        }

        RefreshUI();
    }

    float GetVol()
    {
        if (BGMManager.Instance == null) return 0.1f;
        return BGMManager.Instance.GetMasterVolume01();
    }

    void SetVol(float v01)
    {
        if (BGMManager.Instance == null) return;

        if (BGMManager.Instance.IsMuted() && v01 > 0.0001f)
            BGMManager.Instance.SetMute(false);

        BGMManager.Instance.SetMasterVolume01(v01);
    }

    void RefreshUI()
    {
        float v = GetVol();
        int pct = Mathf.RoundToInt(v * 100f);

        if (percentText) percentText.text = pct + "%";

        bool muted = (pct == 0) || (BGMManager.Instance != null && BGMManager.Instance.IsMuted());
        if (mutedBadge) mutedBadge.SetActive(muted);

        if (labelMute) labelMute.SetActive(!muted);
        if (labelUnmute) labelUnmute.SetActive(muted);

        if (barFill) barFill.fillAmount = v;
    }
}