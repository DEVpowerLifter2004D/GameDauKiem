using UnityEngine;

public class PlayerCollisions : MonoBehaviour
{
    private PlayerController playerController;

    void Awake()
    {
        playerController = GetComponent<PlayerController>();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        HealthPickup pickup = other.GetComponent<HealthPickup>();
        if (pickup == null) return;

        bool healed = false;
        if (playerController != null)
        {
            healed = playerController.Heal(pickup.healAmount);
        }

        // Nếu được hồi hoặc cho phép ăn khi đầy máu → hủy item
        if (healed || pickup.consumeWhenFull)
        {
            if (pickup.destroyOnUse)
            {
                Destroy(other.gameObject);
            }
        }
    }
}
