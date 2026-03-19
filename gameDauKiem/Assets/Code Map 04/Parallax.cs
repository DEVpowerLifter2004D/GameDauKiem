using UnityEngine;

public class Parallax : MonoBehaviour
{
    public Camera cam;
    public float parallaxEffect; // Chỉnh tốc độ (0 = đứng im, 1 = trượt theo cam)

    private float startPosX;

    void Start()
    {
        startPosX = transform.position.x;
    }

    void LateUpdate()
    {
        // Tính toán khoảng cách trượt
        float dist = (cam.transform.position.x * parallaxEffect);

        // Di chuyển lớp background
        transform.position = new Vector3(startPosX + dist, transform.position.y, transform.position.z);
    }
}