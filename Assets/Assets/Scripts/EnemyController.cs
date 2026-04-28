using UnityEngine;

/// <summary>
/// ENEMYCONTROLLER - Sistema básico de enemigos
/// 
/// Este script gestiona:
/// - Vida del enemigo
/// - Recibir daño
/// - Reacciones al daño
/// - Muerte del enemigo
/// </summary>

public class EnemyController : MonoBehaviour
{
    // ╔══════════════════════════════════════════════════════════════════════╗
    // ║                         VARIABLES PÚBLICAS                          ║
    // ╚══════════════════════════════════════════════════════════════════════╝

    [Header("Salud")]
    public int maxHealth = 50;        // Vida máxima del enemigo
    public float knockbackForce = 5f; // Fuerza de empuje al recibir daño

    [Header("Efectos")]
    public GameObject deathVFXPrefab; // Efecto visual al morir
    public bool flashOnHit = true;    // ¿Parpadear al recibir daño?
    
    [Header("Puntuación")]
    public int scoreOnDeath = 10;     // Puntos al derrotar enemigo

    // ╔══════════════════════════════════════════════════════════════════════╗
    // ║                    VARIABLES PRIVADAS (Internas)                    ║
    // ╚══════════════════════════════════════════════════════════════════════╝

    int currentHealth;                // Vida actual
    bool isDead = false;              // ¿Está muerto?
    
    Rigidbody2D rb;                   // Física
    SpriteRenderer sprite;            // Para cambiar color
    Animator anim;                    // Animador
    
    Color originalColor;              // Color original del sprite
    float flashTimer = 0f;            // Temporizador del parpadeo
    float flashDuration = 0.1f;       // Duración del parpadeo

    // ╔══════════════════════════════════════════════════════════════════════╗
    // ║                       INICIALIZACIÓN (Start)                        ║
    // ╚══════════════════════════════════════════════════════════════════════╝

    void Start()
    {
        // Obtener referencias
        rb = GetComponent<Rigidbody2D>();
        sprite = GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>();

        // Inicializar salud
        currentHealth = maxHealth;

        // Guardar color original
        if (sprite)
            originalColor = sprite.color;
    }

    // ╔══════════════════════════════════════════════════════════════════════╗
    // ║                    ACTUALIZACIÓN POR FRAME (Update)                ║
    // ║ Actualizar efectos visuales (parpadeo)                             ║
    // ╚══════════════════════════════════════════════════════════════════════╝

    void Update()
    {
        // ─────────────────────────────────────────────────────────────────
        // ACTUALIZAR PARPADEO AL RECIBIR DAÑO
        // ─────────────────────────────────────────────────────────────────
        
        if (flashTimer > 0)
        {
            flashTimer -= Time.deltaTime;
            
            // Hacer que parpadee entre blanco y color original
            if (sprite)
            {
                float progress = 1 - (flashTimer / flashDuration);
                sprite.color = Color.Lerp(Color.white, originalColor, progress);
            }
        }
    }

    // ╔══════════════════════════════════════════════════════════════════════╗
    // ║                    RECIBIR DAÑO (TakeDamage)                        ║
    // ║ Se llama cuando es golpeado por el jugador                         ║
    // ╚══════════════════════════════════════════════════════════════════════╝

