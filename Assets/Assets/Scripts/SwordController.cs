using UnityEngine;

/// <summary>
/// SWORDCONTROLLER - Sistema de combate con espada
/// </summary>

public class SwordController : MonoBehaviour
{
    [Header("Ataque")]
    public float attackRange = 1.5f;
    public float attackDamage = 10f;
    public int maxCombo = 3;

    [Header("Tiempo")]
    public float attackCooldown = 0.5f;
    public float comboResetTime = 1.5f;
    public float attackDuration = 0.3f;

    [Header("Energía")]
    public float energyRegenPerHit = 20f;

    [Header("Colisión")]
    public LayerMask enemyLayer;
    public Transform attackPoint;

    Animator anim;
    DaggerController daggerCtrl;

    float lastAttackTime = 0f;
    float comboResetTimer = 0f;
    int currentCombo = 0;

    void Start()
    {
        anim = GetComponent<Animator>();
        daggerCtrl = GetComponent<DaggerController>();

        if (!attackPoint)
            Debug.LogWarning("AttackPoint no asignado, usando posición del personaje");
    }

    void Update()
    {
        if (comboResetTimer > 0)
        {
            comboResetTimer -= Time.deltaTime;
        }
        else if (currentCombo > 0)
        {
            currentCombo = 0;
        }

        if (Input.GetButtonDown("Fire2"))
        {
            TryAttack();
        }
    }

    void TryAttack()
    {
        if (Time.time - lastAttackTime < attackCooldown)
            return;

        currentCombo++;
        if (currentCombo > maxCombo)
            currentCombo = 1;

        comboResetTimer = comboResetTime;
        lastAttackTime = Time.time;

        ExecuteAttack();
    }

    void ExecuteAttack()
    {
        // Animación
        anim.SetTrigger("Attack");

        // Detectar enemigos en rango
        Vector3 attackOrigin = attackPoint ? attackPoint.position : transform.position;
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackOrigin, attackRange, enemyLayer);

        // ── CAMBIO: usar EnemyHealth en lugar de EnemyController ──
        foreach (Collider2D enemy in hitEnemies)
        {
            EnemyHealth enemyHealth = enemy.GetComponent<EnemyHealth>();

            if (enemyHealth != null)
            {
                enemyHealth.TakeDamage((int)attackDamage);
                Debug.Log($"¡Golpe! Enemigo golpeado en {enemy.transform.position}");
            }
        }

        // Regenerar energía de la daga
        if (daggerCtrl != null)
            daggerCtrl.RegenEnergy(energyRegenPerHit);

        // Debug visual
        Debug.Log($"Ataque #{currentCombo} realizado en {attackOrigin}");
    }

    void OnDrawGizmosSelected()
    {
        if (!attackPoint) return;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(attackPoint.position, attackRange);
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(attackPoint.position, 0.1f);
    }

    public int GetCurrentCombo() => currentCombo;
    public bool IsInComboWindow() => comboResetTimer > 0 && currentCombo > 0;
    public float GetComboTimeLeft() => comboResetTimer;
}
