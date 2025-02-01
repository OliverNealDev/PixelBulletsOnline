using Unity.Netcode;
using UnityEngine;
using System.Collections;
using UnityEngine.Serialization;

public class Bullet : NetworkBehaviour
{
    public NetworkVariable<float> bulletDamage = new NetworkVariable<float>(); // Networked damage

    public GameObject bulletOwner;
    //public Vector2 velocityToAdd;
    public NetworkVariable<bool> isPlayerBullet = new NetworkVariable<bool>(); // Networked boolean for player bullet
    private SpriteRenderer spriteRenderer;
    private Collider2D bulletCollider;
    private bool hasHit; // Prevents multiple collisions
    public NetworkVariable<Vector2> velocity = new NetworkVariable<Vector2>();
    private Rigidbody2D rb;

    public GameObject bulletsTransform;
    
    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        bulletCollider = GetComponent<Collider2D>();
        rb = GetComponent<Rigidbody2D>();
        
        if (IsOwner) Invoke("FadeOut", 5f);
        
        if (!IsOwner)
        {
            if (!NetworkObject.IsSpawned)
                Destroy(gameObject);
        }
    }

    void FixedUpdate()
    {
        if (!IsOwner) return;
        
        // Bullets freeze and cannot be despawned when a client late-joins. This is a solution that I'd prefer to fix differently.
        if (rb.linearVelocity == Vector2.zero) Invoke("DoubleCheckZeroVelocity", 0.25f);
    }

    void DoubleCheckZeroVelocity()
    {
        if (rb.linearVelocity == Vector2.zero) gameObject.GetComponent<NetworkObject>().Despawn(); Destroy(gameObject);
    }

    
    void Update()
    {
        if (!IsOwner)
        {
            // Clients apply velocity from the NetworkVariable
            //rb.linearVelocity = velocity.Value;
        }
    }

    public void Initialise(bool isPlayerBullet, float bulletDamage, GameObject bulletOwner)
    {
        if (!IsOwner) return;
        
        this.isPlayerBullet.Value = isPlayerBullet; // Using NetworkVariable.Value to set the value
        this.bulletDamage.Value = bulletDamage; // Setting damage using NetworkVariable.Value
        this.bulletOwner = bulletOwner;
    }

    public void FadeOut()
    {
        StartCoroutine(FadeAndEnlarge()); // Start fade/scale effect
    }

    /*void OnTriggerEnter2D(Collider2D other)
    {
        if (!IsOwner) return;
        
        if (hasHit) return; // Prevent multiple hits
        
        // Server health
        if ((other.tag == "Enemy" && isPlayerBullet.Value) ||
            (other.tag == "Bitrock"))
        {
            //Debug.Log(other.tag);
            //Debug.Log(isPlayerBullet);
            
            other.GetComponent<Health>()?.ReceiveDamage(bulletDamage.Value);
            StartCoroutine(FadeAndEnlarge()); // Start fade/scale effect
        }
        else if (other.tag == "Player" && !isPlayerBullet.Value)
        {
            other.GetComponent<Health>()?.ReceiveDamage(bulletDamage.Value);
            StartCoroutine(FadeAndEnlarge()); // Start fade/scale effect
        }
    }*/

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
        if (networkObject != null && IsOwner)
        {
            gameObject.GetComponent<NetworkObject>().Despawn();
            Destroy(gameObject);
        }

        /*if (gameObject != null)
        {
            Destroy(gameObject);
        }*/
    }
}