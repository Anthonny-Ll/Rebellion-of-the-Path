using UnityEngine;

public class DaggerController : MonoBehaviour
{
    [Header("Dagger")]
    public GameObject daggerPrefab;
    public Transform throwPoint;
    public float daggerSpeed = 18f;

    [Header("VFX")]
    public GameObject blinkVFXPrefab;

    [Header("Energy")]
    public float maxEnergy = 100f;
    public float energyCostPerThrow = 30f;

    private float currentEnergy;
    private bool daggerInFlight;
    private GameObject activeDagger;
    private Animator anim;

    private void Start()
    {
        anim = GetComponent<Animator>();
        currentEnergy = maxEnergy;
    }

    private void Update()
    {
        // Click izquierdo: lanzar o teletransportarse
        if (Input.GetButtonDown("Fire1"))
        {
            if (!daggerInFlight && currentEnergy >= energyCostPerThrow)
                ThrowDagger();
            else if (daggerInFlight)
                Translocate();
        }

        // Click derecho: cancelar daga
        if (Input.GetButtonDown("Fire2") && daggerInFlight)
            CancelDagger();
    }

    private void ThrowDagger()
    {
        Vector2 direction = GetAimDirection();
        Vector3 spawnPos = throwPoint ? throwPoint.position : transform.position;

        activeDagger = Instantiate(daggerPrefab, spawnPos, Quaternion.identity);

        Rigidbody2D daggerRb = activeDagger.GetComponent<Rigidbody2D>();
        daggerRb.linearVelocity = direction * daggerSpeed;
        daggerRb.gravityScale = 0.5f;

        // Calcular rotación hacia dirección
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        activeDagger.transform.rotation = Quaternion.Euler(0, 0, angle);

        // Notificar a la daga quién la lanzó
        activeDagger.GetComponent<Dagger>()?.SetOwner(this);

        currentEnergy -= energyCostPerThrow;
        daggerInFlight = true;
        anim.SetTrigger("Throw");
    }

    private void Translocate()
    {
        if (!activeDagger)
        {
            daggerInFlight = false;
            return;
        }

        // Efectos visuales
        if (blinkVFXPrefab)
        {
            Instantiate(blinkVFXPrefab, transform.position, Quaternion.identity);
            Instantiate(blinkVFXPrefab, activeDagger.transform.position, Quaternion.identity);
        }

        // Teletransportarse
        transform.position = activeDagger.transform.position;
        Destroy(activeDagger);
        daggerInFlight = false;
        anim.SetTrigger("Blink");
    }

    private void CancelDagger()
    {
        if (activeDagger)
            Destroy(activeDagger);
        daggerInFlight = false;
    }

    private Vector2 GetAimDirection()
    {
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        return (mousePos - transform.position).normalized;
    }

    // Callbacks desde DaggerController
    public void OnDaggerStuck()
    {
        if (!activeDagger) return;
        Rigidbody2D rb = activeDagger.GetComponent<Rigidbody2D>();
        rb.linearVelocity = Vector2.zero;
        rb.gravityScale = 0;
    }

    public void OnDaggerDestroyed()
    {
        daggerInFlight = false;
        activeDagger = null;
    }

    // Regeneración de energía (llamado desde SwordController)
    public void RegenEnergy(float amount)
    {
        currentEnergy = Mathf.Min(currentEnergy + amount, maxEnergy);
    }
}
