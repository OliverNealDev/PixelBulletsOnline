using System;
using UnityEngine;
using System.Collections;
using Unity.Netcode;

public class HitEffect : NetworkBehaviour
{
    private SpriteRenderer spriteRenderer;
    private Color originalColor;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        originalColor = spriteRenderer.color;
    }

    // Method to call from client to apply damage (ServerRpc)
    public void OnHitEffect()
    {
        OnHitClientRpc();
        
        // Only allow server to apply damage to health
        if (IsOwner)
        {
            OnHitPlayers();
            Debug.Log("I own this!");
        }
        else // kinda sure this else statement causes errors and isn't needed but no probably wrong leave it
        {
            // Request the server to apply damage (for non-owned entities like enemies)
            BeginOnHitEffectForAllServerRpc();
        }
    }
    
    [ServerRpc(RequireOwnership = false)]
    private void BeginOnHitEffectForAllServerRpc()
    {
        // Server applies damage (called by the client, but server must process it)
        OnHitPlayers();
    }

    private void OnHitPlayers()
    {
        OnHit();
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
