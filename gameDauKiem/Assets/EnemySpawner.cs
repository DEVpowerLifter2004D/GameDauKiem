using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    public GameObject enemyPrefab;
    public float spawnInterval = 1.5f;
    public int maxEnemies = 10;

    [Header("Spawn Points - kéo các vị trí spawn vào đây")]
    public Transform[] spawnPoints;

    private float timer = 0f;

    void Update()
    {
        // Không spawn nếu đã đủ enemy
        EnemyController[] current = FindObjectsOfType<EnemyController>();
        if (current.Length >= maxEnemies) return;

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

        // Chọn ngẫu nhiên 1 điểm spawn
        Transform point = spawnPoints[Random.Range(0, spawnPoints.Length)];
        Instantiate(enemyPrefab, point.position, Quaternion.identity);
        Debug.Log("👾 Spawned enemy!");
    }
}