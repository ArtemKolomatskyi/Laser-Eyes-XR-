#region PlayerHealth
using UnityEngine;
using TMPro;

public class PlayerHealth : MonoBehaviour
{
    #region Fields
    [Header("Player Health Settings")]
    public int maxHealth = 100; // The maximum health of the player
    public int currentHealth; // The current health of the player
    public TMP_Text healthText; // Reference to the TMP Text element for displaying health
    #endregion

    #region Unity Methods
    private void Start()
    {
        currentHealth = maxHealth;
        UpdateHealthText();
    }

    private void Update()
    {
        // Example: Reduce health for testing purposes
        if (Input.GetKeyDown(KeyCode.H))
        {
            TakeDamage(10);
        }
    }
    #endregion

    #region Public Methods
    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        if (currentHealth < 0)
        {
            currentHealth = 0;
        }
        UpdateHealthText();
    }

    public void Heal(int amount)
    {
        currentHealth += amount;
        if (currentHealth > maxHealth)
        {
            currentHealth = maxHealth;
        }
        UpdateHealthText();
    }
    #endregion

    #region Private Methods
    private void UpdateHealthText()
    {
        healthText.text = "Health: " + currentHealth.ToString();
    }
    #endregion
}
#endregion
