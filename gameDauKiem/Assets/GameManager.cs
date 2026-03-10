using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class GameManager : MonoBehaviour
{
    [Header("Enemy Spawning")]
    public GameObject enemyPrefab;
    public Transform[] spawnPoints;
    public float spawnCooldown = 2f;  // Cooldown giữa mỗi lần spawn

    [Header("Game Rules")]
    public float survivalTime = 30f;  // Thời gian cần sống sót (30s hoặc 300s cho 5 phút)

    [Header("UI")]
    public TextMeshProUGUI timerText;
    public GameObject winPanel;
    public GameObject losePanel;

    private float currentTime;
    private float nextSpawnTime;
    private bool gameOver = false;

    void Start()
    {
        currentTime = 0f;
        nextSpawnTime = Time.time + spawnCooldown;

        if (winPanel) winPanel.SetActive(false);
        if (losePanel) losePanel.SetActive(false);
    }

    void Update()
    {
        if (gameOver) return;

        // ✅ ĐẾM THỜI GIAN
        currentTime += Time.deltaTime;
        UpdateTimerUI();

        // ✅ CHECK WIN: Hết thời gian → Thắng!
        if (currentTime >= survivalTime)
        {
            Win();
            return;  // Dừng spawn
        }

        // ✅ SPAWN VÔ HẠN: Chỉ spawn khi chưa hết giờ
        if (Time.time >= nextSpawnTime)
        {
            SpawnEnemy();
            nextSpawnTime = Time.time + spawnCooldown;
        }
    }

    void UpdateTimerUI()
    {
        if (timerText == null) return;

        float timeLeft = survivalTime - currentTime;

        // Đếm ngược
        int minutes = Mathf.FloorToInt(timeLeft / 60f);
        int seconds = Mathf.FloorToInt(timeLeft % 60);

        timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
    }

    void SpawnEnemy()
    {
        if (spawnPoints.Length == 0)
        {
            Debug.LogError("❌ No spawn points!");
            return;
        }

        // Chọn spawn point ngẫu nhiên
        Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];

        // Spawn enemy VÔ HẠN
        GameObject enemy = Instantiate(enemyPrefab, spawnPoint.position, Quaternion.identity);
        Debug.Log($"👹 Enemy spawned at {spawnPoint.position}");
    }

    public void PlayerDied()
    {
        if (gameOver) return;

        Debug.Log("💀 PLAYER DIED - GAME OVER!");
        gameOver = true;

        if (losePanel) losePanel.SetActive(true);
        Time.timeScale = 0f;  // Dừng game
    }

    void Win()
    {
        if (gameOver) return;

        Debug.Log("🎉 YOU WIN!");
        gameOver = true;

        if (winPanel) winPanel.SetActive(true);
        Time.timeScale = 0f;  // Dừng game
    }

    // ✅ HÀM MỚI: Load next level
    public void LoadNextLevel()
    {

        Debug.Log("🔥 NEXT LEVEL BUTTON CLICKED!"); // ← THÊM DÒNG NÀY


        Time.timeScale = 1f;  // Reset time scale

        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
        int nextSceneIndex = currentSceneIndex + 1;

        // Check nếu có scene tiếp theo
        if (nextSceneIndex < SceneManager.sceneCountInBuildSettings)
        {
            SceneManager.LoadScene(nextSceneIndex);
        }
        else
        {
            // Nếu hết level, quay về level đầu
            Debug.Log("✅ All levels completed! Restarting...");
            SceneManager.LoadScene(0);
        }
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}