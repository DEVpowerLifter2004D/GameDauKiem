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
        TryHandlePickup(other);
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        // Fallback in case pickup collider is not trigger.
        TryHandlePickup(collision.collider);
    }

    private void TryHandlePickup(Collider2D other)
    {
        if (other == null) return;

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

        if (healed || pickup.consumeWhenFull)
        {
            if (pickup.destroyOnUse)
            {
                Destroy(other.gameObject);
            }
        }
    }
}
