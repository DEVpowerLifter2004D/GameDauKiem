using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;
    public float smoothSpeed = 5f;
    public Vector3 offset = new Vector3(0, 0, -10);

    void Start()
    {
        if (target == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
                target = player.transform;
        }

        // ✅ SET CAMERA Z NGAY TỪ ĐẦU
        transform.position = new Vector3(
            transform.position.x,
            transform.position.y,
            -10f  // ← BẮT BUỘC!
        );
    }

    void LateUpdate()
    {
        if (!target) return;

        // ✅ Vị trí mục tiêu
        Vector3 desiredPos = new Vector3(
            target.position.x + offset.x,
            target.position.y + offset.y,
            -10f  // ← LUÔN LUÔN LÀ -10, KHÔNG DÙNG offset.z
        );

        transform.position = Vector3.Lerp(
            transform.position,
            desiredPos,
            smoothSpeed * Time.deltaTime
        );
    }
}