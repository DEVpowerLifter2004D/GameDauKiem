using UnityEngine;
using UnityEngine.UI;
public class EnemyController : MonoBehaviour
{
    [Header("Stats")]
    public int maxHealth = 3;
    private int currentHealth;

    [Header("Health Bar UI")]
    public Image healthFill;           // Kéo HealthFill vào đây
    public float maxHealthFloat = 100f; // Số máu hiển thị trên thanh (thường để 100)
    private float currentHealthFloat;

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

    [Header("Hit Animation")]
    public float hitAnimationDuration = 0.3f;
    public float groundCheckRadius = 0.5f;
    public bool spriteDefaultFacingLeft = true; // ✅ THÊM: Tích vào đây nếu quái mặc định nhìn trái

    private Transform groundCheck;
    private LayerMask groundLayer;
    private float lastAttackTime;
    private Transform player;
    private Vector3 startPos;
    private int direction = 1;
    private float originalScaleX;
    protected Animator anim;
    private Rigidbody2D rb;
    private bool isGrounded;
    private bool isAttacking = false;
    private bool isHit = false;

    protected virtual void OnMoveStateChanged(bool isMoving)
    {
        if (anim != null) anim.SetBool("isWalking", isMoving);
    }

    protected virtual void OnAttackStarted()
    {
        if (anim == null) return;
        anim.SetBool("isWalking", false);
        anim.ResetTrigger("Attack");
        anim.SetTrigger("Attack");
    }

    protected virtual void OnAttackCancelled()
    {
        if (anim != null) anim.ResetTrigger("Attack");
    }

    protected virtual void OnAttackEnded()
    {
        // Default: trigger-based attack has nothing to reset at end.
    }

    void Start()
    {
        currentHealth = maxHealth;
        startPos = transform.position;
        originalScaleX = Mathf.Abs(transform.localScale.x);
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();




        currentHealthFloat = maxHealthFloat;
        if (healthFill != null)
            healthFill.fillAmount = 1f;


        if (healthFill == null)
            Debug.LogError("❌ healthFill chưa được assign trên " + gameObject.name);
        else
            Debug.Log("✅ healthFill OK: " + healthFill.name);
        // ✅ THAY ĐỔI: Gọi GameManager thay vì EnemySpawner
        GameManager gm = FindFirstObjectByType<GameManager>();
        if (gm != null)
        {
            gm.OnEnemySpawned();
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
        }

    }
    void Die()
    {
        // Trigger death animation
        if (anim != null)
        {
            anim.SetTrigger("Death");
        }

        // Stop movement
        rb.linearVelocity = Vector2.zero;
        rb.bodyType = RigidbodyType2D.Static;

        // Disable collider
        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
            col.enabled = false;

        // ✅ RƠI BÌNH MÁU
        EnemyDropLoot dropLoot = GetComponent<EnemyDropLoot>();
        if (dropLoot != null)
        {
            dropLoot.DropLoot();
        }

        // Thông báo GameManager
        GameManager gm = FindFirstObjectByType<GameManager>();
        if (gm != null)
        {
            gm.OnEnemyDied();
        }

        // Destroy sau 1 giây
        Destroy(gameObject, 1f);
    }
    void FixedUpdate()
    {
        if (player == null)
        {
            Debug.LogWarning("[ENEMY] Player is NULL!");
            return;
        }

        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

        // Kiểm tra Rigidbody constraints
        if (rb.constraints != RigidbodyConstraints2D.None && rb.constraints != RigidbodyConstraints2D.FreezeRotation)
        {
            Debug.LogWarning($"[ENEMY] Rigidbody constraints: {rb.constraints} - This may prevent movement!");
        }

        // Cancel attack nếu Player xa quá
        if (isAttacking)
        {
            float distToPlayer = Vector2.Distance(transform.position, player.position);
            if (distToPlayer > attackRange * 2f)
            {
                CancelAttack();
            }
        }

        // Di chuyển khi không đang attack và không bị đánh (cho phép di chuyển trên không)
        if (!isAttacking && !isHit)
        {
            Patrol();
        }
        else if (isAttacking)
        {
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
        }

        CheckAttack();
    }

