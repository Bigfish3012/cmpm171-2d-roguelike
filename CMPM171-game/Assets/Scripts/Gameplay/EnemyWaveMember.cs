using UnityEngine;

// Runtime helper added to spawned enemies so spawner can track wave clear state.
public class EnemyWaveMember : MonoBehaviour
{
    private EnemySpawner ownerSpawner;
    private bool hasNotified = false;

    public void Init(EnemySpawner spawner)
    {
        ownerSpawner = spawner;
        hasNotified = false;
    }

    private void OnDestroy()
    {
        if (hasNotified)
            return;

        hasNotified = true;

        if (ownerSpawner != null)
            ownerSpawner.NotifyEnemyDestroyed();
    }
}
