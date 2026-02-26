using UnityEngine;

public class EnemyController : MonoBehaviour
{
    public int maxHealth = 3;
    private int currentHealth;
    public float moveSpeed = 2f;
    public float patrolDistance = 3f;

    public float attackRange = 1.5f;  // Khoảng cách attack
    public int attackDamage = 1;
    public float attackCooldown = 1.5f;
    private float lastAttackTime;

    private Transform player;
    private Vector3 startPos;
    private int direction = 1;
    private Animator anim;
    private Rigidbody2D rb;

    void Start()
    {
        currentHealth = maxHealth;
        startPos = transform.position;
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        player = GameObject.FindWithTag("Player").transform;  // Tag "Player" cho Player GO
    }

    void FixedUpdate()
    {
        Patrol();
        CheckAttack();
    }

    void Patrol()
    {
        float distToPlayer = Vector2.Distance(transform.position, player.position);

        if (distToPlayer > attackRange)
        {
            // 🔥 PATROL XA PLAYER: Đi tới lui mượt
            anim.SetBool("isWalking", true);
            rb.linearVelocity = new Vector2(direction * moveSpeed, rb.linearVelocity.y);  // 🔥 FIX: velocity (không linear)

            // 🔥 CHỈ FLIP KHI ĐỦ XA START POS (tránh conflict)
            if (Mathf.Abs(transform.position.x - startPos.x) >= patrolDistance)
            {
                direction *= -1;
                Flip();
            }
        }
        else
        {
            // 🔥 GẦN PLAYER: DỪNG MƯỢT, FACE PLAYER
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);  // Dừng X
            anim.SetBool("isWalking", false);

            // Face player luôn (không teleport)
            direction = (player.position.x > transform.position.x) ? 1 : -1;
            Flip();
        }
    }

    void Flip()
    {
        transform.localScale = new Vector3(direction, 1, 1);
    }

    void CheckAttack()
    {
        float distToPlayer = Vector2.Distance(transform.position, player.position);
        if (distToPlayer <= attackRange && Time.time >= lastAttackTime + attackCooldown)
        {
            // 🔥 TRIGGER ATTACK (sẽ setup anim sau)
            if (anim != null) anim.SetTrigger("Attack");

            // Damage player
            PlayerController playerCtrl = player.GetComponent<PlayerController>();
            if (playerCtrl != null)
            {
                playerCtrl.TakeDamage(attackDamage);
            }

            lastAttackTime = Time.time;
            Debug.Log("Enemy ATTACK!");
        }
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        Debug.Log("Enemy HP: " + currentHealth);
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        Destroy(gameObject);
    }
}