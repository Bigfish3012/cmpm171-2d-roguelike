using UnityEngine;
using UnityEngine.SceneManagement;

public class Player_settings : MonoBehaviour, IDamageable
{
    [SerializeField] private int maxHealth = 10;
    
    private int currentHealth;
    private int lastPrintedHealth;

    void Start()
    {
        currentHealth = maxHealth;
        lastPrintedHealth = currentHealth;
        Debug.Log($"Player Health: {currentHealth}");
    }

    void Update()
    {
        // Print health to console when it changes
        if (currentHealth != lastPrintedHealth)
        {
            Debug.Log($"Player Health: {currentHealth}");
            lastPrintedHealth = currentHealth;
        }
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        
        if (currentHealth <= 0)
        {
            currentHealth = 0;
            // Handle player death here by reloading the scene
            Debug.Log("Player Died!");
            SceneManager.LoadScene("Gameover");
        }
    }

    public int GetCurrentHealth()
    {
        return currentHealth;
    }
}
