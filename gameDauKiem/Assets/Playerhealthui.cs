using UnityEngine;
using UnityEngine.UI;

public class PlayerHealthUI : MonoBehaviour
{
    [Header("References")]
    public Image healthBarFill;  // Thanh đỏ
    public PlayerController playerController;  // Player

    void Start()
    {
        // Tự động tìm Player
        if (playerController == null)
        {
            playerController = FindFirstObjectByType<PlayerController>();
        }

        UpdateHealthBar();
    }

    void Update()
    {
        UpdateHealthBar();
    }

    void UpdateHealthBar()
    {
        if (playerController == null || healthBarFill == null) return;

        // Tính % máu
        float healthPercent = (float)playerController.GetCurrentHealth() / playerController.maxHealth;

        // Update fill amount
        healthBarFill.fillAmount = healthPercent;
    }
}