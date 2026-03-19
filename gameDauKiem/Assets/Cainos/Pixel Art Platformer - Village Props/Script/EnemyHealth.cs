using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    public int maxHealth = 3; // Ong có 3 giọt máu, chém 3 phát mới chết
    private int currentHealth;

    void Start()
    {
        currentHealth = maxHealth; // Vừa sinh ra là đầy máu
    }

    // Hàm này sẽ được gọi khi nhân vật vung kiếm trúng con ong
    public void TakeDamage(int damage)
    {
        currentHealth -= damage; // Trừ máu

        // In ra dòng chữ ở dưới cùng màn hình (Console) để bạn dễ kiểm tra
        Debug.Log("Ong bị chém! Máu còn: " + currentHealth);

        // Nếu máu tụt xuống 0 hoặc âm thì gọi hàm Chết
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        Debug.Log("Ong đã bị tiêu diệt!");

        // Biến con ong thành cát bụi (Xóa nó khỏi game)
        Destroy(gameObject);
    }
}