using UnityEngine;

public class DaggerController : MonoBehaviour
{
    [Header("Daga")]
    public GameObject daggerPrefab;
    public Transform throwPoint;
    public float daggerSpeed = 18f;

    [Header("Blink VFX")]
    public GameObject blinkVFXPrefab;

    [Header("Energia")]
    public float maxEnergy = 100f;
    public float energyCostPerThrow = 30f;
    float currentEnergy;

    [HideInInspector] public bool daggerInFlight;
    GameObject activeDagger;
    Animator anim;

    void Start()
    {
        anim = GetComponent<Animator>();
        currentEnergy = maxEnergy;
    }

    void Update()
    {
        if (Input.GetButtonDown("Fire1"))
        {
            if (!daggerInFlight && currentEnergy >= energyCostPerThrow)
                ThrowDagger();
            else if (daggerInFlight)
                Translocate();
        }

        if (Input.GetButtonDown("Fire2") && daggerInFlight)
            CancelDagger();
    }

    void ThrowDagger()
    {
        Vector2 dir = GetAimDir();
        Vector3 spawn = throwPoint ? throwPoint.position : transform.position;

        activeDagger = Instantiate(daggerPrefab, spawn, Quaternion.identity);

        var rb = activeDagger.GetComponent<Rigidbody2D>();
        rb.linearVelocity = dir * daggerSpeed;
        rb.gravityScale = 0.5f;

        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        activeDagger.transform.rotation = Quaternion.Euler(0, 0, angle);

        activeDagger.GetComponent<Dagger>()?.SetOwner(this);

        currentEnergy -= energyCostPerThrow;
        daggerInFlight = true;
        anim.SetTrigger("Throw");
    }

    void Translocate()
    {
        if (!activeDagger) { daggerInFlight = false; return; }

        if (blinkVFXPrefab)
            Instantiate(blinkVFXPrefab, transform.position, Quaternion.identity);

        transform.position = activeDagger.transform.position;

        if (blinkVFXPrefab)
            Instantiate(blinkVFXPrefab, transform.position, Quaternion.identity);

        Destroy(activeDagger);
        daggerInFlight = false;
        anim.SetTrigger("Blink");
    }

    public void OnDaggerStuck()
    {
        if (!activeDagger) return;
        var rb = activeDagger.GetComponent<Rigidbody2D>();
        rb.linearVelocity = Vector2.zero;
        rb.gravityScale = 0;
    }

    public void OnDaggerDestroyed()
    {
        daggerInFlight = false;
        activeDagger = null;
    }

    public void RegenEnergy(float amount) =>
        currentEnergy = Mathf.Min(currentEnergy + amount, maxEnergy);

    void CancelDagger()
    {
        if (activeDagger) Destroy(activeDagger);
        daggerInFlight = false;
    }

    Vector2 GetAimDir()
    {
        Vector3 mouse = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        return (mouse - transform.position).normalized;
    }
}