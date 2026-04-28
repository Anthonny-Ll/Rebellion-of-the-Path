using UnityEngine;

/// <summary>
/// SWORDCONTROLLER - Sistema de combate con espada
/// 
/// Este script gestiona:
/// - Ataques rápidos con la espada (click derecho)
/// - Combo de ataques (hasta 3 golpes seguidos)
/// - Regeneración de energía al atacar
/// - Cooldown entre ataques
/// - Efectos visuales de ataque
/// - Daño a enemigos en rango
/// </summary>

public class SwordController : MonoBehaviour
{
    // ╔══════════════════════════════════════════════════════════════════════╗
    // ║                         VARIABLES PÚBLICAS                          ║
    // ╚══════════════════════════════════════════════════════════════════════╝

    [Header("Ataque")]
    public float attackRange = 1.5f;          // Rango del ataque (distancia)
    public float attackDamage = 10f;          // Daño por ataque
    public int maxCombo = 3;                  // Máximo número de golpes seguidos
    
    [Header("Tiempo")]
    public float attackCooldown = 0.5f;       // Tiempo entre ataques
    public float comboResetTime = 1.5f;       // Tiempo antes de resetear el combo
    public float attackDuration = 0.3f;       // Duración de la animación de ataque
    
    [Header("Energía")]
    public float energyRegenPerHit = 20f;     // Energía restaurada por cada golpe
    
    [Header("Colisión")]
    public LayerMask enemyLayer;              // Layer de enemigos a atacar
    public Transform attackPoint;             // Punto de origen del ataque (punta de la espada)

    // ╔══════════════════════════════════════════════════════════════════════╗
    // ║                    VARIABLES PRIVADAS (Internas)                    ║
    // ╚══════════════════════════════════════════════════════════════════════╝

    Animator anim;                  // Referencia al Animator
    DaggerController daggerCtrl;    // Referencia al DaggerController para restaurar energía
    
    float lastAttackTime = 0f;      // Tiempo del último ataque
    float comboResetTimer = 0f;     // Temporizador para resetear combo
    int currentCombo = 0;           // Número actual de combo (1, 2, 3)
    bool canAttack = true;          // ¿Puede atacar ahora?

    // ╔══════════════════════════════════════════════════════════════════════╗
    // ║                       INICIALIZACIÓN (Start)                        ║
    // ╚══════════════════════════════════════════════════════════════════════╝

    void Start()
    {
        // Obtener referencias
        anim = GetComponent<Animator>();
        daggerCtrl = GetComponent<DaggerController>();

        // Si no hay AttackPoint, usaremos la posición del personaje
        if (!attackPoint)
        {
            Debug.LogWarning("AttackPoint no asignado, usando posición del personaje");
        }
    }

    // ╔══════════════════════════════════════════════════════════════════════╗
    // ║                    ACTUALIZACIÓN POR FRAME (Update)                ║
    // ║ Detecta entrada del usuario para atacar                            ║
    // ╚══════════════════════════════════════════════════════════════════════╝

    void Update()
    {
        // ─────────────────────────────────────────────────────────────────
        // ACTUALIZAR TEMPORIZADOR DE COMBO
        // ─────────────────────────────────────────────────────────────────
        
        // Decrementar el temporizador de reseteo de combo
        if (comboResetTimer > 0)
        {
            comboResetTimer -= Time.deltaTime;
        }
        else if (currentCombo > 0)
        {
            // Si el timer llegó a 0 y hay combo, resetear
            currentCombo = 0;
        }

        // ─────────────────────────────────────────────────────────────────
        // DETECTAR CLICK DERECHO PARA ATACAR
        // ─────────────────────────────────────────────────────────────────
        
        // Fire2 = Click derecho del ratón
        if (Input.GetButtonDown("Fire2"))
        {
            // Intentar atacar
            TryAttack();
        }
    }

    // ╔══════════════════════════════════════════════════════════════════════╗
    // ║                    INTENTAR ATACAR (TryAttack)                      ║
    // ║ Verifica si puede atacar y ejecuta el ataque                       ║
    // ╚══════════════════════════════════════════════════════════════════════╝

