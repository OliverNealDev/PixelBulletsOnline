using Unity.Netcode;
using UnityEngine;
using System.Collections;

public class Bullet : NetworkBehaviour
{
    public float bulletDamage;
    public bool isPlayerBullet;
    private SpriteRenderer spriteRenderer;
    private Collider2D bulletCollider;
    private bool hasHit = false; // Prevents multiple collisions

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        bulletCollider = GetComponent<Collider2D>();
        Invoke("Timeout", 5f);
    }

    void Timeout()
    {
        Destroy(gameObject);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (hasHit) return; // Prevent multiple hits
        if ((other.CompareTag("Player") && !isPlayerBullet) || (other.CompareTag("Enemy") && isPlayerBullet))
        {
            other.GetComponent<Health>()?.ReceiveDamage(bulletDamage);
            StartCoroutine(FadeAndEnlarge()); // Start fade/scale effect
        }
    }

    private IEnumerator FadeAndEnlarge()
    {
        hasHit = true;
        bulletCollider.enabled = false; // Disable further collisions

        float duration = 0.25f; // Effect duration
        float elapsed = 0f;
        Vector3 initialScale = transform.localScale;
        Vector3 targetScale = initialScale * 1.5f; // Increase size
        Color initialColor = spriteRenderer.color;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            // Increase size
            transform.localScale = Vector3.Lerp(initialScale, targetScale, t);

            // Fade out
            spriteRenderer.color = new Color(initialColor.r, initialColor.g, initialColor.b, 1 - t);

            yield return null;
        }

        Destroy(gameObject); // Remove bullet after animation
    }
}