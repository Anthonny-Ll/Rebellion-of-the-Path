using UnityEngine;

/// <summary>
/// DASHCONTROLLER - Sistema de movimiento rápido (Dash)
/// 
/// Este script gestiona:
/// - Dash rápido en la dirección actual de movimiento
/// - Invulnerabilidad durante el dash
/// - Cooldown entre dashes
/// - Efectos visuales (trail, destello)
/// - Control de velocidad y duración
/// </summary>

public class DashController : MonoBehaviour
{
    // ╔══════════════════════════════════════════════════════════════════════╗
    // ║                         VARIABLES PÚBLICAS                          ║
    // ╚══════════════════════════════════════════════════════════════════════╝

    [Header("Movimiento")]
    public float dashSpeed = 25f;              // Velocidad durante el dash
    public float dashDuration = 0.3f;          // Duración del dash (segundos)
    public float dashCooldown = 1f;            // Tiempo antes de poder dashear de nuevo
    
    [Header("Dirección")]
    public bool useFacingDirection = true;     // ¿Usar la dirección hacia donde mira?
    public bool useInputDirection = false;     // ¿Usar la dirección del input (A/D)?
    
    [Header("Invulnerabilidad")]
    public bool isDashInvulnerable = true;     // ¿Ser invulnerable durante dash?
    public float invulnerabilityDuration = 0.3f; // Duración de invulnerabilidad
    
    [Header("Efectos")]
    public GameObject dashVFXPrefab;           // Prefab del efecto visual
    public bool createTrail = true;            // ¿Crear rastro de movimiento?
    public bool cameraShake = true;            // ¿Temblar la cámara?

    // ╔══════════════════════════════════════════════════════════════════════╗
    // ║                    VARIABLES PRIVADAS (Internas)                    ║
    // ╚══════════════════════════════════════════════════════════════════════╝

    Rigidbody2D rb;                   // Referencia a la física
    Animator anim;                    // Referencia al Animator
    PlayerController playerCtrl;      // Referencia al PlayerController (para dirección)

    bool isDashing = false;           // ¿Está haciendo dash ahora?
    float dashTimer = 0f;             // Tiempo restante del dash
    float lastDashTime = -1000f;      // Tiempo del último dash
    float invulnerabilityTimer = 0f;  // Tiempo de invulnerabilidad restante
    
    Vector2 dashDirection;            // Dirección del dash
    bool facingRight = true;          // Dirección que mira el personaje

    // ╔══════════════════════════════════════════════════════════════════════╗
    // ║                       INICIALIZACIÓN (Start)                        ║
    // ╚══════════════════════════════════════════════════════════════════════╝

    void Start()
    {
        // Obtener referencias
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        playerCtrl = GetComponent<PlayerController>();
    }

    // ╔══════════════════════════════════════════════════════════════════════╗
    // ║                    ACTUALIZACIÓN POR FRAME (Update)                ║
    // ║ Detecta entrada para dash y actualiza temporizadores               ║
    // ╚══════════════════════════════════════════════════════════════════════╝

    void Update()
    {
        // ─────────────────────────────────────────────────────────────────
        // ACTUALIZAR TEMPORIZADORES
        // ─────────────────────────────────────────────────────────────────
        
        // Disminuir timer del dash
        if (isDashing)
        {
            dashTimer -= Time.deltaTime;
            
            // Si el dash terminó
            if (dashTimer <= 0)
            {
                EndDash();
            }
        }

        // Disminuir timer de invulnerabilidad
        if (invulnerabilityTimer > 0)
        {
            invulnerabilityTimer -= Time.deltaTime;
        }

        // ─────────────────────────────────────────────────────────────────
        // DETECTAR ENTRADA PARA DASH
        // ─────────────────────────────────────────────────────────────────
        
        // Usaremos una tecla específica: E
        // Si lo prefieres, puedes cambiar a otro botón
        if (Input.GetKeyDown(KeyCode.E))
        {
            TryDash();
        }

        // ─────────────────────────────────────────────────────────────────
        // ACTUALIZAR DIRECCIÓN DEL PERSONAJE
        // ─────────────────────────────────────────────────────────────────
        
        // Obtener entrada horizontal
        float horizontalInput = Input.GetAxisRaw("Horizontal");
        
        // Actualizar dirección si se está moviendo
        if (horizontalInput > 0)
            facingRight = true;
        else if (horizontalInput < 0)
            facingRight = false;
    }

    // ╔══════════════════════════════════════════════════════════════════════╗
    // ║                    INTENTAR DASH (TryDash)                          ║
    // ║ Verifica si puede dashear y lo ejecuta                             ║
    // ╚══════════════════════════════════════════════════════════════════════╝

