using UnityEngine;
public class ParallaxLayer : MonoBehaviour
{
    public Transform cameraTransform;
    public Transform playerTransform; // ← THÊM: kéo Player vào đây
    [Range(0f, 1f)]
    public float parallaxEffect = 0.5f;
    private float spriteWidth;
    private Vector3 startPos;
    private float startPlayerX;

    void Start()
    {
        startPos = transform.position;
        startPlayerX = playerTransform.position.x;
        spriteWidth = GetComponent<SpriteRenderer>().bounds.size.x;
    }

    void LateUpdate()
    {
        // Tính theo PLAYER thay vì camera → không bị lag
        float playerTravelled = playerTransform.position.x - startPlayerX;
        float newX = startPos.x + playerTravelled * parallaxEffect;
        transform.position = new Vector3(newX, transform.position.y, transform.position.z);
        // Bỏ phần Endless loop đi vì map không cuộn nữa
    }
}