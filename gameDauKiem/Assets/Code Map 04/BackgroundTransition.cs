using UnityEngine;

public class BackgroundTransition : MonoBehaviour
{
    [Header("Kéo Background màu xanh vào đây")]
    public SpriteRenderer newBackground;
    public float fadeSpeed = 1.5f; // Tốc độ chuyển cảnh (to thì nhanh, nhỏ thì chậm)

    private float targetAlpha = 0f;

    void Start()
    {
        // Vừa vào game là giấu cái nền xanh đi (Alpha = 0)
        if (newBackground != null)
        {
            Color c = newBackground.color;
            c.a = 0f;
            newBackground.color = c;
        }
    }

    void Update()
    {
        if (newBackground != null)
        {
            // Lệnh này giúp background từ từ hiện ra hoặc mờ đi rất mượt
            Color c = newBackground.color;
            c.a = Mathf.MoveTowards(c.a, targetAlpha, fadeSpeed * Time.deltaTime);
            newBackground.color = c;
        }
    }

    // Khi nhân vật bước qua cầu (vào vùng Trigger)
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            targetAlpha = 1f; // Hiện nền xanh lên
        }
    }

    // Khi nhân vật lùi lại qua trái (thoát khỏi vùng Trigger)
    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            targetAlpha = 0f; // Mờ nền xanh đi, lộ nền cam ra
        }
    }
}