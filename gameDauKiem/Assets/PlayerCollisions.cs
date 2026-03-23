using UnityEngine;

public class PlayerCollisions : MonoBehaviour
{
    private PlayerController playerController;
    private GameManager gameManager;

    void Awake()
    {
        playerController = GetComponent<PlayerController>();
        gameManager = FindFirstObjectByType<GameManager>();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Key"))
        {
            if (gameManager == null)
            {
                gameManager = FindFirstObjectByType<GameManager>();
            }

            gameManager?.PlayerCollectedKey();
            Destroy(other.gameObject);
            return;
        }

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
