using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BossFireHP : MonoBehaviour
{
    [Header("References")]
    public Image healthBarFill;
    public Image healthBarBorder;
    public TextMeshProUGUI bossNameText;
    public BossFire bossFire;

    [Header("Settings")]
    public string bossName = "BOSS";
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

        if (bossFire != null)
            bossTransform = bossFire.transform;
        else
            Debug.LogError("❌ BossFireHP: Biến 'bossFire' chưa được gán trong Inspector! Kéo thả Boss vào đây.");

        if (healthBarFill == null)
        {
            Transform fillTransform = transform.Find("HealthBarFill");
            if (fillTransform != null) healthBarFill = fillTransform.GetComponent<Image>();
            if (healthBarFill == null)
            {
                Image[] images = GetComponentsInChildren<Image>();
                foreach (var img in images) { if (img.name == "HealthBarFill") healthBarFill = img; }
            }
        }

        if (bossNameText == null)
        {
            Transform textTransform = transform.Find("BossNameText");
            if (textTransform != null) bossNameText = textTransform.GetComponent<TextMeshProUGUI>();
            if (bossNameText == null)
            {
                TextMeshProUGUI[] texts = GetComponentsInChildren<TextMeshProUGUI>();
                foreach (var txt in texts) { if (txt.name == "BossNameText") bossNameText = txt; }
            }
        }
        
        if (healthBarFill == null) Debug.LogError("❌ BossFireHP: Không tìm thấy Component Image có tên 'HealthBarFill'. Thanh máu sẽ không hoạt động!");
        if (bossNameText == null) Debug.LogWarning("⚠️ BossFireHP: Không tìm thấy Text 'BossNameText'.");

        if (healthBarFill != null)
        {
            healthBarFill.type = Image.Type.Filled;
            healthBarFill.fillMethod = Image.FillMethod.Horizontal;
            healthBarFill.fillOrigin = (int)Image.OriginHorizontal.Left;
            healthBarFill.fillAmount = 1f;
        }

        if (bossNameText != null)
            bossNameText.text = bossName;

        UpdateHealthBar();
    }

    void LateUpdate()
    {
        if (bossTransform != null)
            transform.position = bossTransform.position + offset;

        UpdateHealthBar();
        FaceCamera();
    }

    void UpdateHealthBar()
    {
        if (bossFire == null || healthBarFill == null) return;

        int currentHealth = bossFire.GetCurrentHealth();
        int maxHealth = bossFire.GetMaxHealth();

        if (maxHealth <= 0) return;

        float targetFill = Mathf.Clamp01((float)currentHealth / maxHealth);

        if (smoothTransition)
            healthBarFill.fillAmount = Mathf.Lerp(healthBarFill.fillAmount, targetFill, Time.deltaTime * smoothSpeed);
        else
            healthBarFill.fillAmount = targetFill;

        UpdateColor(healthBarFill.fillAmount);
    }

    void UpdateColor(float healthPercent)
    {
        if (healthBarFill == null) return;

        if (healthPercent > 0.6f)
            healthBarFill.color = fullHealthColor;
        else if (healthPercent > 0.3f)
            healthBarFill.color = midHealthColor;
        else
            healthBarFill.color = lowHealthColor;
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
}