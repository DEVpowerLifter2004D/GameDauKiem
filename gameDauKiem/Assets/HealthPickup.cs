using UnityEngine;

public class HealthPickup : MonoBehaviour
{
    [Header("Health Pickup")]
    public int healAmount = 1;

    [Tooltip("Cho phép ăn khi đang đầy máu (sẽ không tăng thêm)")]
    public bool consumeWhenFull = true;

    [Tooltip("Hủy item sau khi ăn")]
    public bool destroyOnUse = true;
}
