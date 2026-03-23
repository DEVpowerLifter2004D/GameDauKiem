using UnityEngine;

public class BossFire : MonoBehaviour
{
    [Header("Boss Stats")]
    public int maxHealth = 20;
    [SerializeField] private int currentHealth;

    [Header("Movement")]
    public float moveSpeed = 2f;
    public float chaseRange = 8f;

    [Header("Combat")]
    public float attackRange = 1.5f;
    public int attackDamage = 20;
    public float attackCooldown = 2f;
    public float attackDuration = 1.0f;
    public float attackDamageDelay = 0.4f;

    [Header("Ground Check")]
    public float groundCheckRadius = 0.5f;

    public Transform groundCheck;
    private LayerMask groundLayer;
    private float lastAttackTime;
    private Transform player;
    private int direction = 1;
    private float originalScaleX;
    private Animator anim;
    private Rigidbody2D rb;
    private bool isGrounded;
    private bool isAttacking = false;
    private bool isDead = false;

    // =============================================
    void Start()
    {
        maxHealth = 20; // Quét sạch giá trị trên Inspector, ép về 50
        currentHealth = maxHealth;
        originalScaleX = -Mathf.Abs(transform.localScale.x);

        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();

        // Kiểm tra Animator
        if (anim != null && anim.runtimeAnimatorController == null)
        {
            Debug.LogWarning("⚠️ BossFire không có Animator Controller!");
            anim.enabled = false;
            anim = null;
        }

        // Kiểm tra layer Enemy
        if (gameObject.layer != LayerMask.NameToLayer("Enemy"))
        {
            string layerName = LayerMask.LayerToName(gameObject.layer);
            Debug.LogError($"❌ BossFire Layer = '{layerName}', PHẢI LÀ 'Enemy'!");
        }

        // Tạo GroundCheck tự động nếu chưa có
        groundCheck = transform.Find("GroundCheck");
        if (groundCheck == null)
        {
            GameObject checkObj = new GameObject("GroundCheck");
            checkObj.transform.parent = transform;
            checkObj.transform.localPosition = new Vector3(0, -0.6f, 0);
            groundCheck = checkObj.transform;
        }

        groundLayer = LayerMask.GetMask("Ground");

        // Tìm Player
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
            player = playerObj.transform;
        else
            Debug.LogError("❌ BossFire không tìm thấy Player! Kiểm tra tag.");
    }

    // =============================================
    void FixedUpdate()
    {
        if (isDead || player == null || rb == null) return;

        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
        Debug.Log($"isGrounded={isGrounded}, groundLayer={groundLayer.value}, pos={groundCheck.position}");

        // Nếu đang tấn công mà player chạy xa → hủy attack
        if (isAttacking)
        {
            float dist = Vector2.Distance(transform.position, player.position);
            if (dist > attackRange * 2f)
                CancelAttack();
        }

        if (!isAttacking)
        {
            ChasePlayer();
            CheckAttack(); // 🔥 THÊM DÒNG NÀY NGAY ĐÂY
        }
        else
        {
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
        }
    }

        // =============================================
        void ChasePlayer()
    {
        float distToPlayer = Vector2.Distance(transform.position, player.position);

        if (distToPlayer <= chaseRange)
        {
            if (distToPlayer > attackRange)
            {
                // Đuổi theo
                if (anim != null) anim.SetBool("isWalking", true);

                int chaseDir = (player.position.x > transform.position.x) ? 1 : -1;
                if (chaseDir != direction) { direction = chaseDir; Flip(); }

                rb.linearVelocity = new Vector2(direction * moveSpeed, rb.linearVelocity.y);
            }
            else
            {
                // Đứng yên chờ tấn công
                rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
                if (anim != null) anim.SetBool("isWalking", false);

                int faceDir = (player.position.x > transform.position.x) ? 1 : -1;
                if (faceDir != direction) { direction = faceDir; Flip(); }
            }
        }
        else
        {
            // Player ra ngoài tầm → đứng yên
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
            if (anim != null) anim.SetBool("isWalking", false);
        }
    }

    // =============================================
    void CheckAttack()
    {
        if (player == null || isAttacking) return;

        float distToPlayer = Vector2.Distance(transform.position, player.position);

        if (distToPlayer <= attackRange && Time.time >= lastAttackTime + attackCooldown)
        {
            isAttacking = true;
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);

            if (anim != null)
            {
                anim.SetBool("isWalking", false);
                anim.ResetTrigger("attack");
                anim.SetTrigger("attack");
            }

            Invoke("DealDamageToPlayer", attackDamageDelay);
            Invoke("EndAttack", attackDuration);
            lastAttackTime = Time.time;
            Debug.Log("ATTACK!!!");
        }
    }

    void DealDamageToPlayer()
    {
        if (player == null) return;

        float distToPlayer = Vector2.Distance(transform.position, player.position);
        if (distToPlayer <= attackRange)
        {
            PlayerController pc = player.GetComponent<PlayerController>();
            if (pc != null)
                pc.TakeDamage(attackDamage);
        }
    }

    void EndAttack() { isAttacking = false; }

    void CancelAttack()
    {
        isAttacking = false;
        CancelInvoke("DealDamageToPlayer");
        CancelInvoke("EndAttack");
        if (anim != null) anim.ResetTrigger("attack");
    }

    // =============================================
    void Flip()
    {
        Vector3 scale = transform.localScale;
        scale.x = originalScaleX * direction;
        transform.localScale = scale;
    }

    // =============================================
    public void TakeDamage(int damage)
    {
        if (isDead) return;
        currentHealth -= damage;
        Debug.Log($"🔥 Boss HP: {currentHealth}/{maxHealth}");
        
        // Chỉ reset animation và flag, KHÔNG hủy Invoke đang chờ
        isAttacking = false;
        if (anim != null) anim.ResetTrigger("attack");

        anim?.SetTrigger("takeHit");

        if (currentHealth <= 0) Die();
        Debug.Log("BOSS HIT!");
    }
    public int GetCurrentHealth() { return currentHealth; }
    public int GetMaxHealth() { return maxHealth; }

    // =============================================
    void Die()
    {
        if (isDead) return;
        isDead = true;

        if (anim != null) anim.SetBool("isDead", true);
        if (rb != null) rb.linearVelocity = Vector2.zero;

        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false;

        Destroy(gameObject, 2f);
    }

    // =============================================
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, chaseRange);
    }
}