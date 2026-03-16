using System.Collections;
using System.Reflection;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using static UnityEditor.PlayerSettings;

public class GameManager : MonoBehaviour
{
    [Header("Enemy Spawning")]
    public Transform[] spawnPoints;
    public float spawnCooldown = 2f;
    public int maxEnemiesOnScreen = 15;

    [Header("Boss Wave Settings")]
    [Range(0f, 1f)]
    public float bossWaveStartPercent = 0.5f;
    public Transform bossSpawnPoint;
    public float bossWarningTime = 3f; // Cảnh báo trước 3 giây

    [Header("Game Rules")]
    public float survivalTime = 60f;

    [Header("UI")]
    public TextMeshProUGUI timerText;
    public TextMeshProUGUI waveText;
    public GameObject winPanel;
    public GameObject losePanel;

    [Header("Boss Warning UI")]
    public GameObject bossWarningPanel; // Panel cảnh báo Boss
    public TextMeshProUGUI bossWarningText; // Text "WARNING! BOSS INCOMING!"
    public TextMeshProUGUI bossCountdownText; // Text "3" "2" "1"

    private float currentTime;
    private float nextSpawnTime;
    private bool gameOver = false;
    private int currentAliveCount = 0;

    private bool isBossWave = false;
    private bool bossSpawned = false;
    private bool bossWarningShown = false;
    private float bossWaveStartTime;
    private GameObject bossInstance;

    private GameObject enemyPrefab;
    private GameObject bossPrefab;

    void Awake()
    {
        enemyPrefab = Resources.Load<GameObject>("Enemy");
        bossPrefab = Resources.Load<GameObject>("Boss");

        if (enemyPrefab != null)
            Debug.Log("✅ Loaded Enemy prefab");
        else
            Debug.LogError("❌ Failed to load Enemy prefab!");

        if (bossPrefab != null)
            Debug.Log("✅ Loaded Boss prefab");
        else
            Debug.LogError("❌ Failed to load Boss prefab!");
    }

