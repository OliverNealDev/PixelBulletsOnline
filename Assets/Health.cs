using System;
using UnityEngine;
using System.Collections;
using Unity.Netcode;

public class Health : NetworkBehaviour
{
    public float maxHealth = 100f;
    private NetworkVariable<float> currentHealth = new NetworkVariable<float>();

    private SpriteRenderer spriteRenderer;
    private Color originalColor;

    void Start()
    {
        // Initialize health value (this should be done only on the server)
        if (IsServer)
        {
            currentHealth.Value = maxHealth;
        }

        spriteRenderer = GetComponent<SpriteRenderer>();
        originalColor = spriteRenderer.color;
    }

    // Method to call from client to apply damage (ServerRpc)
    public void ReceiveDamage(float damage)
    {
        OnHitClientRpc();
        
        // Only allow server to apply damage to health
        if (IsServer)
        {
            ApplyDamage(damage);
        }
        else
        {
            // Request the server to apply damage (for non-owned entities like enemies)
            ReceiveDamageServerRpc(damage);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void ReceiveDamageServerRpc(float damage)
    {
        // Server applies damage (called by the client, but server must process it)
        ApplyDamage(damage);
    }

    private void ApplyDamage(float damage)
    {
        OnHit();
        
        // Update health on server-side (this modifies the health variable)
        currentHealth.Value -= damage;

        // Check if the object is out of health (only on the server)
        if (currentHealth.Value <= 0)
        {
            // Despawn the enemy or player on the server
            NetworkObject networkObject = gameObject.GetComponent<NetworkObject>();
            if (networkObject != null && IsServer) // Ensure this is done on server
            {
                networkObject.Despawn();
            }
        }
        
        ShowHitEffectClientRpc();
    }

    [ClientRpc]
    private void OnHitClientRpc()
    {
        OnHit();
    }

    public void OnHit()
    {
        StopAllCoroutines(); // Stop any existing color transition
        StartCoroutine(HitColorCoroutine());
    }

    private IEnumerator HitColorCoroutine()
    {
        // Turn the entity red (indicating it's been hit)
        spriteRenderer.color = new Color(1, 0.25f, 0.25f);
        yield return new WaitForSeconds(0.1f); // Be the above color for 0.1 seconds

        // Fade back to original color
        float elapsedTime = 0f;
        while (elapsedTime < 0.2f) // Fade back to the original color
        {
            spriteRenderer.color = Color.Lerp(new Color(1, 0.25f, 0.25f), originalColor, elapsedTime / 0.2f);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        spriteRenderer.color = originalColor;
    }
    
    // ClientRpc that triggers the hit effect on all clients
    [ClientRpc]
    private void ShowHitEffectClientRpc(ClientRpcParams rpcParams = default)
    {
        // Apply the hit effect on all clients
        if (!IsOwner) // Only show the hit effect to clients that are not the owner
        {
            StartCoroutine(HitColorCoroutine()); // Trigger the color change effect on all clients
        }
    }
}
