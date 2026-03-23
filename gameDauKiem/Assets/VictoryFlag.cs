using Unity.VisualScripting;
using UnityEngine;

public class VictoryFlag : MonoBehaviour
{
    [Header("Victory Settings")]
    [Tooltip("Có hiển thị hiệu ứng khi thắng không")]
    public bool showVictoryEffect = true;

    void OnTriggerEnter2D(Collider2D other)
    {
        // Kiểm tra có phải Player không
        if (other.CompareTag("Player"))
        {
            Debug.Log("🏁🎉 PLAYER REACHED THE FLAG! VICTORY!");

            // Gọi GameManager để hiện Win Panel
            GameManager gm = FindFirstObjectByType<GameManager>();
            if (gm != null)
            {
                Debug.Log("✅ Calling GameManager.Win()");

                // Nếu GameManager có hàm Win() public
                gm.GetComponent<GameManager>().SendMessage("Win", SendMessageOptions.DontRequireReceiver);
            }
            else
            {
                Debug.LogError("❌ GameManager not found!");
            }

            // Disable collider để không trigger nhiều lần
            GetComponent<Collider2D>().enabled = false;
        }
    }
}
