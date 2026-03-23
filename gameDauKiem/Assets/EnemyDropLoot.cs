using UnityEngine;

public class EnemyDropLoot : MonoBehaviour
{
    [Header("Drop Settings")]
    [Tooltip("Prefab bình máu sẽ rơi ra")]
    public GameObject healthPotionPrefab;

    [Tooltip("Tỉ lệ % rơi bình máu (0-100)")]
    [Range(0f, 100f)]
    public float dropChance = 30f;

    [Tooltip("Lực bật bình máu ra")]
    public float dropForce = 5f;

    void Start()
    {
        Debug.Log($"🎲 EnemyDropLoot START on {gameObject.name}");

        if (healthPotionPrefab == null)
        {
            Debug.LogError($"❌❌❌ [{gameObject.name}] healthPotionPrefab = NULL! Chưa gán Prefab!");
        }
        else
        {
            Debug.Log($"✅ [{gameObject.name}] healthPotionPrefab = {healthPotionPrefab.name}");
        }

        Debug.Log($"📊 [{gameObject.name}] Drop Chance: {dropChance}%, Drop Force: {dropForce}");
    }

    public void DropLoot()
    {
        Debug.Log($"🎯🎯🎯 DropLoot() CALLED on {gameObject.name}!");

        if (healthPotionPrefab == null)
        {
            Debug.LogError($"❌❌❌ [{gameObject.name}] KHÔNG THỂ DROP! healthPotionPrefab = NULL!");
            return;
        }

        float randomValue = Random.Range(0f, 100f);
        Debug.Log($"🎲 Random: {randomValue:F2}% vs Drop Chance: {dropChance}%");

        if (randomValue <= dropChance)
        {
            Debug.Log($"✅ PASSED! Spawning health potion...");
            SpawnHealthPotion();
        }
        else
        {
            Debug.Log($"❌ FAILED! {randomValue:F2}% > {dropChance}% - Không rơi item");
        }
    }

    void SpawnHealthPotion()
    {
        Debug.Log($"💊💊💊 SpawnHealthPotion() START!");

        if (healthPotionPrefab == null)
        {
            Debug.LogError($"❌ Prefab NULL trong SpawnHealthPotion!");
            return;
        }

        Vector3 spawnPos = transform.position;
        Debug.Log($"📍 Spawn Position: {spawnPos}");

        GameObject droppedPotion = Instantiate(healthPotionPrefab, spawnPos, Quaternion.identity);

        if (droppedPotion == null)
        {
            Debug.LogError($"❌❌❌ Instantiate FAILED! GameObject = NULL!");
            return;
        }

        Debug.Log($"✅✅✅ Instantiated: {droppedPotion.name} at {spawnPos}");

        Rigidbody2D potionRb = droppedPotion.GetComponent<Rigidbody2D>();
        if (potionRb == null)
        {
            Debug.LogError($"❌ Health Potion KHÔNG CÓ Rigidbody2D! Sẽ không rơi!");
        }
        else
        {
            Debug.Log($"✅ Found Rigidbody2D on potion");

            Vector2 randomDirection = new Vector2(Random.Range(-1f, 1f), 1f);
            Vector2 force = randomDirection * dropForce;
            potionRb.AddForce(force, ForceMode2D.Impulse);

            Debug.Log($"💨 Added Force: {force}, Velocity: {potionRb.linearVelocity}");
        }

        // Không cần check HealthPickup - sẽ tự có
        Collider2D col = droppedPotion.GetComponent<Collider2D>();
        if (col == null)
        {
            Debug.LogWarning($"⚠️ Potion không có Collider!");
        }
        else
        {
            Debug.Log($"✅ Collider found: {col.GetType().Name}, IsTrigger: {col.isTrigger}");
        }

        Debug.Log($"🎉🎉🎉 SPAWN COMPLETE!");
    }
}