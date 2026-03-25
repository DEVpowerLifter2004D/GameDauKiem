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

        Debug.Log($"🔍 Player touched: {other.name} | Tag: {other.tag}"); // ✅ THÊM DÒNG NÀY

        if (other.CompareTag("Key"))
        {
            Debug.Log("🔑 KEY DETECTED!"); // ✅ THÊM DÒNG NÀY

            if (gameManager == null)
            {
                gameManager = FindFirstObjectByType<GameManager>();
                Debug.Log($"GameManager found: {gameManager != null}"); // ✅ THÊM DÒNG NÀY
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