    void TryAttack()
    {
        // ─────────────────────────────────────────────────────────────────
        // VALIDACIÓN: ¿Ha pasado suficiente tiempo desde el último ataque?
        // ─────────────────────────────────────────────────────────────────
        
        // Time.time: tiempo total transcurrido desde inicio del juego
        // Si: tiempo_actual - tiempo_último_ataque >= cooldown → puede atacar
        if (Time.time - lastAttackTime < attackCooldown)
        {
            // No ha pasado suficiente tiempo, no puede atacar aún
            return;
        }

        // ─────────────────────────────────────────────────────────────────
        // INCREMENTAR COMBO
        // ─────────────────────────────────────────────────────────────────
        
        // Aumentar el contador de combo
        currentCombo++;
        
        // Si supera el máximo, resetear a 1
        if (currentCombo > maxCombo)
            currentCombo = 1;
        
        // Resetear el temporizador de combo
        comboResetTimer = comboResetTime;
        
        // Registrar el tiempo de este ataque
        lastAttackTime = Time.time;

        // ─────────────────────────────────────────────────────────────────
        // EJECUTAR EL ATAQUE
        // ─────────────────────────────────────────────────────────────────
        
        ExecuteAttack();
    }

    // ╔══════════════════════════════════════════════════════════════════════╗
    // ║                    EJECUTAR ATAQUE (ExecuteAttack)                  ║
    // ║ Realiza el ataque: detecta enemigos, causa daño, restaura energía  ║
    // ╚══════════════════════════════════════════════════════════════════════╝

    void ExecuteAttack()
    {
        // ─────────────────────────────────────────────────────────────────
        // 1. REPRODUCIR ANIMACIÓN
        // ─────────────────────────────────────────────────────────────────
        
        // Usar SetTrigger con nombre dinámico según el combo
        // Esto permite tener animaciones diferentes para cada golpe
        // Attack1, Attack2, Attack3
        string animTrigger = "Attack" + currentCombo;
        anim.SetTrigger(animTrigger);
        
        // También reproducir un trigger genérico
        anim.SetTrigger("Attack");

        // ─────────────────────────────────────────────────────────────────
        // 2. DETECTAR ENEMIGOS EN RANGO
        // ─────────────────────────────────────────────────────────────────
        
        // attackPoint es donde comienza el ataque (punta de la espada)
        Vector3 attackOrigin = attackPoint ? attackPoint.position : transform.position;
        
        // OverlapCircleAll: detecta TODOS los objetos en un círculo
        // - attackOrigin: centro del círculo
        // - attackRange: radio del círculo
        // - enemyLayer: solo detecta objetos en el layer "Enemy"
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(
            attackOrigin, attackRange, enemyLayer);

        // ─────────────────────────────────────────────────────────────────
        // 3. CAUSAR DAÑO A ENEMIGOS DETECTADOS
        // ─────────────────────────────────────────────────────────────────
        
        // Recorrer cada enemigo golpeado
        foreach (Collider2D enemy in hitEnemies)
        {
            // Intentar obtener el script de enemigo
            // Se asume que los enemigos tienen un script con método TakeDamage
            EnemyController enemyCtrl = enemy.GetComponent<EnemyController>();
            
            if (enemyCtrl != null)
            {
                // Causar daño al enemigo
                enemyCtrl.TakeDamage((int)attackDamage);
                
                // Reproducir efecto de impacto (opcional)
                OnHitEffect(enemy.transform.position);
            }
        }

        // ─────────────────────────────────────────────────────────────────
        // 4. RESTAURAR ENERGÍA
        // ─────────────────────────────────────────────────────────────────
        
        // Cada golpe restaura energía de la daga
        if (daggerCtrl != null)
        {
            daggerCtrl.RegenEnergy(energyRegenPerHit);
            
            // Feedback visual: mostrar número de energía restaurada
            // (opcional - se puede agregar un FloatingText)
        }

        // ─────────────────────────────────────────────────────────────────
        // 5. EFECTO VISUAL DEL ATAQUE
        // ─────────────────────────────────────────────────────────────────
        
        // Visualizar el área de ataque (para debugging)
        VisualizeAttackRange();
    }

