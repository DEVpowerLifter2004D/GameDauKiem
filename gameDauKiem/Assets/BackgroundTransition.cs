using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.LookDev;
using UnityEngine.UI;
using static Unity.Collections.AllocatorManager;

public class BackgroundTransition : MonoBehaviour
{
    [Header("Player Reference")]
    public Transform player;

    [Header("Transition Zone")]
    public float skyZoneY = 0f;           // Y của tầng trên (Sky)
    public float undergroundZoneY = -15f; // Y của tầng dưới (Underground)
    public float transitionHeight = 5f;   // Độ cao vùng chuyển tiếp

    [Header("Camera Background Colors")]
    public Color skyColor = new Color(0.53f, 0.81f, 0.98f, 1f);      // Xanh trời sáng
    public Color undergroundColor = new Color(0.2f, 0.15f, 0.1f, 1f); // Nâu tối

    [Header("Overlay Darkness (Optional)")]
    public Image darkOverlay;              // UI Image màu đen (optional)
    public float maxOverlayAlpha = 0.4f;   // Độ tối tối đa ở dưới

    private Camera mainCamera;

    void Start()
    {
        mainCamera = Camera.main;

        // Tự động tìm player nếu chưa gán
        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
                player = playerObj.transform;
        }

        // Setup overlay nếu có
        if (darkOverlay != null)
        {
            Color c = darkOverlay.color;
            c.a = 0f;
            darkOverlay.color = c;
        }
    }

    void Update()
    {
        if (player == null)
        {
            Debug.LogError("❌ Player is NULL!");
            return;
        }

        float playerY = player.position.y;
        Debug.Log($"Player Y: {playerY}"); // ← THÊM DÒNG NÀY

        // Tính vùng chuyển tiếp
        float transitionStart = undergroundZoneY + transitionHeight;
        float transitionEnd = undergroundZoneY;

        // Tính % transition (0 = sky, 1 = underground)
        float t = 0f;
        if (playerY <= transitionEnd)
        {
            t = 1f; // Hoàn toàn underground
        }
        else if (playerY >= transitionStart)
        {
            t = 0f; // Hoàn toàn sky
        }
        else
        {
            t = Mathf.InverseLerp(transitionStart, transitionEnd, playerY);
        }

        // ✅ Đổi màu camera background
        mainCamera.backgroundColor = Color.Lerp(skyColor, undergroundColor, t);

        // ✅ Fade overlay (tối dần khi xuống)
        if (darkOverlay != null)
        {
            Color c = darkOverlay.color;
            c.a = Mathf.Lerp(0f, maxOverlayAlpha, t);
            darkOverlay.color = c;
        }
    }
}
