using UnityEngine;

public class HealthPickup : MonoBehaviour
{
    [Header("Health Pickup")]
    public int healAmount = 1;

    [Tooltip("Allow pickup when player is already full HP")]
    public bool consumeWhenFull = true;

    [Tooltip("Destroy potion after pickup")]
    public bool destroyOnUse = true;

    void Reset()
    {
        EnsureTriggerCollider();
    }

    void OnValidate()
    {
        EnsureTriggerCollider();
    }

    private void EnsureTriggerCollider()
    {
        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
        {
            col.isTrigger = true;
        }
    }
}