    // ╔══════════════════════════════════════════════════════════════════════╗
    // ║                  EFECTO AL GOLPEAR (OnHitEffect)                    ║
    // ║ Crea efectos visuales cuando se golpea un enemigo                  ║
    // ╚══════════════════════════════════════════════════════════════════════╝

    void OnHitEffect(Vector3 position)
    {
        // Aquí se pueden agregar efectos:
        // - Instanciar un prefab de impacto
        // - Reproducir sonido
        // - Shake de cámara
        // - Línea de trayectoria
        
        // Ejemplo simple: parpadeo rojo (se implementaría en un shader)
        // El enemigo golpeado podría cambiar su color brevemente
        
        // Por ahora, solo registramos el hit
        Debug.Log($"¡Golpe! Enemigo golpeado en {position}");
    }

    // ╔══════════════════════════════════════════════════════════════════════╗
    // ║               VISUALIZAR RANGO DE ATAQUE (FixedUpdate)              ║
    // ║ Para debugging - muestra el área donde el ataque golpea            ║
    // ╚══════════════════════════════════════════════════════════════════════╝

    void VisualizeAttackRange()
    {
        // Este método puede ser llamado para ver el área de ataque
        // En un juego real, se llamaría en OnDrawGizmosSelected
        // Aquí solo mostramos visualmente durante el ataque
        
        Vector3 attackOrigin = attackPoint ? attackPoint.position : transform.position;
        
        // En la consola mostrar que se realizó ataque
        Debug.Log($"Ataque #{currentCombo} realizado en {attackOrigin}");
    }

    // ╔══════════════════════════════════════════════════════════════════════╗
    // ║              VISUALIZAR EN EDITOR (OnDrawGizmosSelected)            ║
    // ║ Dibuja el rango de ataque en el editor para debugging              ║
    // ╚══════════════════════════════════════════════════════════════════════╝

    void OnDrawGizmosSelected()
    {
        // Si no hay attackPoint, no dibujar
        if (!attackPoint) return;

        // Dibujar un círculo rojo mostrando el rango de ataque
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(attackPoint.position, attackRange);
        
        // Dibujar un punto en el centro del ataque
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(attackPoint.position, 0.1f);
    }

    // ╔══════════════════════════════════════════════════════════════════════╗
    // ║                    OBTENER INFORMACIÓN (Getters)                    ║
    // ║ Otros scripts pueden preguntar el estado del combo                 ║
    // ╚══════════════════════════════════════════════════════════════════════╝

    /// <summary>
    /// Retorna el número actual de combo
    /// 0 = sin combo, 1-3 = número de golpes seguidos
    /// </summary>
    public int GetCurrentCombo()
    {
        return currentCombo;
    }

    /// <summary>
    /// Retorna si el jugador está en el "ventana de combo"
    /// (puede continuar el combo)
    /// </summary>
    public bool IsInComboWindow()
    {
        return comboResetTimer > 0 && currentCombo > 0;
    }

    /// <summary>
    /// Retorna el tiempo restante antes de que se resetee el combo
    /// </summary>
    public float GetComboTimeLeft()
    {
        return comboResetTimer;
    }
}

// ╔════════════════════════════════════════════════════════════════════════╗
// ║                           RESUMEN DEL FLUJO                          ║
// ╚════════════════════════════════════════════════════════════════════════╝
//
// 1. UPDATE(): Detecta click derecho
//    │
//    └─→ TryAttack()
//        │
//        ├─ Validar cooldown (¿puede atacar?)
//        ├─ Incrementar combo (1, 2, 3)
//        └─→ ExecuteAttack()
//            │
//            ├─ Reproducir animación
//            ├─ Detectar enemigos en rango (OverlapCircleAll)
//            ├─ Causar daño a cada enemigo
//            ├─ Restaurar energía (energyRegenPerHit)
//            ├─ Efectos visuales
//            └─ Resetear combo si pasa tiempo
//
// 2. COMBO SYSTEM:
//    - Golpe 1 → 2 segundos para golpe 2
//    - Golpe 2 → 2 segundos para golpe 3
//    - Golpe 3 → Resetear a 0
//    - Si pasa el tiempo → Resetear combo
