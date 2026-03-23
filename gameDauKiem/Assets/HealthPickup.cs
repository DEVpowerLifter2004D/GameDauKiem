using UnityEngine;

public class HealthPickup : MonoBehaviour
{
    [Header("Health Pickup")]
    public int healAmount = 1;

    [Tooltip("Cho phép ăn khi đang đầy máu")]
    public bool consumeWhenFull = true;

    [Tooltip("Hủy item sau khi ăn")]
    public bool destroyOnUse = true;

    private Rigidbody2D rb;
    private Collider2D col;
    private bool hasLanded = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();

        // ✅ TẮT TRIGGER LÚC MỚI SPAWN (để rơi xuống)
        if (col != null)
        {
            col.isTrigger = false;
        }
    }

    void Update()
    {
        // ✅ Khi chạm đất (velocity gần 0) → BẬT TRIGGER
        if (!hasLanded && rb != null)
        {
            if (Mathf.Abs(rb.linearVelocity.y) < 0.1f && Mathf.Abs(rb.linearVelocity.x) < 0.1f)
            {
                hasLanded = true;

                if (col != null)
                {
                    col.isTrigger = true; // Bật trigger để Player nhặt được
                }

                // Freeze để không bị lăn lung tung
                if (rb != null)
                {
                    rb.constraints = RigidbodyConstraints2D.FreezeAll;
                }

                Debug.Log("💊 Bình máu đã hạ cánh!");
            }
        }
    }

    private void Pickup(GameObject obj)
    {
        PlayerController player = obj.GetComponentInParent<PlayerController>();

        if (player != null)
        {
            bool healed = player.Heal(healAmount);
            Debug.Log($"💊 Player nhặt bình máu! Hồi được: {healed}");

            if (healed || consumeWhenFull)
            {
                if (destroyOnUse)
                {
                    Debug.Log("🗑️ Bình máu biến mất!");
                    Destroy(gameObject);
                }
            }
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Pickup(other.gameObject);
        }
    }

    void OnTriggerStay2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Pickup(other.gameObject);
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            Pickup(collision.gameObject);
        }
    }
}