    void Start()
    {
        currentTime = 0f;
        nextSpawnTime = Time.time + spawnCooldown;
        bossWaveStartTime = survivalTime * bossWaveStartPercent;

        if (winPanel) winPanel.SetActive(false);
        if (losePanel) losePanel.SetActive(false);

        // ✅ Hide Boss Warning Panel
        if (bossWarningPanel) bossWarningPanel.SetActive(false);

        EnemyController[] existing = FindObjectsOfType<EnemyController>();
        currentAliveCount = existing.Length;

        UpdateWaveText("ENEMY WAVE");

        Debug.Log($"🎮 Game Start | Boss Warning at: {bossWaveStartTime - bossWarningTime}s, Boss at: {bossWaveStartTime}s");
    }
    void Update()
    {
        if (gameOver) return;

        currentTime += Time.deltaTime;
        UpdateTimerUI();

        float warningTime = bossWaveStartTime - bossWarningTime; // 27s

        // ✅ DEBUG: In ra LIÊN TỤC để kiểm tra
        if (currentTime >= 26f && currentTime <= 35f)
        {
            Debug.Log($"⏰ Time: {currentTime:F2}s | warningTime: {warningTime:F2}s | bossWarningShown: {bossWarningShown}");
        }

        // ✅ HIỂN THỊ CẢNH BÁO Ở 27s (TRƯỚC 3 GIÂY)
        if (!bossWarningShown && currentTime >= warningTime)
        {
            bossWarningShown = true;
            StartCoroutine(ShowBossWarning());
            Debug.Log("🚨 WARNING TRIGGERED at 27s!");
        }

        if (currentTime >= 26f && currentTime <= 35f)
        {
            Debug.Log($"⏰ Current Time: {currentTime:F2}s | Warning at: {warningTime:F2}s | Boss at: {bossWaveStartTime:F2}s");
        }

        // ✅ HIỂN THỊ CẢNH BÁO Ở 27s (TRƯỚC 3 GIÂY)
        if (!bossWarningShown && currentTime >= warningTime)
        {
            bossWarningShown = true;
            StartCoroutine(ShowBossWarning());
            Debug.Log("🚨 WARNING TRIGGERED at 27s!");
        }

        // ✅ BẮT ĐẦU BOSS WAVE Ở 30s
        if (!isBossWave && currentTime >= bossWaveStartTime)
        {
            StartBossWave();
        }

        // Enemy Wave
        if (!isBossWave)
        {
            if (Time.time >= nextSpawnTime && currentAliveCount < maxEnemiesOnScreen)
            {
                SpawnEnemy();
                nextSpawnTime = Time.time + spawnCooldown;
            }
        }
        // Boss Wave
        else
        {
            if (bossSpawned && bossInstance == null)
            {
                Win();
            }
        }

        if (currentTime >= survivalTime && isBossWave && bossInstance != null)
        {
            PlayerDied();
        }
    }
    IEnumerator ShowBossWarning()
    {
        Debug.Log("⚠️ BOSS WARNING STARTED!");

        // ✅ FORCE Panel full screen
        if (bossWarningPanel)
        {
            RectTransform panelRT = bossWarningPanel.GetComponent<RectTransform>();
            panelRT.anchorMin = Vector2.zero;
            panelRT.anchorMax = Vector2.one;
            panelRT.offsetMin = Vector2.zero;
            panelRT.offsetMax = Vector2.zero;
            panelRT.localScale = Vector3.one; // 👈 FIX SCALE!

            bossWarningPanel.SetActive(true);

            var img = bossWarningPanel.GetComponent<UnityEngine.UI.Image>();
            if (img == null) img = bossWarningPanel.AddComponent<UnityEngine.UI.Image>();
            img.color = new Color(0, 0, 0, 0.8f);

            Debug.Log("✅ Panel FULL SCREEN!");
        }

        // ✅ Warning text
        if (bossWarningText)
        {
            bossWarningText.gameObject.SetActive(true);

            RectTransform wtRT = bossWarningText.GetComponent<RectTransform>();
            wtRT.anchorMin = new Vector2(0.5f, 0.5f);
            wtRT.anchorMax = new Vector2(0.5f, 0.5f);
            wtRT.pivot = new Vector2(0.5f, 0.5f);
            wtRT.anchoredPosition = new Vector2(0, 100);
            wtRT.sizeDelta = new Vector2(1000, 200);
            wtRT.localScale = Vector3.one; // 👈 FIX SCALE!

            bossWarningText.text = "⚠️ WARNING! BOSS INCOMING! ⚠️";
            bossWarningText.fontSize = 80;
            bossWarningText.color = Color.red;
            bossWarningText.alignment = TextAlignmentOptions.Center;
        }

        // ✅ Countdown text
        if (bossCountdownText)
        {
            bossCountdownText.gameObject.SetActive(true);

            RectTransform ctRT = bossCountdownText.GetComponent<RectTransform>();
            ctRT.anchorMin = new Vector2(0.5f, 0.5f);
            ctRT.anchorMax = new Vector2(0.5f, 0.5f);
            ctRT.pivot = new Vector2(0.5f, 0.5f);
            ctRT.anchoredPosition = new Vector2(0, -100);
            ctRT.sizeDelta = new Vector2(800, 400);
            ctRT.localScale = Vector3.one; // 👈 FIX SCALE!

            bossCountdownText.text = "";
            bossCountdownText.alignment = TextAlignmentOptions.Center;
        }

        yield return new WaitForSecondsRealtime(1f);

        // Hide warning text
        if (bossWarningText) bossWarningText.gameObject.SetActive(false);

        // Countdown 3-2-1
        for (int i = 3; i > 0; i--)
        {
            if (bossCountdownText)
            {
                bossCountdownText.text = i.ToString();
                bossCountdownText.fontSize = 250;
                bossCountdownText.color = Color.yellow;

                Debug.Log($"⏰ COUNTDOWN: {i}");
            }

            yield return new WaitForSecondsRealtime(1f);
        }

        // Hide countdown
        if (bossCountdownText)
        {
            bossCountdownText.text = "";
            bossCountdownText.gameObject.SetActive(false);
        }

        // Show "BOSS APPEARED!"
        if (bossWarningText)
        {
            bossWarningText.gameObject.SetActive(true);
            bossWarningText.text = "👑 BOSS APPEARED! 👑";
            bossWarningText.fontSize = 80;
            bossWarningText.color = Color.red;
        }

        yield return new WaitForSecondsRealtime(1.5f);

        // Hide all
        if (bossWarningPanel) bossWarningPanel.SetActive(false);

        Debug.Log("✅ Boss Warning FINISHED!");
    }
     void StartBossWave()
    {
        isBossWave = true;

        Debug.Log("👑 BOSS WAVE STARTED at 30s!");
        UpdateWaveText("⚔️ BOSS FIGHT ⚔️");

        // Clear all enemies
        EnemyController[] enemies = FindObjectsOfType<EnemyController>();
        foreach (var enemy in enemies)
        {
            Destroy(enemy.gameObject);
        }
        currentAliveCount = 0;

        Debug.Log($"🧹 Cleared {enemies.Length} enemies for Boss fight!");

        // ✅ SPAWN BOSS NGAY
        SpawnBoss();
        bossSpawned = true;
    }
    void SpawnEnemy()
    {
        if (enemyPrefab == null)
        {
            enemyPrefab = Resources.Load<GameObject>("Enemy");
            if (enemyPrefab == null) return;
        }

        if (spawnPoints == null || spawnPoints.Length == 0)
        {
            Debug.LogError("❌ No spawn points!");
            return;
        }

        Transform point = spawnPoints[Random.Range(0, spawnPoints.Length)];
        GameObject newEnemy = Instantiate(enemyPrefab, point.position, Quaternion.identity);
    }

