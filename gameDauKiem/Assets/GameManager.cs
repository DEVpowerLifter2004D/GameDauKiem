using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

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
    public float bossWarningTime = 3f;

    [Header("Game Rules")]
    public float survivalTime = 60f;

    [Header("UI")]
    public TextMeshProUGUI timerText;
    public TextMeshProUGUI waveText;
    public GameObject winPanel;
    public GameObject losePanel;

    [Header("Boss Warning UI")]
    public GameObject bossWarningPanel;
    public TextMeshProUGUI bossWarningText;
    public TextMeshProUGUI bossCountdownText;

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

        if (enemyPrefab == null)
            Debug.LogError("❌ Enemy prefab not found in Resources!");

        if (bossPrefab == null)
            Debug.LogError("❌ Boss prefab not found in Resources!");
    }

    void Start()
    {
        currentTime = 0f;
        nextSpawnTime = Time.time + spawnCooldown;
        bossWaveStartTime = survivalTime * bossWaveStartPercent;

        if (winPanel) winPanel.SetActive(false);
        if (losePanel) losePanel.SetActive(false);
        if (bossWarningPanel) bossWarningPanel.SetActive(false);

        UpdateWaveText("ENEMY WAVE");

        Debug.Log($"🎮 Game Start | Boss Warning at {bossWaveStartTime - bossWarningTime}s | Boss at {bossWaveStartTime}s");
    }

    void Update()
    {
        if (gameOver) return;

        currentTime += Time.deltaTime;
        UpdateTimerUI();

        float warningTime = bossWaveStartTime - bossWarningTime;

        if (!bossWarningShown && currentTime >= warningTime)
        {
            bossWarningShown = true;
            StartCoroutine(ShowBossWarning());
        }

        if (!isBossWave && currentTime >= bossWaveStartTime)
        {
            StartBossWave();
        }

        if (!isBossWave)
        {
            if (Time.time >= nextSpawnTime && currentAliveCount < maxEnemiesOnScreen)
            {
                SpawnEnemy();
                nextSpawnTime = Time.time + spawnCooldown;
            }
        }
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
        if (bossWarningPanel) bossWarningPanel.SetActive(true);

        if (bossWarningText)
        {
            bossWarningText.gameObject.SetActive(true);
            bossWarningText.text = "⚠ WARNING! MA VƯƠNG TỚI CHƠI BROO!! ⚠";
        }

        if (bossCountdownText)
        {
            bossCountdownText.gameObject.SetActive(true); // ← THÊM DÒNG NÀY

            for (int i = 3; i > 0; i--)
            {
                bossCountdownText.text = i.ToString();
                yield return new WaitForSeconds(1f);
            }

            bossCountdownText.text = "";
            bossCountdownText.gameObject.SetActive(false); // ← TẮT LẠI SAU KHI XONG
        }

        if (bossWarningPanel) bossWarningPanel.SetActive(false);
    }
    void StartBossWave()
    {
        isBossWave = true;

        Debug.Log("👑 Boss Wave Started!");
        UpdateWaveText("⚔ BOSS FIGHT ⚔");

        EnemyController[] enemies = FindObjectsOfType<EnemyController>();

        foreach (var enemy in enemies)
        {
            Destroy(enemy.gameObject);
        }

        currentAliveCount = 0;

        SpawnBoss();
        bossSpawned = true;
    }

    void SpawnEnemy()
    {
        if (enemyPrefab == null || spawnPoints.Length == 0) return;

        Transform point = spawnPoints[Random.Range(0, spawnPoints.Length)];

        Instantiate(enemyPrefab, point.position, Quaternion.identity);

        OnEnemySpawned();
    }

    void SpawnBoss()
    {
        if (bossPrefab == null) return;

        Vector3 spawnPos = bossSpawnPoint != null
            ? bossSpawnPoint.position
            : new Vector3(0, -2, 0);

        bossInstance = Instantiate(bossPrefab, spawnPos, Quaternion.identity);

        Debug.Log("👑 Boss Spawned!");
    }

    void UpdateTimerUI()
    {
        if (timerText == null) return;

        float timeLeft = survivalTime - currentTime;

        int minutes = Mathf.FloorToInt(timeLeft / 60f);
        int seconds = Mathf.FloorToInt(timeLeft % 60);

        timerText.text = $"{minutes:00}:{seconds:00}";
    }

    void UpdateWaveText(string text)
    {
        if (waveText)
            waveText.text = text;
    }

    public void OnEnemySpawned()
    {
        currentAliveCount++;
    }

    public void OnEnemyDied()
    {
        currentAliveCount--;

        if (currentAliveCount < 0)
            currentAliveCount = 0;
    }

    public void PlayerDied()
    {
        if (gameOver) return;

        gameOver = true;

        Debug.Log("💀 Player Died");

        if (losePanel) losePanel.SetActive(true);

        Time.timeScale = 0f;
    }

    void Win()
    {
        if (gameOver) return;

        gameOver = true;

        Debug.Log("🎉 Boss Defeated - You Win!");

        if (winPanel) winPanel.SetActive(true);

        Time.timeScale = 0f;
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;

        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
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

    public void QuitGame()
    {
        Application.Quit();
    }
}