using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BossHealthBarUI : MonoBehaviour
{
    [Header("References")]
    public Image healthBarFill;
    public Image healthBarBorder;
    public TextMeshProUGUI bossNameText;
    public BossController bossController;

    [Header("Settings")]
    public string bossName = "MINOTAUR";
    public bool smoothTransition = true;
    public float smoothSpeed = 5f;
    public Vector3 offset = new Vector3(0, 2f, 0);

    [Header("Colors")]
    public Color fullHealthColor = new Color(0.8f, 0f, 0f);
    public Color midHealthColor = Color.yellow;
    public Color lowHealthColor = new Color(1f, 0.3f, 0f);

    private Camera mainCamera;
    private Transform bossTransform;

    void Start()
    {
        mainCamera = Camera.main;

        // Find Boss Transform
        if (bossController != null)
        {
            bossTransform = bossController.transform;
        }
        else
        {
            bossController = GetComponentInParent<BossController>();
            if (bossController != null)
                bossTransform = bossController.transform;
        }

        // Auto-find UI elements
        if (healthBarFill == null)
        {
            Transform fillTransform = transform.Find("HealthBarFill");
            if (fillTransform != null)
                healthBarFill = fillTransform.GetComponent<Image>();
        }

        if (bossNameText == null)
        {
            Transform textTransform = transform.Find("BossNameText");
            if (textTransform != null)
                bossNameText = textTransform.GetComponent<TextMeshProUGUI>();
        }

        // Setup health bar image
        if (healthBarFill != null)
        {
            healthBarFill.type = Image.Type.Filled;
            healthBarFill.fillMethod = Image.FillMethod.Horizontal;
            healthBarFill.fillOrigin = (int)Image.OriginHorizontal.Left;
            healthBarFill.fillAmount = 1f;
        }

        // Set boss name
        if (bossNameText != null)
        {
            bossNameText.text = bossName;
        }

        UpdateHealthBar();
    }

    void LateUpdate()
    {
        // Follow boss position
        if (bossTransform != null)
        {
            transform.position = bossTransform.position + offset;
        }

        UpdateHealthBar();
        FaceCamera();
    }

    void UpdateHealthBar()
    {
        if (bossController == null || healthBarFill == null) return;

        int currentHealth = bossController.GetCurrentHealth();
        int maxHealth = bossController.GetMaxHealth();

        if (maxHealth <= 0) return;

        float targetFill = Mathf.Clamp01((float)currentHealth / maxHealth);

        // Smooth transition
        if (smoothTransition)
        {
            healthBarFill.fillAmount = Mathf.Lerp(
                healthBarFill.fillAmount,
                targetFill,
                Time.deltaTime * smoothSpeed
            );
        }
        else
        {
            healthBarFill.fillAmount = targetFill;
        }

        // Update color
        UpdateColor(healthBarFill.fillAmount);
    }

    void UpdateColor(float healthPercent)
    {
        if (healthBarFill == null) return;

        if (healthPercent > 0.6f)
        {
            healthBarFill.color = fullHealthColor;
        }
        else if (healthPercent > 0.3f)
        {
            healthBarFill.color = midHealthColor;
        }
        else
        {
            healthBarFill.color = lowHealthColor;
        }
    }

    void FaceCamera()
    {
        if (mainCamera != null)
        {
            transform.LookAt(
                transform.position + mainCamera.transform.rotation * Vector3.forward,
                mainCamera.transform.rotation * Vector3.up
            );
        }
    }

    public void SetBossName(string name)
    {
        bossName = name;
        if (bossNameText != null)
        {
            bossNameText.text = name;
        }
    }
}