    // ✅ DÙNG LATEUPDATE để ghi đè lên Animation (Animation thường đè Scale trong Update)
    void LateUpdate()
    {
        if (player == null) return;

        float distToPlayer = Vector2.Distance(transform.position, player.position);

        if (distToPlayer <= chaseRange)
        {
            direction = (player.position.x > transform.position.x) ? 1 : -1;
        }

        Vector3 scale = transform.localScale;
        scale.x = originalScaleX * direction;
        transform.localScale = scale;
    }
    void CancelAttack()
    {
        isAttacking = false;
        OnAttackEnded();
        CancelInvoke("DealDamageToPlayer");
        CancelInvoke("EndAttack");

        OnAttackCancelled();

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
            OnMoveStateChanged(false);

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
            OnMoveStateChanged(true);

            int chaseDir = (player.position.x > transform.position.x) ? 1 : -1;
            direction = chaseDir;

            rb.linearVelocity = new Vector2(direction * moveSpeed * 1.5f, rb.linearVelocity.y);
        }
        // CASE 3: PATROL - Tuần tra
        else
        {
            OnMoveStateChanged(true);

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
        }
    }

    // Hàm Flip cũ có thể xóa hoặc để trống vì LateUpdate đã lo việc này
    void Flip() { }

    void CheckAttack()
    {
        if (player == null || isAttacking) return;

        float distToPlayer = Vector2.Distance(transform.position, player.position);

        // CHỈ ATTACK khi trong chaseRange và không đang bị đánh
        if (distToPlayer <= attackRange && distToPlayer <= chaseRange && Time.time >= lastAttackTime + attackCooldown && !isHit)
        {
            isAttacking = true;
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);

            OnAttackStarted();

            // ✅ Schedule damage với delay có thể customize
            Invoke("DealDamageToPlayer", attackDamageDelay);
            Invoke("EndAttack", attackDuration);

            lastAttackTime = Time.time;
        }
    }

    void DealDamageToPlayer()
    {
        if (player == null)
        {
            return;
        }

        float distToPlayer = Vector2.Distance(transform.position, player.position);


        if (distToPlayer <= attackRange)
        {
            PlayerController playerCtrl = player.GetComponent<PlayerController>();
            if (playerCtrl != null)
            {
                playerCtrl.TakeDamage(attackDamage);
            }

        }
        else
        {
        }
    }

    void EndAttack()
    {
        isAttacking = false;
        OnAttackEnded();
        Debug.Log("✅ Attack ended");
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        currentHealthFloat -= damage;                    // ← thêm dòng này

        Debug.Log($"💔 Enemy HP: {currentHealth}/{maxHealth}");

        // ==================== THÊM PHẦN NÀY ====================
        UpdateHealthBar();


        isHit = true;
        CancelInvoke(nameof(ResetHit));
        Invoke(nameof(ResetHit), hitAnimationDuration);

        OnTakeDamage();

        if (currentHealth <= 0)
        {
            Die();
        }
    }
    // ==================== THÊM HÀM MỚI NÀY Ở CUỐI CLASS ====================
    private void UpdateHealthBar()
    {
        if (healthFill != null)
        {
            // Dùng currentHealth/maxHealth thay vì float
            healthFill.fillAmount = Mathf.Clamp01((float)currentHealth / (float)maxHealth);
            Debug.Log($"Set fillAmount = {healthFill.fillAmount}");
        }
    }
    // =====================================================================
    void ResetHit()
    {
        isHit = false;
    }

    protected virtual void OnTakeDamage()
    {
        if (anim != null)
        {
            anim.SetTrigger("Hurt");
        }
    }

    // ✅ Getter cho UI
    public int GetCurrentHealth()
    {
        return currentHealth;
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
