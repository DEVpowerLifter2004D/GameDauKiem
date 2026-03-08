using UnityEngine;

public class EnemyController : MonoBehaviour
{
    [Header("Stats")]
    public int maxHealth = 3;
    private int currentHealth;

    [Header("Movement")]
    public float moveSpeed = 2f;
    public float patrolDistance = 3f;
    public float chaseRange = 4f;  // ✅ THÊM: Khoảng cách đuổi theo Player

    [Header("Combat")]
    public float attackRange = 2f;  // ✅ TĂNG từ 1.5 → 2.0
    public int attackDamage = 1;
    public float attackCooldown = 1.5f;
    public float attackDuration = 0.8f;

    [Header("Ground Check")]
    public float groundCheckRadius = 0.5f;

    private Transform groundCheck;
    private LayerMask groundLayer;
    private float lastAttackTime;
    private Transform player;
    private Vector3 startPos;
    private int direction = 1;
    private float originalScaleX;  // ✅ LƯU scale gốc
    private Animator anim;
    private Rigidbody2D rb;
    private bool isGrounded;
    private bool isAttacking = false;
    void Start()
    {
        currentHealth = maxHealth;
        startPos = transform.position;
        originalScaleX = Mathf.Abs(transform.localScale.x);
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();

        // ✅ DEBUG: In ra vị trí và parent của Enemy
        Debug.Log($"🎯 ENEMY FOUND AT: {transform.position}, Parent: {transform.parent?.name ?? "NULL"}, GameObject: {gameObject.name}");

        groundCheck = transform.Find("GroundCheck");
        if (groundCheck == null)
        {
            GameObject checkObj = new GameObject("GroundCheck");
            checkObj.transform.parent = transform;
            checkObj.transform.localPosition = new Vector3(0, -0.6f, 0);
            groundCheck = checkObj.transform;
            Debug.Log("✅ Created GroundCheck");
        }

        groundLayer = LayerMask.GetMask("Ground");

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
            Debug.Log("✅ Enemy found Player!");
        }

