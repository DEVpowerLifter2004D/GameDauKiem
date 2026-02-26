using UnityEngine;

public class ParallaxLayer : MonoBehaviour
{
    public Transform cameraTransform;
    [Range(0f, 1f)]
    public float parallaxEffect = 0.5f;

    private float spriteWidth;
    private Vector3 lastCameraPos;

    void Start()
    {
        lastCameraPos = cameraTransform.position;
        spriteWidth = GetComponent<SpriteRenderer>().bounds.size.x;
    }

    void LateUpdate()
    {
        Vector3 delta = cameraTransform.position - lastCameraPos;
        transform.position += new Vector3(delta.x * parallaxEffect, 0, 0);
        lastCameraPos = cameraTransform.position;

        // Endless
        float diff = cameraTransform.position.x - transform.position.x;

        if (Mathf.Abs(diff) >= spriteWidth)
        {
            float offset = (diff > 0) ? spriteWidth : -spriteWidth;
            transform.position += new Vector3(offset, 0, 0);
        }
    }
}
