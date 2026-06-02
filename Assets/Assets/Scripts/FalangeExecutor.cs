using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(EnemyHealth))]
public class FalangeExecutor : MonoBehaviour
{
    [Header("Patrulla")]
    public Transform[] patrolPoints;
    public float walkSpeed = 2f;
    private int currentPoint = 0;

    [Header("Detección")]
    public float detectionRange = 7f;
    public float attackRange = 1.8f;
    public float fieldOfView = 100f;

    [Header("Ataque")]
    public int meleeDamage = 20;
    public float attackCooldown = 1.2f;
    public float knockbackForce = 5f;
    private float nextAttack;

    [Header("Escudo")]
    public GameObject shieldObject;
    public bool shieldActive = true;

    private EnemyHealth health;
    private bool isDead = false;

    Transform player;
    Rigidbody2D rb;
    Animator anim;

    enum State { Patrol, Alert, Attack, Dead }
    State state = State.Patrol;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        health = GetComponent<EnemyHealth>();
        health.onDamageReceived = OnDamageAttempt;

        if (player == null) Debug.LogError("FalangeExecutor: No encontró al Player.");
        Debug.Log("FalangeExecutor iniciado correctamente.");
    }

    void Update()
    {
        if (isDead || player == null) return;

        if (transform.position.y < -20f)
        {
            Debug.LogWarning("FalangeExecutor cayó fuera del mapa.");
            Destroy(gameObject);
            return;
        }

        float dist = Vector2.Distance(transform.position, player.position);
        CheckShield();

        switch (state)
        {
            case State.Patrol: DoPatrol(dist); break;
            case State.Alert: DoAlert(dist); break;
            case State.Attack: DoAttack(dist); break;
        }

        if (shieldObject != null)
            shieldObject.SetActive(shieldActive);
    }

    void CheckShield()
    {
        if (player == null) return;
        float facingDir = transform.localScale.x;
        float playerDir = player.position.x - transform.position.x;
        bool playerInFront = (facingDir > 0 && playerDir > 0) ||
                             (facingDir < 0 && playerDir < 0);
        shieldActive = playerInFront;
        anim.SetBool("ShieldActive", shieldActive);
    }

    bool OnDamageAttempt(int damage)
    {
        if (shieldActive)
        {
            Debug.Log("Escudo ACTIVO — daño bloqueado");
            anim.SetTrigger("Block");
            StartCoroutine(ShieldHitEffect());
            return false;
        }
        Debug.Log($"Escudo INACTIVO — daño recibido: {damage}");
        return true;
    }

    System.Collections.IEnumerator ShieldHitEffect()
    {
        if (shieldObject != null)
        {
            var sr = shieldObject.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                Color orig = sr.color;
                sr.color = Color.white;
                yield return new WaitForSeconds(0.08f);
                sr.color = orig;
            }
        }
    }

    void DoPatrol(float distToPlayer)
    {
        if (distToPlayer < detectionRange)
        {
            Debug.Log("Falange: Jugador detectado → Estado ALERTA");
            state = State.Alert;
            anim.SetTrigger("Alert");
            return;
        }

        if (patrolPoints.Length == 0) return;

        Transform target = patrolPoints[currentPoint];
        MoveToward(target.position, walkSpeed);

        if (Vector2.Distance(transform.position, target.position) < 0.15f)
            currentPoint = (currentPoint + 1) % patrolPoints.Length;

        anim.SetFloat("Speed", walkSpeed);
    }

    void DoAlert(float distToPlayer)
    {
        if (distToPlayer > detectionRange * 1.5f)
        {
            Debug.Log("Falange: Jugador lejos → Estado PATRULLA");
            state = State.Patrol;
            anim.SetFloat("Speed", 0);
            return;
        }

        if (distToPlayer <= attackRange)
        {
            Debug.Log("Falange: Jugador en rango → Estado ATAQUE");
            state = State.Attack;
            return;
        }

        MoveToward(player.position, walkSpeed * 0.8f);
        anim.SetFloat("Speed", walkSpeed * 0.8f);
        anim.SetBool("Alert", true);
    }

    void DoAttack(float distToPlayer)
    {
        if (distToPlayer > attackRange * 1.5f)
        {
            Debug.Log("Falange: Jugador se alejó → Estado ALERTA");
            state = State.Alert;
            return;
        }

        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
        anim.SetFloat("Speed", 0);

        if (Time.time >= nextAttack)
        {
            nextAttack = Time.time + attackCooldown;
            Debug.Log("Falange: ATACANDO al jugador");
            anim.SetTrigger("Attack");
            DealMeleeDamage(); // ← agregar esta línea
        }
    }

    public void DealMeleeDamage()
    {
        if (player == null) return;
        float dist = Vector2.Distance(transform.position, player.position);
        if (dist <= attackRange + 0.5f)
        {
            Debug.Log($"Falange: Daño aplicado a Kael: {meleeDamage}");
            player.GetComponent<PlayerController>()?.TakeDamage(meleeDamage);
            Vector2 dir = (player.position - transform.position).normalized;
            player.GetComponent<Rigidbody2D>()?.AddForce(dir * knockbackForce, ForceMode2D.Impulse);
        }
    }

    public void Die()
    {
        if (isDead) return;
        isDead = true;
        state = State.Dead;
        shieldActive = false;
        Debug.Log("Falange: MUERTO");
        anim.SetTrigger("Die");
        rb.linearVelocity = Vector2.zero;
        rb.gravityScale = 0;
        rb.bodyType = RigidbodyType2D.Kinematic;
        GetComponent<Collider2D>().enabled = false;

        // ── Notificar GameManager y UI directamente desde aquí ──
        Debug.Log("Falange: Notificando GameManager...");
        FindFirstObjectByType<UIAutoSetup>()?.AddKill();
        GameManager.Instance?.EnemyKilled();

        Destroy(gameObject, 2f);
    }

    void MoveToward(Vector3 target, float speed)
    {
        Vector2 dir = (target - transform.position).normalized;
        rb.linearVelocity = new Vector2(dir.x * speed, rb.linearVelocity.y);
        if (dir.x > 0.05f) transform.localScale = new Vector3(0.5f, 0.5f, 1);
        if (dir.x < -0.05f) transform.localScale = new Vector3(-0.5f, 0.5f, 1);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
        Gizmos.color = Color.cyan;
        Vector3 fwd = transform.localScale.x > 0 ? Vector3.right : Vector3.left;
        Gizmos.DrawRay(transform.position, fwd * 2);
    }
}
