using UnityEngine;
using System;

public class EnemyHealth : MonoBehaviour
{
    [Header("Salud")]
    public int maxHealth = 80;
    int currentHealth;

    [Header("Efectos")]
    public GameObject deathVFXPrefab;
    public GameObject hitVFXPrefab;

    [HideInInspector]
    public Func<int, bool> onDamageReceived;

    bool isDead = false;
    Animator anim;

    void Start()
    {
        currentHealth = maxHealth;
        anim = GetComponent<Animator>();
    }

    public void TakeDamage(int damage)
    {
        if (isDead) return;

        if (onDamageReceived != null && !onDamageReceived(damage))
            return;

        currentHealth -= damage;

        if (hitVFXPrefab)
            Instantiate(hitVFXPrefab, transform.position, Quaternion.identity);

        anim?.SetTrigger("Hit");

        if (currentHealth <= 0)
        {
            var falange = GetComponent<FalangeExecutor>();
            if (falange != null) falange.Die();
            else Die();
        }
    }

    public void Die()
    {
        if (isDead) return;
        isDead = true;
        anim?.SetTrigger("Die");
        GetComponent<Collider2D>().enabled = false;

        if (deathVFXPrefab)
            Instantiate(deathVFXPrefab, transform.position, Quaternion.identity);

        // Notificar UI y GameManager
        FindFirstObjectByType<UIAutoSetup>()?.AddKill();
        GameManager.Instance?.EnemyKilled();
        Debug.Log("EnemyHealth: Die() llamado — notificando GameManager");

        Destroy(gameObject, 1.5f);
    }

    public float HealthPercent => (float)currentHealth / maxHealth;
}
