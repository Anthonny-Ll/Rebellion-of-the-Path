using UnityEngine;
using System.Collections;

/// <summary>
/// Agrega este script a la Main Camera.
/// Llama CameraShake.Instance.Shake() desde cualquier script.
/// </summary>
public class CameraShake : MonoBehaviour
{
    public static CameraShake Instance;

    Vector3 originalPos;

    void Awake()
    {
        Instance = this;
        originalPos = transform.localPosition;
    }

    // ── API PÚBLICA ──────────────────────────────────────
    // duration: cuánto dura el shake (segundos)
    // magnitude: qué tan fuerte tiembla
    public void Shake(float duration = 0.2f, float magnitude = 0.15f)
    {
        StopAllCoroutines();
        StartCoroutine(DoShake(duration, magnitude));
    }

    IEnumerator DoShake(float duration, float magnitude)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            float x = Random.Range(-1f, 1f) * magnitude;
            float y = Random.Range(-1f, 1f) * magnitude;

            transform.localPosition = originalPos + new Vector3(x, y, 0f);

            elapsed += Time.deltaTime;
            yield return null;
        }

        // Restaurar posición original
        transform.localPosition = originalPos;
    }
}
