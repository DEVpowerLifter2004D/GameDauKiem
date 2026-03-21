using UnityEngine;

public class BossController : MonoBehaviour
{
    [Header("Boss Stats")]
    public int maxHealth = 10;
    private int currentHealth;

    [Header("Movement")]
    public float moveSpeed = 1.5f;
    public float chaseRange = 8f;

    [Header("Combat")]
    public float attackRange = 4f;
    public int attackDamage = 2;
    public float attackCooldown = 2f;
    public float attackDuration = 1.2f;
    public float attackDamageDelay = 0.5f;

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
    private bool isDead = false;

    void Start()
    {
        currentHealth = maxHealth;
        startPos = transform.position;

        // ✅ FIX: Invert originalScaleX nếu Boss đi ngược
        originalScaleX = -Mathf.Abs(transform.localScale.x);

        Debug.Log($"🎯 Boss originalScaleX: {originalScaleX}, Initial Scale: {transform.localScale}");

        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();

        if (anim != null && anim.runtimeAnimatorController == null)
        {
            Debug.LogWarning("⚠️ Boss không có Animator Controller!");
            anim.enabled = false;
            anim = null;
        }

        if (rb == null)
        {
            Debug.LogError("❌ Boss thiếu Rigidbody2D!");
        }

        // ✅ CHECK LAYER
        string layerName = LayerMask.LayerToName(gameObject.layer);
        Debug.Log($"👑 BOSS Layer: {layerName}");

        if (gameObject.layer != LayerMask.NameToLayer("Enemy"))
        {
            Debug.LogError($"❌ CRITICAL: Boss Layer = '{layerName}', PHẢI LÀ 'Enemy'!");
        }

        groundCheck = transform.Find("GroundCheck");
        if (groundCheck == null)
        {
            GameObject checkObj = new GameObject("GroundCheck");
            checkObj.transform.parent = transform;
            checkObj.transform.localPosition = new Vector3(0, -0.6f, 0);
            groundCheck = checkObj.transform;
        }

        groundLayer = LayerMask.GetMask("Ground");

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
            Debug.Log("✅ Boss found Player!");
        }
        else
        {
            Debug.LogError("❌ Boss không tìm thấy Player!");
        }
    }

    void FixedUpdate()
    {
        if (isDead || player == null || rb == null) return;

        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

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
            ChasePlayer();
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

        if (anim != null && anim.enabled)
        {
            anim.ResetTrigger("Attack");
        }
    }

    void ChasePlayer()
    {
        if (player == null) return;

        float distToPlayer = Vector2.Distance(transform.position, player.position);

        if (distToPlayer <= chaseRange)
        {
            if (distToPlayer > attackRange)
            {
                if (anim != null && anim.enabled)
                    anim.SetBool("isWalking", true);

                int chaseDir = (player.position.x > transform.position.x) ? 1 : -1;

                if (chaseDir != direction)
                {
                    direction = chaseDir;
                    Flip();
                }

                rb.linearVelocity = new Vector2(direction * moveSpeed, rb.linearVelocity.y);

                Debug.Log($"🏃 Chase! Dir={direction}, Vel={rb.linearVelocity.x}, Scale={transform.localScale.x}");
            }
            else
            {
                rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
                if (anim != null && anim.enabled)
                    anim.SetBool("isWalking", false);

                int faceDir = (player.position.x > transform.position.x) ? 1 : -1;

                if (faceDir != direction)
                {
                    direction = faceDir;
                    Flip();
                }
            }
        }
        else
        {
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
            if (anim != null && anim.enabled)
                anim.SetBool("isWalking", false);
        }
    }

    void Flip()
    {
        Vector3 scale = transform.localScale;
        scale.x = originalScaleX * direction;
        transform.localScale = scale;

        Debug.Log($"🔄 Flipped! Dir={direction}, Scale.x={scale.x}");
    }

    void CheckAttack()
    {
        if (player == null || isAttacking) return;

        float distToPlayer = Vector2.Distance(transform.position, player.position);

        if (distToPlayer <= attackRange && Time.time >= lastAttackTime + attackCooldown)
        {
            isAttacking = true;
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);

            if (anim != null && anim.enabled)
            {
                anim.SetBool("isWalking", false);
                anim.ResetTrigger("Attack");
                anim.SetTrigger("Attack");
            }

            Invoke("DealDamageToPlayer", attackDamageDelay);
            Invoke("EndAttack", attackDuration);

            lastAttackTime = Time.time;
            Debug.Log("👑 BOSS ATTACK!");
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
                Debug.Log($"💥 BOSS HIT Player! Dealt {attackDamage} damage!");
            }
        }
    }

    void EndAttack()
    {
        isAttacking = false;
    }

    public void TakeDamage(int damage)
    {
        if (isDead) return;

        currentHealth -= damage;
        Debug.Log($"👑 Boss took {damage} damage! HP: {currentHealth}/{maxHealth}");

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    public int GetCurrentHealth()
    {
        return currentHealth;
    }

    public int GetMaxHealth()
    {
        return maxHealth;
    }

    void Die()
    {
        if (isDead) return;
        isDead = true;

        Debug.Log("💀 BOSS DEFEATED!");

        // Animation
        if (anim != null && anim.enabled)
        {
            anim.SetTrigger("Die");
        }

        // Stop
        if (rb != null)
            rb.linearVelocity = Vector2.zero;

        // Collider
        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
            col.enabled = false;

        // ✅ RƠI BÌNH MÁU (boss rơi nhiều hơn!)
        EnemyDropLoot dropLoot = GetComponent<EnemyDropLoot>();
        if (dropLoot != null)
        {
            dropLoot.DropLoot();
        }

        Destroy(gameObject, 2f);
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

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, chaseRange);
    }
}