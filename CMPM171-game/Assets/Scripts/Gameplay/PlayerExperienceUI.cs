using UnityEngine;
using UnityEngine.UI;

// Assign to a UI object. Optionally assign Slider for XP bar and Text for level/skill points.
public class PlayerExperienceUI : MonoBehaviour
{
    [Header("Source")]
    [SerializeField] private PlayerExperience source;

    [Header("Optional UI")]
    [SerializeField] private Slider xpSlider;
    [SerializeField] private Text levelText;

    void Start()
    {
        if (source == null)
            source = FindFirstObjectByType<PlayerExperience>();
    }

    void Update()
    {
        if (source == null) return;

        if (xpSlider != null)
            xpSlider.value = source.XpProgress;

        if (levelText != null)
            levelText.text = "Lv." + source.Level + " SP:" + source.SkillPoints;
    }
}
