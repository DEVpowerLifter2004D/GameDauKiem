using UnityEngine;

public class ParallaxLayer : MonoBehaviour
{
    public Transform cameraTransform;
    public Transform playerTransform;

    [Range(0f, 1f)]
    public float parallaxEffect = 0.5f;

    private Vector3 previousCameraPos;

    void Start()
    {
        // Tự động tìm Camera nếu chưa gán
        if (cameraTransform == null)
            cameraTransform = Camera.main.transform;

        // Tự động tìm Player nếu chưa gán
        if (playerTransform == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
                playerTransform = player.transform;
        }

        if (cameraTransform != null)
            previousCameraPos = cameraTransform.position;
    }

    void LateUpdate()
    {
        if (cameraTransform == null) return;

        // Tính khoảng cách camera di chuyển
        float deltaX = cameraTransform.position.x - previousCameraPos.x;

        // Di chuyển background theo parallax
        transform.position += new Vector3(deltaX * parallaxEffect, 0, 0);

        // Cập nhật vị trí camera
        previousCameraPos = cameraTransform.position;
    }
}