    public void TakeDamage(int damage)
    {
        // ─────────────────────────────────────────────────────────────────
        // VALIDACIÓN: ¿Ya está muerto?
        // ─────────────────────────────────────────────────────────────────
        
        if (isDead) return;

        // ─────────────────────────────────────────────────────────────────
        // RESTAR VIDA
        // ─────────────────────────────────────────────────────────────────
        
        currentHealth -= damage;
        Debug.Log($"Enemigo recibió {damage} de daño. Vida: {currentHealth}/{maxHealth}");

        // ─────────────────────────────────────────────────────────────────
        // EFECTO VISUAL: PARPADEO
        // ─────────────────────────────────────────────────────────────────
        
        if (flashOnHit)
        {
            flashTimer = flashDuration;
        }

        // ─────────────────────────────────────────────────────────────────
        // EFECTO FÍSICO: EMPUJE
        // ─────────────────────────────────────────────────────────────────
        
        if (rb)
        {
            // Calcular dirección del golpe (hacia la izquierda del enemigo)
            // Se puede mejorar usando la posición del jugador
            Vector2 knockbackDir = transform.position.x > 0 ? Vector2.left : Vector2.right;
            
            // Aplicar impulso
            rb.linearVelocity = knockbackDir * knockbackForce;
        }

        // ─────────────────────────────────────────────────────────────────
        // REPRODUCIR ANIMACIÓN
        // ─────────────────────────────────────────────────────────────────
        
        if (anim)
        {
            anim.SetTrigger("Hit");  // Trigger de recibir golpe
        }

        // ─────────────────────────────────────────────────────────────────
        // VERIFICAR SI MURIÓ
        // ─────────────────────────────────────────────────────────────────
        
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    // ╔══════════════════════════════════════════════════════════════════════╗
    // ║                      MUERTE (Die)                                    ║
    // ║ Se ejecuta cuando la vida llega a 0                                ║
    // ╚══════════════════════════════════════════════════════════════════════╝

    void Die()
    {
        // ─────────────────────────────────────────────────────────────────
        // MARCAR COMO MUERTO
        // ─────────────────────────────────────────────────────────────────
        
        isDead = true;
        Debug.Log("¡Enemigo derrotado!");

        // ─────────────────────────────────────────────────────────────────
        // REPRODUCIR ANIMACIÓN DE MUERTE
        // ─────────────────────────────────────────────────────────────────
        
        if (anim)
        {
            anim.SetTrigger("Die");
        }

        // ─────────────────────────────────────────────────────────────────
        // CREAR EFECTO VISUAL
        // ─────────────────────────────────────────────────────────────────
        
        if (deathVFXPrefab)
        {
            Instantiate(deathVFXPrefab, transform.position, Quaternion.identity);
        }

        // ─────────────────────────────────────────────────────────────────
        // AGREGAR PUNTUACIÓN (opcional)
        // ─────────────────────────────────────────────────────────────────
        
        // GameManager.instance.AddScore(scoreOnDeath);

        // ─────────────────────────────────────────────────────────────────
        // DESACTIVAR O DESTRUIR
        // ─────────────────────────────────────────────────────────────────
        
        // Opción 1: Destruir después de un tiempo (para que se vea la animación)
        Destroy(gameObject, 1f);  // Destruir después de 1 segundo
        
        // Opción 2: Desactivar inmediatamente
        // gameObject.SetActive(false);
    }

    // ╔══════════════════════════════════════════════════════════════════════╗
    // ║                    OBTENER INFORMACIÓN (Getters)                    ║
    // ║ Otros scripts pueden preguntar el estado del enemigo               ║
    // ╚══════════════════════════════════════════════════════════════════════╝

    /// <summary>
    /// Retorna la vida actual del enemigo
    /// </summary>
    public int GetCurrentHealth()
    {
        return currentHealth;
    }

    /// <summary>
    /// Retorna el porcentaje de vida (0-1)
    /// </summary>
    public float GetHealthPercent()
    {
        return (float)currentHealth / maxHealth;
    }

    /// <summary>
    /// ¿Está muerto?
    /// </summary>
    public bool IsDead()
    {
        return isDead;
    }
}

// ╔════════════════════════════════════════════════════════════════════════╗
// ║                           RESUMEN DEL FLUJO                          ║
// ╚════════════════════════════════════════════════════════════════════════╝
//
// 1. JUGADOR GOLPEA ENEMIGO
//    │
//    └─→ SwordController.ExecuteAttack()
//        │
//        └─→ enemyCtrl.TakeDamage(damage)
//            │
//            ├─ Restar vida
//            ├─ Parpadeo (flashOnHit)
//            ├─ Empuje (knockback)
//            ├─ Animación de golpe
//            │
//            └─ ¿Vida <= 0?
//                │
//                └─→ Die()
//                    │
//                    ├─ Animación de muerte
//                    ├─ Efecto visual
//                    ├─ Agregar puntuación
//                    └─ Destruir después de 1 segundo