    void TryDash()
    {
        // ─────────────────────────────────────────────────────────────────
        // VALIDACIÓN: ¿Ya está haciendo dash?
        // ─────────────────────────────────────────────────────────────────
        
        if (isDashing)
        {
            // No puede dashear mientras ya está dasheando
            return;
        }

        // ─────────────────────────────────────────────────────────────────
        // VALIDACIÓN: ¿Ha pasado suficiente tiempo desde el último dash?
        // ─────────────────────────────────────────────────────────────────
        
        // Time.time: tiempo total desde inicio del juego
        // Si: tiempo_actual - tiempo_último_dash >= cooldown → puede dashear
        if (Time.time - lastDashTime < dashCooldown)
        {
            // No ha pasado suficiente tiempo, no puede dashear aún
            return;
        }

        // ─────────────────────────────────────────────────────────────────
        // CALCULAR DIRECCIÓN DEL DASH
        // ─────────────────────────────────────────────────────────────────
        
        if (useFacingDirection)
        {
            // Usar la dirección hacia donde mira el personaje
            dashDirection = facingRight ? Vector2.right : Vector2.left;
        }
        else if (useInputDirection)
        {
            // Usar la dirección del input (A/D)
            float inputX = Input.GetAxisRaw("Horizontal");
            
            if (inputX != 0)
            {
                dashDirection = inputX > 0 ? Vector2.right : Vector2.left;
            }
            else
            {
                // Si no hay input, usar dirección actual
                dashDirection = facingRight ? Vector2.right : Vector2.left;
            }
        }

        // ─────────────────────────────────────────────────────────────────
        // EJECUTAR EL DASH
        // ─────────────────────────────────────────────────────────────────
        
        ExecuteDash();
    }

    // ╔══════════════════════════════════════════════════════════════════════╗
    // ║                    EJECUTAR DASH (ExecuteDash)                      ║
    // ║ Realiza el dash: aplica velocidad, invulnerabilidad, efectos       ║
    // ╚══════════════════════════════════════════════════════════════════════╝

    void ExecuteDash()
    {
        // ─────────────────────────────────────────────────────────────────
        // 1. REGISTRAR QUE ESTÁ DASHEANDO
        // ─────────────────────────────────────────────────────────────────
        
        isDashing = true;                       // Marcar como dasheando
        dashTimer = dashDuration;               // Establecer duración
        lastDashTime = Time.time;               // Guardar tiempo del dash

        // ─────────────────────────────────────────────────────────────────
        // 2. ACTIVAR INVULNERABILIDAD
        // ─────────────────────────────────────────────────────────────────
        
        if (isDashInvulnerable)
        {
            invulnerabilityTimer = invulnerabilityDuration;
            // Aquí se puede cambiar el color del sprite o un shader
            SetInvulnerable(true);
        }

        // ─────────────────────────────────────────────────────────────────
        // 3. REPRODUCIR ANIMACIÓN
        // ─────────────────────────────────────────────────────────────────
        
        // Reproducir animación de dash
        anim.SetTrigger("Dash");

        // ─────────────────────────────────────────────────────────────────
        // 4. CREAR EFECTO VISUAL
        // ─────────────────────────────────────────────────────────────────
        
        if (dashVFXPrefab)
        {
            // Crear efecto en la posición actual
            Instantiate(dashVFXPrefab, transform.position, Quaternion.identity);
        }

        // ─────────────────────────────────────────────────────────────────
        // 5. CREAR RASTRO DE MOVIMIENTO
        // ─────────────────────────────────────────────────────────────────
        
        if (createTrail)
        {
            // Se puede crear un trail renderer durante el dash
            // Por ahora, solo dejamos la funcionalidad lista
        }

        // ─────────────────────────────────────────────────────────────────
        // 6. TEMBLOR DE CÁMARA
        // ─────────────────────────────────────────────────────────────────
        
        if (cameraShake)
        {
            // Hacer vibrar la cámara (si existe el sistema)
            CameraShake();
        }

        Debug.Log($"¡DASH! Dirección: {dashDirection}");
    }

    // ╔══════════════════════════════════════════════════════════════════════╗
    // ║                    FÍSICA DEL DASH (FixedUpdate)                    ║
    // ║ Aplica la velocidad del dash cada frame                            ║
    // ╚══════════════════════════════════════════════════════════════════════╝

    void FixedUpdate()
    {
        // ─────────────────────────────────────────────────────────────────
        // APLICAR VELOCIDAD DEL DASH
        // ─────────────────────────────────────────────────────────────────
        
        if (isDashing)
        {
            // Aplicar velocidad en la dirección del dash
            // dashDirection.x es 1 (derecha) o -1 (izquierda)
            rb.linearVelocity = new Vector2(
                dashDirection.x * dashSpeed,
                0  // No queremos que el dash afecte la velocidad vertical
            );
        }
    }

    // ╔══════════════════════════════════════════════════════════════════════╗
    // ║                      TERMINAR DASH (EndDash)                        ║
    // ║ Se ejecuta cuando el dash termina                                  ║
    // ╚══════════════════════════════════════════════════════════════════════╝

