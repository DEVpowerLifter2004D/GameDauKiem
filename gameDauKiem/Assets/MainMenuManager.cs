using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    // Panel để chọn level (Setting Panel)
    public GameObject levelSelectPanel;

    public void PlayGame()
    {
        SceneManager.LoadScene(0);   // Tự động vào Round 1 (Level 1)
    }

    public void OpenLevelSelect()
    {
        if (levelSelectPanel != null)
        {
            levelSelectPanel.SetActive(true);   // Mở bảng chọn round
        }
        else
        {
            Debug.LogWarning("Chưa kéo LevelSelectPanel vào slot!");
        }
    }

    public void CloseLevelSelect()
    {
        if (levelSelectPanel != null)
            levelSelectPanel.SetActive(false);
    }

    public void LoadLevel(int levelIndex)
    {
        SceneManager.LoadScene(levelIndex);   // levelIndex = 1,2,3,4,5
    }

    public void QuitGame()
    {
        Debug.Log("Thoát game!");
        Application.Quit();
    }
}