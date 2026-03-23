using UnityEngine;
using UnityEngine.UI;

public class BossFireHPBar : MonoBehaviour
{
    public BossFire boss;
    public Image fill;

    public Vector3 offset = new Vector3(0, 2f, 0);

    void Update()
    {
        if (boss == null)
        {
            Debug.LogError("❌ boss chưa được gán!");
            return;
        }
        if (fill == null)
        {
            Debug.LogError("❌ fill chưa được gán!");
            return;
        }

        transform.position = boss.transform.position + offset;

        float percent = (float)boss.GetCurrentHealth() / boss.GetMaxHealth();
        Debug.Log($"HP Bar: {percent}"); // ← thêm dòng này
        fill.fillAmount = percent;
    }
}