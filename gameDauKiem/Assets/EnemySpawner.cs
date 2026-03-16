using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    public GameObject enemyPrefab;
    public float spawnInterval = 1.5f;
    public int maxEnemies = 10;

    [Header("Spawn Points")]
    public Transform[] spawnPoints;

    private float timer = 0f;
    private int currentAliveCount = 0;

    void Update()
    {

        if (currentAliveCount >= maxEnemies) return;

        timer += Time.deltaTime;
        if (timer >= spawnInterval)
        {
            timer = 0f;
            SpawnEnemy();
        }
    }

    void SpawnEnemy()
    {
        if (spawnPoints.Length == 0 || enemyPrefab == null) return;

        Transform point = spawnPoints[Random.Range(0, spawnPoints.Length)];
        GameObject newEnemy = Instantiate(enemyPrefab, point.position, Quaternion.identity);

        // ✅ Enemy sẽ tự gọi OnEnemySpawned() trong Start()
        // Nên KHÔNG cần currentAliveCount++ ở đây nữa

    }

    // ✅ HÀM MỚI: Được gọi từ Enemy.Start()
    public void OnEnemySpawned()
    {
        currentAliveCount++;
    }

    // ✅ Được gọi từ Enemy.Die()
    public void OnEnemyDied()
    {
        currentAliveCount--;
    }
}