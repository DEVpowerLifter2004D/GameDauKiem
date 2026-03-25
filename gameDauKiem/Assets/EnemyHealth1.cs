using UnityEngine;
using UnityEngine.UI;

public class EnemyHealth1 : MonoBehaviour
{
    public float maxHealth = 100f;
    private float currentHealth;

    public Image healthFill;   // Kéo HealthFill vào đây

    void Start()
    {
        currentHealth = maxHealth;
        UpdateHealthBar();
    }
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))   // Nhấn phím Space để test
        {
            TakeDamage(20);
            Debug.Log("Đã trừ 20 máu! Current health = " + currentHealth);
        }
    }
    public void TakeDamage(float damage)
    {
        currentHealth -= damage;
        UpdateHealthBar();

        if (currentHealth <= 0)
            Destroy(gameObject);
    }

    void UpdateHealthBar()
    {
        if (healthFill != null)
        {
            float newFill = currentHealth / maxHealth;
            healthFill.fillAmount = newFill;
            Debug.Log($"Set fillAmount = {newFill} | Current health = {currentHealth}");
        }
        else
        {
            Debug.LogError("healthFill is NULL!");
        }
    }
}