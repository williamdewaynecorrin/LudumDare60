using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class Health : MonoBehaviour
{
    public int maxhealth;
    public Image attachedfill;
    public bool hasdamagecooldown = true;
    public float damagecooldowntime = 0.5f;
    public UnityEvent ondeath;

    private int health;
    private float damagecooldowntimer = 0f;

    public int CurrentHealth => health;
    public bool IsDead => health <= 0f;
    public float HealthRatio() => Mathf.Clamp01((float)health / (float)maxhealth);
    public bool CanBeDamaged => !hasdamagecooldown || damagecooldowntimer <= 0f;

    void Awake()
    {
        health = maxhealth;

        if (attachedfill != null)
            attachedfill.fillAmount = HealthRatio();

        damagecooldowntimer = 0f;
    }

    void Update()
    {
        if(damagecooldowntimer > 0f)
        {
            damagecooldowntimer = Mathf.Max(0f, damagecooldowntimer - Time.deltaTime);
        }
    }

    public void TakeDamage(int damage)
    {
        if (health <= 0 || (hasdamagecooldown && damagecooldowntimer > 0f))
            return;

        health -= damage;
        if(health <= 0)
        {
            Die();
        }

        if (attachedfill != null)
            attachedfill.fillAmount = HealthRatio();

        if (hasdamagecooldown)
            damagecooldowntimer = damagecooldowntime;
    }

    public void Die()
    {
        ondeath?.Invoke();
    }
}
