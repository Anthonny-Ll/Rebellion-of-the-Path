using UnityEngine;

public class DashController : MonoBehaviour
{
    [Header("Movimiento")]
    public float dashSpeed = 25f;
    public float dashDuration = 0.3f;
    public float dashCooldown = 1f;

    [Header("Invulnerabilidad")]
    public bool isDashInvulnerable = true;
    public float invulnerabilityDuration = 0.3f;

    [Header("Efectos")]
    public GameObject dashVFXPrefab;

    Rigidbody2D rb;
    Animator anim;
    SpriteRenderer spriteRenderer;
    PlayerController playerCtrl;

    bool isDashing = false;
    float dashTimer = 0f;
    float lastDashTime = -1000f;
    float invulnerabilityTimer = 0f;
    bool wasInvulnerable = false;
    Vector2 dashDirection;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        playerCtrl = GetComponent<PlayerController>();
    }

    void Update()
    {
        if (isDashing)
        {
            dashTimer -= Time.deltaTime;
            if (dashTimer <= 0) EndDash();
        }

        if (invulnerabilityTimer > 0)
        {
            invulnerabilityTimer -= Time.deltaTime;
            wasInvulnerable = true;
            if (invulnerabilityTimer <= 0 && wasInvulnerable)
            {
                RestoreSprite();
                wasInvulnerable = false;
            }
        }

        if (Input.GetKeyDown(KeyCode.E))
            TryDash();
    }

    void TryDash()
    {
        if (isDashing) return;
        if (Time.time - lastDashTime < dashCooldown) return;

        float scaleX = transform.localScale.x;
        dashDirection = scaleX > 0 ? Vector2.right : Vector2.left;

        ExecuteDash();
    }

    void ExecuteDash()
    {
        isDashing = true;
        dashTimer = dashDuration;
        lastDashTime = Time.time;

        // ── Deshabilitar PlayerController durante el dash ──
        if (playerCtrl != null) playerCtrl.enabled = false;

        if (isDashInvulnerable)
        {
            invulnerabilityTimer = invulnerabilityDuration;
            SetSemiTransparent();
        }

        anim.SetTrigger("Dash");

        if (dashVFXPrefab)
            Instantiate(dashVFXPrefab, transform.position, Quaternion.identity);

        // Aplicar velocidad inmediatamente
        rb.linearVelocity = new Vector2(dashDirection.x * dashSpeed, 0);
    }

    void FixedUpdate()
    {
        if (isDashing)
            rb.linearVelocity = new Vector2(dashDirection.x * dashSpeed, 0);
    }

    void EndDash()
    {
        isDashing = false;

        // ── Re-habilitar PlayerController al terminar ──
        if (playerCtrl != null) playerCtrl.enabled = true;

        rb.linearVelocity = Vector2.zero;

        if (invulnerabilityTimer <= 0)
            RestoreSprite();

        if (dashVFXPrefab)
            Instantiate(dashVFXPrefab, transform.position, Quaternion.identity);
    }

    void SetSemiTransparent()
    {
        if (spriteRenderer != null)
        {
            Color c = spriteRenderer.color;
            c.a = 0.5f;
            spriteRenderer.color = c;
        }
    }

    void RestoreSprite()
    {
        if (spriteRenderer != null)
        {
            Color c = spriteRenderer.color;
            c.a = 1f;
            spriteRenderer.color = c;
        }
    }

    public bool IsDashing() => isDashing;
    public bool IsInvulnerable() => invulnerabilityTimer > 0;
    public float GetDashCooldownRemaining() =>
        Mathf.Max(0, dashCooldown - (Time.time - lastDashTime));
}