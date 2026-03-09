using UnityEngine;

public class EnemyController : MonoBehaviour
{
    [Header("Stats")]
    public int maxHealth = 3;
    private int currentHealth;

    [Header("Movement")]
    public float moveSpeed = 2f;
    public float patrolDistance = 3f;
    public float chaseRange = 5f;

    [Header("Combat")]
    public float attackRange = 3f;
    public int attackDamage = 1;
    public float attackCooldown = 1.5f;
    public float attackDuration = 0.8f;
    public float attackDamageDelay = 0.3f;  // ✅ THÊM: Delay trước khi deal damage

    [Header("Ground Check")]
    public float groundCheckRadius = 0.5f;

    private Transform groundCheck;
    private LayerMask groundLayer;
    private float lastAttackTime;
    private Transform player;
    private Vector3 startPos;
    private int direction = 1;
    private float originalScaleX;
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

        // Cancel attack nếu Player xa quá
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

        Debug.Log("⚠️ Attack cancelled!");
    }

    void Patrol()
    {
        if (player == null) return;

        float distToPlayer = Vector2.Distance(transform.position, player.position);
        float distToStart = Mathf.Abs(transform.position.x - startPos.x);

        // CASE 1: GẦN PLAYER - Dừng lại, quay mặt
        if (distToPlayer <= attackRange)
        {
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
            if (anim != null) anim.SetBool("isWalking", false);

            int faceDir = (player.position.x > transform.position.x) ? 1 : -1;
            if (faceDir != direction)
            {
                direction = faceDir;
                Flip();
            }
        }
        // CASE 2: CHASE - Đuổi theo Player
        else if (distToPlayer <= chaseRange)
        {
            if (anim != null) anim.SetBool("isWalking", true);

            int chaseDir = (player.position.x > transform.position.x) ? 1 : -1;

            if (chaseDir != direction)
            {
                direction = chaseDir;
                Flip();
            }

            rb.linearVelocity = new Vector2(direction * moveSpeed * 1.5f, rb.linearVelocity.y);
        }
        // CASE 3: PATROL - Tuần tra
        else
        {
            if (anim != null) anim.SetBool("isWalking", true);

            if (distToStart > patrolDistance)
            {
                int returnDir = (startPos.x > transform.position.x) ? 1 : -1;

                if (returnDir != direction)
                {
                    direction = returnDir;
                    Flip();
                }
            }

            rb.linearVelocity = new Vector2(direction * moveSpeed, rb.linearVelocity.y);

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
        Vector3 scale = transform.localScale;
        scale.x = originalScaleX * direction;
        transform.localScale = scale;
    }

    void CheckAttack()
    {
        if (player == null || isAttacking) return;

        float distToPlayer = Vector2.Distance(transform.position, player.position);

        // CHỈ ATTACK khi trong chaseRange
        if (distToPlayer <= attackRange && distToPlayer <= chaseRange && Time.time >= lastAttackTime + attackCooldown)
        {
            isAttacking = true;
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);

            if (anim != null)
            {
                anim.SetBool("isWalking", false);
                anim.ResetTrigger("Attack");
                anim.SetTrigger("Attack");
            }

            // ✅ Schedule damage với delay có thể customize
            Invoke("DealDamageToPlayer", attackDamageDelay);
            Invoke("EndAttack", attackDuration);

            lastAttackTime = Time.time;
            Debug.Log("🗡️ ENEMY ATTACK TRIGGERED!");
        }
    }

    // ✅ IMPROVED: Thêm nhiều debug log hơn
    void DealDamageToPlayer()
    {
        if (player == null)
        {
            Debug.LogError("❌ Player is NULL!");
            return;
        }

        float distToPlayer = Vector2.Distance(transform.position, player.position);

        Debug.Log($"🗡️ Enemy trying to deal damage! Distance: {distToPlayer:F2}, AttackRange: {attackRange}");

        if (distToPlayer <= attackRange)
        {
            PlayerController playerCtrl = player.GetComponent<PlayerController>();
            if (playerCtrl != null)
            {
                playerCtrl.TakeDamage(attackDamage);
                Debug.Log($"💥 Enemy HIT Player! Dealt {attackDamage} damage!");
            }
            else
            {
                Debug.LogError("❌ Player doesn't have PlayerController component!");
            }
        }
        else
        {
            Debug.LogWarning($"⚠️ Player too far to hit! Distance: {distToPlayer:F2} > AttackRange: {attackRange}");
        }
    }

    void EndAttack()
    {
        isAttacking = false;
        Debug.Log("✅ Attack animation finished");
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

    // ✅ Getter cho UI
    public int GetCurrentHealth()
    {
        return currentHealth;
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