    void EndDash()
    {
        // ─────────────────────────────────────────────────────────────────
        // DETENER EL DASH
        // ─────────────────────────────────────────────────────────────────
        
        isDashing = false;
        
        // Resetear velocidad (el jugador mantiene la dirección normal)
        // Se puede dejar que el playerController controle la velocidad nuevamente
        
        // ─────────────────────────────────────────────────────────────────
        // DESACTIVAR INVULNERABILIDAD
        // ─────────────────────────────────────────────────────────────────
        
        if (isDashInvulnerable && invulnerabilityTimer <= 0)
        {
            SetInvulnerable(false);
        }

        // ─────────────────────────────────────────────────────────────────
        // CREAR EFECTO DE LLEGADA
        // ─────────────────────────────────────────────────────────────────
        
        if (dashVFXPrefab)
        {
            Instantiate(dashVFXPrefab, transform.position, Quaternion.identity);
        }

        Debug.Log("Dash terminado");
    }

    // ╔══════════════════════════════════════════════════════════════════════╗
    // ║                  INVULNERABILIDAD (SetInvulnerable)                 ║
    // ║ Activa/desactiva la invulnerabilidad                               ║
    // ╚══════════════════════════════════════════════════════════════════════╝

    void SetInvulnerable(bool invulnerable)
    {
        // ─────────────────────────────────────────────────────────────────
        // CAMBIAR APARIENCIA
        // ─────────────────────────────────────────────────────────────────
        
        // Se puede parpadear el sprite o cambiar su color
        if (invulnerable)
        {
            // Hacer que brille o parpadee (opcional)
            // Ejemplo: cambiar a rojo o gris semitransparente
            
            // Color actual con alpha reducido
            SpriteRenderer sprite = GetComponent<SpriteRenderer>();
            if (sprite)
            {
                Color c = sprite.color;
                c.a = 0.6f;  // 60% de opacidad
                sprite.color = c;
            }
        }
        else
        {
            // Restaurar apariencia normal
            SpriteRenderer sprite = GetComponent<SpriteRenderer>();
            if (sprite)
            {
                Color c = sprite.color;
                c.a = 1f;  // 100% de opacidad
                sprite.color = c;
            }
        }
    }

    // ╔══════════════════════════════════════════════════════════════════════╗
    // ║                      TEMBLOR DE CÁMARA (CameraShake)               ║
    // ║ Hace vibrar la cámara para efecto de impacto                       ║
    // ╚══════════════════════════════════════════════════════════════════════╝

    void CameraShake()
    {
        // Aquí se puede llamar a un sistema de camera shake
        // Por ahora, solo mostramos cómo se usaría
        
        Camera mainCam = Camera.main;
        if (mainCam != null)
        {
            // Se podría usar un componente CameraShake si existe
            // CameraShake shake = mainCam.GetComponent<CameraShake>();
            // if (shake) shake.Shake(0.1f, 0.15f);  // duración, magnitud
        }
    }

    // ╔══════════════════════════════════════════════════════════════════════╗
    // ║                    OBTENER INFORMACIÓN (Getters)                    ║
    // ║ Otros scripts pueden preguntar el estado del dash                  ║
    // ╚══════════════════════════════════════════════════════════════════════╝

    /// <summary>
    /// ¿Está actualmente hasiendo un dash?
    /// </summary>
    public bool IsDashing()
    {
        return isDashing;
    }

    /// <summary>
    /// ¿Es invulnerable en este momento?
    /// </summary>
    public bool IsInvulnerable()
    {
        return invulnerabilityTimer > 0;
    }

    /// <summary>
    /// Retorna el tiempo que falta para poder dashear de nuevo
    /// </summary>
    public float GetDashCooldownRemaining()
    {
        float timeSinceLastDash = Time.time - lastDashTime;
        float remaining = dashCooldown - timeSinceLastDash;
        return Mathf.Max(0, remaining);
    }

    /// <summary>
    /// Retorna el progreso del dash actual (0-1)
    /// 0 = sin hacer dash, 1 = acaba de empezar
    /// </summary>
    public float GetDashProgress()
    {
        if (!isDashing) return 0f;
        
        // Progress va de 1 (acaba de empezar) a 0 (termina)
        return dashTimer / dashDuration;
    }
}

// ╔════════════════════════════════════════════════════════════════════════╗
// ║                           RESUMEN DEL FLUJO                          ║
// ╚════════════════════════════════════════════════════════════════════════╝
//
// 1. UPDATE(): Detecta tecla E
//    │
//    └─→ TryDash()
//        │
//        ├─ ¿Ya está dasheando? → Salir
//        ├─ ¿Cooldown activo? → Salir
//        └─→ ExecuteDash()
//            │
//            ├─ isDashing = true
//            ├─ Activar invulnerabilidad
//            ├─ Reproducir animación
//            ├─ Crear efectos visuales
//            └─ Temblor de cámara
//
// 2. FIXEDUPDATE(): Aplicar velocidad del dash
//    │
//    └─ rb.linearVelocity = dashDirection * dashSpeed
//
// 3. UPDATE(): Disminuir temporizadores
//    │
//    ├─ Si dashTimer <= 0 → EndDash()
//    ├─ Si invulnerabilityTimer <= 0 → Desactivar invulnerabilidad
//    └─ Actualizar dirección según input
//
// 4. CONTROLES:
//    - E = Dash
//    - Cooldown: 1 segundo entre dashes
//    - Duración: 0.3 segundos de dash rápido
//    - Invulnerabilidad: 0.3 segundos (igual que duración)
