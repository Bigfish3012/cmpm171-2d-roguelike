using UnityEngine;

// XP and level on player. Subscribe to enemy deaths: Enemy1 +1 XP, Enemy2 +2 XP. 10 XP = 1 level, +1 skill point.
public class PlayerExperience : MonoBehaviour
{
    [Header("Level")]
    [SerializeField] private int xpToNextLevel = 10;

    private int currentXP;
    private int level = 1;
    private int skillPoints;

    public int CurrentXP => currentXP;
    public int Level => level;
    public int SkillPoints => skillPoints;
    public int XpToNextLevel => xpToNextLevel;
    public float XpProgress => xpToNextLevel > 0 ? (float)currentXP / xpToNextLevel : 0f;

    void OnEnable()
    {
        EnemyHealth.OnEnemyDied += OnEnemyDied;
    }

    void OnDisable()
    {
        EnemyHealth.OnEnemyDied -= OnEnemyDied;
    }

    void OnEnemyDied(EnemyHealth.EnemyType type)
    {
        int xp = type == EnemyHealth.EnemyType.Enemy1 ? 1 : 2;
        currentXP += xp;

        while (currentXP >= xpToNextLevel)
        {
            currentXP -= xpToNextLevel;
            level++;
            skillPoints++;
        }
    }
}
