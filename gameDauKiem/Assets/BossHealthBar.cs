using UnityEngine;
using UnityEngine.UI;

public class BossHealthBar : MonoBehaviour
{
    [Header("References")]
    public Slider healthSlider;
    public Image fillImage;
    public BossController bossController;

    [Header("Colors")]
    public Color fullHealthColor = Color.red;
    public Color lowHealthColor = Color.yellow;

    private Camera mainCamera;

    void Start()
    {
        mainCamera = Camera.main;

        if (bossController == null)
        {
            bossController = GetComponentInParent<BossController>();
        }

        if (healthSlider == null)
        {
            healthSlider = GetComponentInChildren<Slider>();
        }

        if (fillImage == null && healthSlider != null)
        {
            fillImage = healthSlider.fillRect.GetComponent<Image>();
        }

        UpdateHealthBar();
    }

    void Update()
    {
        UpdateHealthBar();

        // Luôn quay về camera
        if (mainCamera != null)
        {
            transform.LookAt(transform.position + mainCamera.transform.rotation * Vector3.forward,
                            mainCamera.transform.rotation * Vector3.up);
        }
    }

    void UpdateHealthBar()
    {
        if (bossController == null || healthSlider == null) return;

        float currentHealth = bossController.GetCurrentHealth();
        float maxHealth = bossController.GetMaxHealth();

        healthSlider.maxValue = maxHealth;
        healthSlider.value = currentHealth;

        // Đổi màu theo HP
        if (fillImage != null)
        {
            float healthPercent = currentHealth / maxHealth;
            fillImage.color = Color.Lerp(lowHealthColor, fullHealthColor, healthPercent);
        }
    }
}