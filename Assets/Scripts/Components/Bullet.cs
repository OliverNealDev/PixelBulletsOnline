using Unity.Netcode;
using UnityEngine;
using System.Collections;

public class Bullet : NetworkBehaviour
{
    public NetworkVariable<float> bulletDamage = new NetworkVariable<float>(); // Networked damage
    public NetworkVariable<bool> isPlayerBullet = new NetworkVariable<bool>(); // Networked boolean for player bullet
    private SpriteRenderer spriteRenderer;
    private Collider2D bulletCollider;
    private bool hasHit; // Prevents multiple collisions

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        bulletCollider = GetComponent<Collider2D>();

        if (!IsServer) return;
        Invoke("Timeout", 5f);
    }

    public void Initialise(bool isPlayerBullet, float bulletDamage)
    {
        this.isPlayerBullet.Value = isPlayerBullet; // Using NetworkVariable.Value to set the value
        this.bulletDamage.Value = bulletDamage; // Setting damage using NetworkVariable.Value
    }
    
    void Timeout()
    {
        NetworkObject networkObject = gameObject.GetComponent<NetworkObject>();
        if (networkObject != null && IsServer)
        {
            gameObject.GetComponent<NetworkObject>().Despawn();
        }

        /*if (gameObject != null)
        {
            Destroy(gameObject);
        }*/
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (hasHit) return; // Prevent multiple hits
        if ((other.tag == "Player" && !isPlayerBullet.Value) || (other.tag == "Enemy" && isPlayerBullet.Value))
        {
            Debug.Log(other.tag);
            Debug.Log(isPlayerBullet);
            
            other.GetComponent<Health>()?.ReceiveDamage(bulletDamage.Value);
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

        NetworkObject networkObject = gameObject.GetComponent<NetworkObject>();
        if (networkObject != null && IsServer)
        {
            gameObject.GetComponent<NetworkObject>().Despawn();
        }

        /*if (gameObject != null)
        {
            Destroy(gameObject);
        }*/
    }
}