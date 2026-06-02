using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Animator))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 8f;
    public float jumpForce = 16f;
    public int maxJumps = 2;

    [Header("Ground Detection")]
    public LayerMask groundLayer;
    public Transform groundCheck;
    public float groundCheckRadius = 0.15f;

    [Header("Health")]
    public int maxHealth = 100;
    public int currentHealth;

    private Rigidbody2D rb;
    private Animator anim;
    private bool facingRight = true;
    private int jumpsLeft;
    private bool isDead;
    private float horizontalInput;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        currentHealth = maxHealth;
        jumpsLeft = maxJumps;
    }

    private void Update()
    {
        if (isDead) return;

        float horizontal = Input.GetAxisRaw("Horizontal");
        bool isGrounded = Physics2D.OverlapCircle(
            groundCheck.position, groundCheckRadius, groundLayer);

        if (isGrounded) jumpsLeft = maxJumps;

        if (Input.GetButtonDown("Jump") && jumpsLeft > 0)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            jumpsLeft--;
        }

        if ((horizontal > 0 && !facingRight) || (horizontal < 0 && facingRight))
            Flip();

        anim.SetFloat("Speed", Mathf.Abs(horizontal));
        anim.SetBool("IsGrounded", isGrounded);
        horizontalInput = horizontal;
    }

    private void FixedUpdate()
    {
        if (isDead) return;
        rb.linearVelocity = new Vector2(horizontalInput * moveSpeed, rb.linearVelocity.y);
    }

    private void Flip()
    {
        facingRight = !facingRight;
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
    }

    private void OnDrawGizmosSelected()
    {
        if (!groundCheck) return;
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
    }

    public void TakeDamage(int damage)
    {
        if (isDead) return;
        currentHealth -= damage;
        anim.SetTrigger("Hit");

        // ── Screen shake al recibir daño ──
        CameraShake.Instance?.Shake(0.25f, 0.2f);

        FindFirstObjectByType<UIAutoSetup>()?.OnPlayerHurt();

        if (currentHealth <= 0) Die();
    }

    private void Die()
    {
        isDead = true;
        anim.SetTrigger("Die");
        GetComponent<Collider2D>().enabled = false;

        // ── Screen shake fuerte al morir ──
        CameraShake.Instance?.Shake(0.4f, 0.35f);

        GameManager.Instance?.TriggerGameOver();
        Invoke(nameof(Reload), 2f);
    }

    private void Reload()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex);
    }
}
