using UnityEngine;

public class BeeAI : MonoBehaviour
{
    public Transform player;
    public float patrolSpeed = 2f;
    public float chaseSpeed = 4f;

    public float chaseRange = 5f;
    public float attackRange = 1.5f;
    public float patrolDistance = 3f;

    // --- BỘ VŨ KHÍ MỚI CỦA ONG ---
    public int damage = 1; // Lượng máu trừ đi mỗi lần chích
    public float attackRate = 1.5f; // Tốc độ chích (1.5 giây chích 1 lần)
    private float nextAttackTime = 0f; // Đồng hồ đếm thời gian

    private Animator anim;
    private Vector3 originalScale;
    private Vector3 startPos;
    private bool movingRight = true;

    void Start()
    {
        anim = GetComponent<Animator>();
        originalScale = transform.localScale;
        startPos = transform.position;

        if (player == null)
        {
            player = GameObject.FindGameObjectWithTag("Player").transform;
        }
    }

    void Update()
    {
        if (player == null) return;

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        if (distanceToPlayer < chaseRange)
        {
            if (player.position.x > transform.position.x)
                transform.localScale = new Vector3(-Mathf.Abs(originalScale.x), originalScale.y, originalScale.z);
            else
                transform.localScale = new Vector3(Mathf.Abs(originalScale.x), originalScale.y, originalScale.z);

            if (distanceToPlayer > attackRange)
            {
                transform.position = Vector2.MoveTowards(transform.position, player.position, chaseSpeed * Time.deltaTime);
            }
            else
            {
                // --- ĐÃ ÁP SÁT VÀ BẮT ĐẦU CHÍCH ---
                if (Time.time >= nextAttackTime)
                {
                    anim.SetTrigger("IsAttacking"); // Kích hoạt hình ảnh chích

                    // --- GỌI HÀM TRỪ MÁU PLAYER Ở ĐÂY ---
                    // Bạn xóa 2 dấu gạch chéo '//' ở 2 dòng dưới này đi để kích hoạt nhé:
                    // Playerhealthui playerHp = player.GetComponent<Playerhealthui>();
                    // if (playerHp != null) playerHp.TakeDamage(damage);

                    // Cài lại đồng hồ chờ cho cú chích tiếp theo
                    nextAttackTime = Time.time + attackRate;
                }
            }
        }
        else
        {
            if (movingRight)
            {
                transform.Translate(Vector2.right * patrolSpeed * Time.deltaTime);
                transform.localScale = new Vector3(-Mathf.Abs(originalScale.x), originalScale.y, originalScale.z);

                if (transform.position.x > startPos.x + patrolDistance)
                    movingRight = false;
            }
            else
            {
                transform.Translate(Vector2.left * patrolSpeed * Time.deltaTime);
                transform.localScale = new Vector3(Mathf.Abs(originalScale.x), originalScale.y, originalScale.z);

                if (transform.position.x < startPos.x - patrolDistance)
                    movingRight = true;
            }
        }
    }
}