        Debug.Log($"🔥 ENEMY START - AttackRange: {attackRange}, ChaseRange: {chaseRange}");
    }
    void FixedUpdate()
    {
        if (player == null) return;

        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

        // ✅ THÊM DEBUG NÀY
        Debug.Log($"🟢 isGrounded: {isGrounded} | Velocity: {rb.linearVelocity.x:F2}");

        // ✅ CANCEL attack nếu Player xa quá
        if (isAttacking)
        {
            float distToPlayer = Vector2.Distance(transform.position, player.position);
            if (distToPlayer > attackRange * 2f)
            {
                CancelAttack();
            }
        }

        if (isGrounded && !isAttacking)
        {
            Patrol();
        }
        else if (isGrounded && isAttacking)
        {
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
        }
        else
        {
            // ✅ THÊM DEBUG KHI KHÔNG GROUNDED
            Debug.LogWarning("⚠️ ENEMY NOT GROUNDED! Falling or blocked!");
        }

        CheckAttack();
    }
    void CancelAttack()
    {
        isAttacking = false;
        CancelInvoke("DealDamageToPlayer");
        CancelInvoke("EndAttack");

        if (anim != null)
        {
            anim.ResetTrigger("Attack");
        }
    }
    void Patrol()
    {
        if (player == null) return;

        float distToPlayer = Vector2.Distance(transform.position, player.position);
        float distToStart = Mathf.Abs(transform.position.x - startPos.x);

        // ✅ DEBUG
        string state = "";
        if (distToPlayer <= attackRange)
            state = "STOP";
        else if (distToPlayer <= chaseRange)
            state = "CHASE";
        else
            state = "PATROL";

        Debug.Log($"🤖 {state} | Dist: {distToPlayer:F2} | Pos: {transform.position.x:F2} | Start: {startPos.x:F2} | Dir: {direction}");

        // ==========================================
        // ✅ CASE 1: GẦN PLAYER - Dừng lại, quay mặt
        // ==========================================
        if (distToPlayer <= attackRange)
        {
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
            if (anim != null) anim.SetBool("isWalking", false);

            // Quay mặt về Player
            int faceDir = (player.position.x > transform.position.x) ? 1 : -1;
            if (faceDir != direction)
            {
                direction = faceDir;
                Flip();
            }
        }
        // ==========================================
        // ✅ CASE 2: CHASE - Đuổi theo Player (trong chaseRange)
        // ==========================================
        else if (distToPlayer <= chaseRange)
        {
            if (anim != null) anim.SetBool("isWalking", true);

            // Xác định hướng chase
            int chaseDir = (player.position.x > transform.position.x) ? 1 : -1;

            // CHỈ flip khi cần thiết
            if (chaseDir != direction)
            {
                direction = chaseDir;
                Flip();
            }

            // Di chuyển nhanh về phía Player
            rb.linearVelocity = new Vector2(direction * moveSpeed * 1.5f, rb.linearVelocity.y);
        }
        // ==========================================
        // ✅ CASE 3: PATROL - Player quá xa, tuần tra quanh startPos
        // ==========================================
        else
        {
            if (anim != null) anim.SetBool("isWalking", true);

            // 🔄 Nếu đi quá xa startPos → quay về
            if (distToStart > patrolDistance)
            {
                // Xác định hướng về startPos
                int returnDir = (startPos.x > transform.position.x) ? 1 : -1;

                if (returnDir != direction)
                {
                    direction = returnDir;
                    Flip();
                    Debug.Log("🔄 Turning back to start");
                }
            }

            // Di chuyển theo hướng hiện tại
            rb.linearVelocity = new Vector2(direction * moveSpeed, rb.linearVelocity.y);

            // ⚠️ Kiểm tra STUCK (va tường)
            if (Mathf.Abs(rb.linearVelocity.x) < 0.1f && moveSpeed > 0)
            {
                Debug.LogWarning("🚫 STUCK! Reversing...");
                direction *= -1;
                Flip();
            }
        }
    }
    void Flip()
    {
        // ✅ FIX: Giữ nguyên scale gốc, chỉ đổi hướng
        Vector3 scale = transform.localScale;
        scale.x = originalScaleX * direction;
        transform.localScale = scale;
    }

    void CheckAttack()
    {
        if (player == null || isAttacking) return;

        float distToPlayer = Vector2.Distance(transform.position, player.position);

        // ✅ DEBUG: Xem enemy đang ở trạng thái nào
        Debug.Log($"Distance: {distToPlayer:F2}, AttackRange: {attackRange}, isAttacking: {isAttacking}");

        if (distToPlayer <= attackRange && Time.time >= lastAttackTime + attackCooldown)
        {
            isAttacking = true;
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);

            if (anim != null)
            {
                anim.SetBool("isWalking", false);  // ✅ TẮT walking trước
                anim.ResetTrigger("Attack");
                anim.SetTrigger("Attack");
            }

            Invoke("DealDamageToPlayer", 0.3f);
            Invoke("EndAttack", attackDuration);

            lastAttackTime = Time.time;
            Debug.Log("🗡️ ATTACK TRIGGERED!");
        }
    }

    void DealDamageToPlayer()
    {
        if (player == null) return;

        float distToPlayer = Vector2.Distance(transform.position, player.position);
        if (distToPlayer <= attackRange)
        {
            PlayerController playerCtrl = player.GetComponent<PlayerController>();
            if (playerCtrl != null)
            {
                playerCtrl.TakeDamage(attackDamage);
                Debug.Log("🗡️ Enemy HIT Player!");
            }
        }
    }

    void EndAttack()
    {
        isAttacking = false;
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        Debug.Log($"💔 Enemy HP: {currentHealth}/{maxHealth}");

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        Debug.Log("💀 Enemy died!");
        Destroy(gameObject);
    }

    void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, chaseRange);
    }
}