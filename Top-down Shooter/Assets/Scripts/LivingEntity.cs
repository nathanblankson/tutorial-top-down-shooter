using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LivingEntity : MonoBehaviour, IDamageable
{
    public float startingHealth = 10f;
    protected float health;
    protected bool isDead = false;

    public event System.Action OnDeath;

    protected virtual void Start()
    {
        health = startingHealth;
    }

    public void TakeHit(float damage, RaycastHit hit)
    {
        // TODO: use hit
        TakeDamage(damage);
    }

    public void TakeDamage(float damage)
    {
        health -= damage;

        if (health <= 0 && !isDead)
        {
            Die();
        }
    }

    [ContextMenu("Die")]
    protected void Die()
    {
        isDead = true;
        if (OnDeath != null)
        {
            OnDeath();
        }
        Destroy(gameObject);
    }
}
