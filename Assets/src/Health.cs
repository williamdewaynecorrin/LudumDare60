using UnityEngine;
using UnityEngine.Events;

public class Health : MonoBehaviour
{
    public int maxhealth;
    public UnityEvent ondeath;
    private int health;

    public int CurrentHealth => health;

    void Awake()
    {
        health = maxhealth;
    }

    public void TakeDamage(int damage)
    {
        if (health <= 0)
            return;

        health -= damage;
        if(health <= 0)
        {
            Die();
        }
    }

    public void Die()
    {
        ondeath?.Invoke();
    }
}
