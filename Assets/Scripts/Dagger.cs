using UnityEngine;

public class Dagger : MonoBehaviour
{
    DaggerController owner;
    bool stuck;

    public void SetOwner(DaggerController c) => owner = c;

    void Start() => Invoke(nameof(SelfDestruct), 5f);

    void OnTriggerEnter2D(Collider2D other)
    {
        if (stuck) return;

        if (other.CompareTag("Ground") || other.CompareTag("Wall"))
        {
            stuck = true;
            owner?.OnDaggerStuck();
            CancelInvoke(nameof(SelfDestruct));
            Invoke(nameof(SelfDestruct), 8f);
        }

       
       
    }

    void SelfDestruct()
    {
        owner?.OnDaggerDestroyed();
        Destroy(gameObject);
    }
}