    void SpawnBoss()
    {
        if (bossPrefab == null)
        {
            bossPrefab = Resources.Load<GameObject>("Boss");
            if (bossPrefab == null)
            {
                Debug.LogError("❌ Cannot spawn Boss!");
                return;
            }
        }

        Vector3 spawnPos = bossSpawnPoint != null
            ? bossSpawnPoint.position
            : new Vector3(0, -2, 0);

        bossInstance = Instantiate(bossPrefab, spawnPos, Quaternion.identity);

        Debug.Log($"👑 BOSS SPAWNED at {spawnPos}!");

        // Optional: Camera shake
        // CameraShake.Shake(0.5f, 0.3f);
    }

    void UpdateTimerUI()
    {
        if (timerText == null) return;

        float timeLeft = survivalTime - currentTime;
        int minutes = Mathf.FloorToInt(timeLeft / 60f);
        int seconds = Mathf.FloorToInt(timeLeft % 60);

        timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
    }

    void UpdateWaveText(string text)
    {
        if (waveText != null)
        {
            waveText.text = text;
        }
    }

    public void OnEnemySpawned()
    {
        currentAliveCount++;
    }

    public void OnEnemyDied()
    {
        currentAliveCount--;
    }

    public void PlayerDied()
    {
        if (gameOver) return;
        gameOver = true;

        Debug.Log("💀 PLAYER DIED!");

        if (losePanel) losePanel.SetActive(true);
        Time.timeScale = 0f;
    }

    void Win()
    {
        if (gameOver) return;
        gameOver = true;

        Debug.Log("🎉 YOU WIN! Boss defeated!");

        if (winPanel) winPanel.SetActive(true);
        Time.timeScale = 0f;
    }

    public void LoadNextLevel()
    {
        Time.timeScale = 1f;
        int next = SceneManager.GetActiveScene().buildIndex + 1;
        if (next < SceneManager.sceneCountInBuildSettings)
            SceneManager.LoadScene(next);
        else
            SceneManager.LoadScene(0);
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
