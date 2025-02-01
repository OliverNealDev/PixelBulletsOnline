using Unity.Netcode;
using UnityEngine;

public class bitrockController : NetworkBehaviour
{
    public float xpValue;

    public float MaxHealth;
    private float currentHealth;

    void Start()
    {
        if (!IsOwner) return;
        currentHealth = MaxHealth;
    }
    
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.tag == "Bullet")
        {
            Bullet bullet = other.GetComponent<Bullet>();
            bullet.FadeOut();
            GetComponent<HitEffect>()?.OnHit();
            
            if (!IsOwner) return;
            
            currentHealth -= bullet.bulletDamage.Value;

            if (currentHealth <= 0)
            {
                NetworkObject networkObject = gameObject.GetComponent<NetworkObject>();
                if (networkObject != null && IsOwner)
                {
                    gameObject.GetComponent<NetworkObject>().Despawn();
                    Destroy(gameObject);
                }
            }
        }
    }
}
