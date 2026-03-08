using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class DebugEnemies : MonoBehaviour
{
    void Start()
    {
        EnemyController[] enemies = FindObjectsOfType<EnemyController>();
        Debug.Log($"🔍 TOTAL ENEMIES FOUND: {enemies.Length}");

        for (int i = 0; i < enemies.Length; i++)
        {
            Debug.Log($"Enemy {i}: Position = {enemies[i].transform.position}, Parent = {enemies[i].transform.parent?.name ?? "NULL"}");
        }
    }
}
