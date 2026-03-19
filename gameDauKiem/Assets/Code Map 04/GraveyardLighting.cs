using UnityEngine;
using UnityEngine.Rendering.Universal; // Thư viện bắt buộc để điều khiển ánh sáng 2D

public class GraveyardLighting : MonoBehaviour
{
    [Header("Kéo Global Light 2D vào đây")]
    public Light2D globalLight;

    [Header("Cài đặt màu sắc")]
    public Color darkColor = new Color(0.2f, 0.2f, 0.35f); // Màu tối u ám (xanh xám)
    public float transitionSpeed = 1.5f; // Tốc độ đổi màu (càng to đổi càng nhanh)

    private Color normalColor; // Bộ nhớ lưu lại màu nắng ban ngày
    private Color targetColor;

    void Start()
    {
        if (globalLight != null)
        {
            normalColor = globalLight.color;
            targetColor = normalColor;
        }
    }

    void Update()
    {
        if (globalLight != null)
        {
            // Lệnh Lerp giúp đổi màu từ từ cực kỳ mượt mà
            globalLight.color = Color.Lerp(globalLight.color, targetColor, transitionSpeed * Time.deltaTime);
        }
    }

    // Khi nhân vật bước vào vùng cảm ứng
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            targetColor = darkColor; // Bắt đầu tắt đèn
        }
    }

    // Khi nhân vật quay lưng đi khỏi nghĩa địa
    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            targetColor = normalColor; // Bật nắng lên lại
        }
    }
}