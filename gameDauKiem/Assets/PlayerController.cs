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
    public float fallDeathY = -10f;  // Rơi xuống dưới -10 thì chết

    [Header("Attack")]
    public Transform attackPoint;
    public float attackRadius = 0.5f;
    public int attackDamage = 1;
    public float attackDelay = 0.3f;  // Thời gian đến khi damage được deal

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

        groundLayer = LayerMask.GetMask("Ground");
        enemyLayer = LayerMask.GetMask("Enemy");
        Debug.Log($"✅ Player Ground Layer: {groundLayer.value}, Enemy Layer: {enemyLayer.value}");
    }

    void Update()
    {
        // Kiểm tra rơi ra khỏi map → Chết (trừ hết máu)
        if (transform.position.y < fallDeathY)
        {
            Debug.Log("💀 Player fell off map! INSTANT DEATH!");
            currentHealth = 0;
            FindFirstObjectByType<GameManager>()?.PlayerDied();
            return;
        }

        moveInput = 0f;
        if (Keyboard.current.aKey.isPressed) moveInput = -1f;
        if (Keyboard.current.dKey.isPressed) moveInput = 1f;
        Flip(moveInput);

        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

        if (Keyboard.current.spaceKey.wasPressedThisFrame && isGrounded)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);
            rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
            anim.SetTrigger("Jump");
            Debug.Log($"🔥 JUMP! Velocity Y: {rb.linearVelocity.y}");
        }

        // ✅ FIX: Gọi AttackSequence thay vì chỉ set animation
        if (Mouse.current.leftButton.wasPressedThisFrame && !isAttacking)
        {
            isAttacking = true;
            anim.SetTrigger("Attack");
            StartCoroutine(AttackSequence());
        }

        anim.SetFloat("Speed", Mathf.Abs(moveInput));
        anim.SetFloat("YVelocity", rb.linearVelocity.y);
        anim.SetBool("Grounded", isGrounded);

        //Vector3 pos = transform.position;
        //pos.x = Mathf.Clamp(pos.x, -8f, 8f); // chỉnh -8 và 8 theo map của bạn
        //transform.position = pos;

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
            float currentX = rb.linearVelocity.x;
            float newX = Mathf.Lerp(currentX, targetSpeed, airControlMultiplier);
            rb.linearVelocity = new Vector2(newX, rb.linearVelocity.y);
        }
    }

    void Flip(float moveX)
    {
        if (moveX == 0) return;
        transform.localScale = new Vector3(moveX > 0 ? 1 : -1, 1, 1);
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        if (currentHealth <= 0)
        {
            FindFirstObjectByType<GameManager>()?.PlayerDied();
        }
    }

    // ✅ Getter cho UI
    public int GetCurrentHealth()
    {
        return currentHealth;
    }

    // ✅ Hồi máu, clamp không vượt quá maxHealth
    public bool Heal(int amount)
    {
        if (amount <= 0) return false;

        int before = currentHealth;
        currentHealth = Mathf.Clamp(currentHealth + amount, 0, maxHealth);
        return currentHealth > before;
    }

    // ✅ ĐƯỢC GỌI TỪ AttackSequence hoặc Animation Event
    public void DealDamage()
    {
        Debug.Log("🗡️ Player DealDamage() called!");
        Debug.Log($"🔍 AttackPoint position: {attackPoint.position}");
        Debug.Log($"🔍 Attack radius: {attackRadius}");
        Debug.Log($"🔍 Enemy Layer mask: {enemyLayer.value}");

        Collider2D[] hits = Physics2D.OverlapCircleAll(attackPoint.position, attackRadius, enemyLayer);

        Debug.Log($"🔍 Found {hits.Length} enemies in attack range");

        foreach (Collider2D hit in hits)
        {
            Debug.Log($"🎯 Hit object: {hit.gameObject.name}, Layer: {LayerMask.LayerToName(hit.gameObject.layer)}");

            // ✅ TRY: Detect BossController thay vì EnemyController
            BossController boss = hit.GetComponent<BossController>();
            if (boss != null)
            {
                boss.TakeDamage(attackDamage);
                Debug.Log($"💥 Player HIT BOSS! Dealt {attackDamage} damage!");
                continue;
            }

            EnemyController enemy = hit.GetComponent<EnemyController>();
            if (enemy != null)
            {
                enemy.TakeDamage(attackDamage);
                Debug.Log($"💥 Player HIT {enemy.gameObject.name}! Dealt {attackDamage} damage!");
            }
        }
    }

    // ✅ NEW: Sequence attack có delay
    private IEnumerator AttackSequence()
    {
        // Chờ đến khi tay vung đến (giữa animation)
        yield return new WaitForSeconds(attackDelay);

        // Gây damage
        DealDamage();

        // Chờ animation kết thúc
        yield return new WaitForSeconds(0.6f - attackDelay);

        isAttacking = false;
    }

    // ✅ Có thể gọi từ Animation Event nếu dùng phương pháp 1
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
