using UnityEngine;

public class WallHealth : MonoBehaviour
{
    public int maxHealth = 3;

    private int currentHealth;
    void Start()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
       

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        GetComponent<Collider2D>().enabled = false;

        Destroy(gameObject);
    }
}
