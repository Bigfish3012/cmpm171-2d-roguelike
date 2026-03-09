// Interface for objects that can take damage
public interface IDamageable
{
    void TakeDamage(int damage, bool isCrit = false);
}
