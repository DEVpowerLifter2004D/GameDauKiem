using System.Collections;
using UnityEngine;

public class BoarCharge : MonoBehaviour
{
    [Header("Chỉ số của Heo")]
    public float chargeSpeed = 6f;      // Tốc độ lúc húc (chỉnh càng to chạy càng gắt)
    public float detectRange = 5f;      // Tầm nhìn phát hiện người chơi

    [Header("Thành phần kết nối")]
    public Transform player;            // Mục tiêu để húc

    private Rigidbody2D rb;
    private Animator anim;

    private bool isCharging = false;
    private bool isPreparing = false;
    private int facingDirection = -1;   // Mặc định heo đang quay mặt sang trái (-1)

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();

        // Tự động tìm nhân vật chính nếu bạn quên kéo thả vào
        if (player == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) player = p.transform;
        }
    }

    void Update()
    {
        if (player == null) return;

        // Tính khoảng cách từ heo đến người chơi
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        // Nếu người lọt vào tầm nhìn và heo chưa bắt đầu húc
        if (distanceToPlayer <= detectRange && !isCharging && !isPreparing)
        {
            // Tính toán xem người đang ở bên trái hay bên phải con heo
            float directionToPlayer = player.position.x - transform.position.x;

            // Lật mặt heo về đúng phía người chơi
            if (directionToPlayer > 0 && facingDirection == -1) Flip();
            else if (directionToPlayer < 0 && facingDirection == 1) Flip();

            // Khởi động chuỗi hành động: Lấy đà -> Húc
            StartCoroutine(PrepareAndCharge());
        }

        // Truyền tốc độ vào Animator để kích hoạt sợi dây nối sang cuộn phim "Run"
        anim.SetFloat("Speed", Mathf.Abs(rb.linearVelocity.x));
    }

    // Cuộn phim hành động của Heo
    IEnumerator PrepareAndCharge()
    {
        isPreparing = true;
        rb.linearVelocity = Vector2.zero; // Khựng lại lấy đà

        // Heo đứng gồng 0.5 giây (bạn có thể chèn âm thanh rống lên ở đây)
        yield return new WaitForSeconds(0.5f);

        isPreparing = false;
        isCharging = true;

        // Phóng thẳng về phía trước với tốc độ cao
        rb.linearVelocity = new Vector2(chargeSpeed * facingDirection, rb.linearVelocity.y);

        // Chạy thục mạng trong 1.5 giây rồi hết hơi
        yield return new WaitForSeconds(1.5f);

        rb.linearVelocity = Vector2.zero; // Phanh gấp
        isCharging = false;

        // Đứng thở dốc 1.5 giây trước khi có thể húc hiệp tiếp theo
        yield return new WaitForSeconds(1.5f);
    }

    // Hàm lật mặt quái vật
    void Flip()
    {
        facingDirection *= -1;
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
    }

    // Vẽ một vòng tròn màu đỏ trong lúc thiết kế để bạn dễ căn chỉnh tầm nhìn của heo
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectRange);
    }
}