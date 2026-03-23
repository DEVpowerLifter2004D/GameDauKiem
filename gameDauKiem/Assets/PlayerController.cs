using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f;
    public float jumpForce = 400f;

    [Header("Check Ground")]
    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;

    [Header("Air Control")]
    public float airControlMultiplier = 0.5f;

    [Header("Health")]
    public int maxHealth = 5;
    private int currentHealth;

    [Header("Fall Death")]
    public float fallDeathY = -10f;

    [Header("Attack")]
    public Transform attackPoint;
    public float attackRadius = 0.5f;
    public int attackDamage = 5;
    public float attackDelay = 0.3f;

    private LayerMask groundLayer;
    private LayerMask enemyLayer;
    private Rigidbody2D rb;
    private Animator anim;
    private SpriteRenderer sr;
    private float moveInput;
    private bool isGrounded;
    private bool isAttacking = false;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        sr = GetComponent<SpriteRenderer>();
    }

    void Start()
    {
        currentHealth = maxHealth;

        // ✅ Setup Ground Layer - CHỈ DETECT Ground, KHÔNG detect Player
        groundLayer = LayerMask.GetMask("Ground");
        enemyLayer = LayerMask.GetMask("Enemy");

      
        // ✅ Kiểm tra Collider của Player
        Collider2D playerCol = GetComponent<Collider2D>();
      
    }

    void Update()
    {
        // 💀 Rơi khỏi map
        if (transform.position.y < fallDeathY)
        {
            currentHealth = 0;
            FindFirstObjectByType<GameManager>()?.PlayerDied();
            return;
        }

        // 🎮 Input
        moveInput = 0f;
        if (Keyboard.current.aKey.isPressed) moveInput = -1f;
        if (Keyboard.current.dKey.isPressed) moveInput = 1f;

        Flip(moveInput);

        // 🟢 Check ground
        // 🟢 Check ground
        // 🟢 Check ground
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

        // 🔍 DEBUG FULL
        Collider2D[] allHits = Physics2D.OverlapCircleAll(groundCheck.position, groundCheckRadius);

        foreach (Collider2D hit in allHits)
        {
            string layerName = LayerMask.LayerToName(hit.gameObject.layer);
        }

       
        // 🔍 DEBUG
        if (!isGrounded && rb.linearVelocity.y == 0)
        {

            // Kiểm tra xem có collider nào ở dưới không
            Collider2D hit = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius);
            if (hit != null)
            {
            }
        }
        // 🦘 Jump
        if (Keyboard.current.spaceKey.wasPressedThisFrame && isGrounded)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);
            rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
            anim.SetTrigger("Jump");
        }

        // ⚔️ Attack
        if (Mouse.current.leftButton.wasPressedThisFrame && !isAttacking)
        {
            isAttacking = true;
            anim.SetTrigger("Attack");
            StartCoroutine(AttackSequence());
        }

        // 🎞 Animation
        anim.SetFloat("Speed", Mathf.Abs(moveInput));
        anim.SetFloat("YVelocity", rb.linearVelocity.y);
        anim.SetBool("Grounded", isGrounded);
    }

    void FixedUpdate()
    {
        float targetSpeed = moveInput * moveSpeed;

        if (isGrounded)
        {
            rb.linearVelocity = new Vector2(targetSpeed, rb.linearVelocity.y);
        }
        else
        {
            float newX = Mathf.Lerp(rb.linearVelocity.x, targetSpeed, airControlMultiplier);
            rb.linearVelocity = new Vector2(newX, rb.linearVelocity.y);
        }
    }

    void Flip(float moveX)
    {
        if (moveX == 0) return;

        // Lấy scale hiện tại và giữ nguyên giá trị tuyệt đối
        Vector3 scale = transform.localScale;
        float absX = Mathf.Abs(scale.x);
        scale.x = absX * (moveX > 0 ? 1 : -1);
        transform.localScale = scale;
    }

    // ❤️ DAMAGE
    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        if (currentHealth <= 0)
        {
            FindFirstObjectByType<GameManager>()?.PlayerDied();
        }
    }

    public int GetCurrentHealth()
    {
        return currentHealth;
    }

    public bool Heal(int amount)
    {
        if (amount <= 0) return false;

        int before = currentHealth;
        currentHealth = Mathf.Clamp(currentHealth + amount, 0, maxHealth);
        return currentHealth > before;
    }

    // ⚔️ DEAL DAMAGE (FULL SUPPORT)
    public void DealDamage()
    {

        Collider2D[] hits = Physics2D.OverlapCircleAll(
            attackPoint.position,
            attackRadius,
            enemyLayer
        );

        foreach (Collider2D hit in hits)
        {
            // 🔥 THÊM ĐOẠN NÀY
            BossFire bossFire = hit.GetComponent<BossFire>();
            if (bossFire != null)
            {
                Debug.Log("🔥 HIT BOSS FIRE"); // test
                bossFire.TakeDamage(attackDamage);
                continue;
            }

            // ✅ Boss static (ArchDemon)
            BossStaticController bossStatic = hit.GetComponent<BossStaticController>();
            if (bossStatic != null)
            {
                bossStatic.TakeDamage(attackDamage);
                continue;
            }

            // ✅ Boss thường
            BossController boss = hit.GetComponent<BossController>();
            if (boss != null)
            {
                boss.TakeDamage(attackDamage);
                continue;
            }

            // ✅ Enemy cũ
            EnemyController oldEnemy = hit.GetComponent<EnemyController>();
            if (oldEnemy != null)
            {
                oldEnemy.TakeDamage(attackDamage);
                continue;
            }

            // ✅ Enemy mới (EnemyHealth)
            EnemyHealth newEnemy = hit.GetComponent<EnemyHealth>();
            if (newEnemy != null)
            {
                newEnemy.TakeDamage(attackDamage);
            }
        }
    }

    // ⏱ Attack timing
    private IEnumerator AttackSequence()
    {
        yield return new WaitForSeconds(attackDelay);
        DealDamage();

        yield return new WaitForSeconds(0.6f - attackDelay);
        isAttacking = false;
    }

    public void OnAttackFinished()
    {
        isAttacking = false;
    }

    void OnDrawGizmos()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }

        if (attackPoint != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(attackPoint.position, attackRadius);
        }
    }
}