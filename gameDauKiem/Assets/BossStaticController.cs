using UnityEngine;

public class BossStaticController : MonoBehaviour
{
    [Header("Stats")]
    public int maxHealth = 10;
    private int currentHealth;

    [Header("Combat")]
    public float attackRange = 3f;
    public int attackDamage = 1;
    public float attackCooldown = 2f;
    public float attackDuration = 1f;
    public float attackDamageDelay = 0.5f;

    [Header("Sprite Flip")]
    public bool spriteDefaultFacingLeft = true;

    private Transform player;
    private Animator anim;
    private float lastAttackTime;
    private bool isAttacking = false;
    private float originalScaleX;
    private bool isDead = false;  // ✅ THÊM FLAG

    void Start()
    {
        currentHealth = maxHealth;
        anim = GetComponent<Animator>();
        originalScaleX = Mathf.Abs(transform.localScale.x);

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
        }

        GameManager gm = FindFirstObjectByType<GameManager>();
        if (gm != null)
        {
            gm.OnEnemySpawned();
        }
    }

    void Update()
    {
        if (player == null || isDead) return;  // ✅ CHECK isDead

        FacePlayer();
        CheckAttack();
    }

    void FacePlayer()
    {
        if (player == null) return;

        int direction = (player.position.x > transform.position.x) ? 1 : -1;

        Vector3 scale = transform.localScale;

        if (spriteDefaultFacingLeft)
        {
            scale.x = -direction * originalScaleX;
        }
        else
        {
            scale.x = direction * originalScaleX;
        }

        transform.localScale = scale;
    }

    void CheckAttack()
    {
        if (isAttacking) return;

        float distToPlayer = Vector2.Distance(transform.position, player.position);

        if (distToPlayer <= attackRange && Time.time >= lastAttackTime + attackCooldown)
        {
            isAttacking = true;

            if (anim != null)
            {
                anim.SetTrigger("Attack");
            }

            Invoke("DealDamageToPlayer", attackDamageDelay);
            Invoke("EndAttack", attackDuration);

            lastAttackTime = Time.time;
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
                Debug.Log($"💥 Boss hit Player! Dealt {attackDamage} damage!");
            }
        }
    }

    void EndAttack()
    {
        isAttacking = false;
    }

    public void TakeDamage(int damage)
    {
        // ✅ NẾU ĐÃ CHẾT → RETURN NGAY
        if (isDead) return;

        currentHealth -= damage;
        Debug.Log($"💔 Boss HP: {currentHealth}/{maxHealth}");

        if (currentHealth > 0)
        {
            // ✅ Trigger Hurt chỉ khi còn sống
            if (anim != null)
            {
                anim.SetTrigger("Hurt");
            }
        }
        else
        {
            // ✅ Chết → Gọi Die() 1 LẦN DUY NHẤT
            Die();
        }
    }

    void Die()
    {
        // ✅ NẾU ĐÃ CHẾT RỒI → RETURN (tránh gọi nhiều lần)
        if (isDead) return;

        isDead = true;  // ✅ SET FLAG NGAY

        Debug.Log("👑 Boss Defeated!");

        // Tắt script
        this.enabled = false;

        // Tắt collider NGAY LẬP TỨC
        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false;

        // Tắt Rigidbody để không bị rơi
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.bodyType = RigidbodyType2D.Kinematic;
        }

        // Play animation Death
        if (anim != null)
        {
            anim.SetTrigger("Death");
            Debug.Log("💀 Playing Death animation!");
        }

        // Thông báo GameManager
        GameManager gm = FindFirstObjectByType<GameManager>();
        if (gm != null)
        {
            gm.OnEnemyDied();
        }

        // Destroy sau khi animation chạy xong
        Destroy(gameObject, 2f);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}