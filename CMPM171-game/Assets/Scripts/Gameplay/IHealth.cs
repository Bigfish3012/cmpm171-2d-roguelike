/// <summary>
/// Interface for objects that have health (enemies, players, etc.)
/// </summary>
public interface IHealth
{
    int GetCurrentHealth();
    int GetMaxHealth();
}
