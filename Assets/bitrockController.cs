using Unity.Netcode;
using UnityEngine;

public class bitrockController : NetworkBehaviour
{
    public int xpValue;
    public float MaxHealth;
    private float currentHealth;

    void Start()
    {
        // Use the server as the authoritative instance for health.
        if (!IsOwner) return;
        currentHealth = MaxHealth;
    }
    
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Bullet"))
        {
            Bullet bullet = other.GetComponent<Bullet>();
            if (bullet == null) return;
        
            bullet.FadeOut();
            HitEffectClientRpc();
        
            // Ensure only the server modifies health and despawns
            if (!IsOwner) return;
        
            currentHealth -= bullet.bulletDamage.Value;
        
            if (currentHealth <= 0)
            {
                NetworkObject ownerNetworkObject = bullet.bulletOwner.Value;
                PlayerController ownerController = ownerNetworkObject.GetComponent<PlayerController>();
                if (ownerController != null)
                {
                    ownerController.GrantXPClientRpc(xpValue);
                    Debug.Log("Sending XP to player");
                }
                else
                {
                    Debug.Log("PlayerController component not found on bullet owner.");
                }
            
                NetworkObject networkObject = GetComponent<NetworkObject>();
                if (networkObject != null)
                {
                    networkObject.Despawn();
                    Destroy(gameObject);
                }
            }
        }
    }

    /*[ServerRpc(RequireOwnership = false)]
    private void ProcessMeleeCollisionServerRpc(float damage, ServerRpcParams rpcParams = default)
    {
        // Use the sender's client ID from the RPC parameters.
        ulong hitterClientId = rpcParams.Receive.SenderClientId;
        ProcessMeleeCollision(damage, hitterClientId);
    }*/

    
    /*private void ProcessMeleeCollision(float damage, ulong hitterClientId)
    {
        // Trigger the hit effect on all clients.
        HitEffectClientRpc();
    
        // Subtract the reported damage.
        currentHealth -= damage;
    
        if (currentHealth <= 0)
        {
            // Use the hitterClientId to find the hitter's NetworkObject.
            if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(hitterClientId, out NetworkObject hitterNetworkObject))
            {
                PlayerController hitterController = hitterNetworkObject.GetComponent<PlayerController>();
                if (hitterController != null)
                {
                    hitterController.GrantXP(xpValue);
                    Debug.Log("Sending XP to player from ProcessMeleeCollision");
                }
                else
                {
                    Debug.Log("PlayerController component not found on melee hitter.");
                }
            }
            else
            {
                Debug.Log("Could not find hitter with client ID: " + hitterClientId);
            }
        
            // Despawn and destroy the bitrock.
            NetworkObject networkObject = GetComponent<NetworkObject>();
            if (networkObject != null)
            {
                networkObject.Despawn();
                Destroy(gameObject);
            }
        }
    }*/



    
    // In OnCollisionEnter2D, if the bitrock is hit by a melee attack...
    /*void OnCollisionEnter2D(Collision2D other)
    {
        Debug.Log("collision detected, IsOwner: " + IsOwner);
    
        if (other.gameObject.CompareTag("Player"))
        {
            // Try to get the PlayerClassController to check melee damage.
            PlayerClassController pcc = other.gameObject.GetComponent<PlayerClassController>();
            if (pcc != null && pcc.meleeDamage.Value > 0)
            {
                // Trigger the hit effect on all clients.
                HitEffectClientRpc();
                Debug.Log("Hit effect triggered");
                
                // If this instance is not running on the server, the client tells the server what to do.
                if (!IsServer)
                {
                    // Get the hitter's PlayerController on this client.
                    PlayerController pc = other.gameObject.GetComponent<PlayerController>();
                    if (pc != null)
                    {
                        // Request the server to grant XP to this PlayerController.
                        pc.RequestGrantXPServerRpc(xpValue);
                        Debug.Log("Requested XP grant from client");
                    }
                    
                    // Also request that the server destroy this bitrock.
                    RequestDestroyServerRpc();
                    return;
                }
                else
                {
                    // If we are on the server, process collision normally.
                    float playerMeleeDamage = pcc.meleeDamage.Value;
                    currentHealth -= playerMeleeDamage;
    
                    if (currentHealth <= 0)
                    {
                        // If the server can find the hitter, grant XP.
                        PlayerController pc = other.gameObject.GetComponent<PlayerController>();
                        if (pc != null)
                        {
                            pc.GrantXP(xpValue);
                            Debug.Log("Server granted XP to player");
                        }
                        else
                        {
                            Debug.Log("PlayerController component not found on melee hitter.");
                        }
    
                        ProcessDestruction();
                    }
                }
            }
            else
            {
                Debug.Log("Player collider detected but no valid melee damage value.");
            }
        }
        else
        {
            Debug.Log("Collided with " + other.gameObject.tag);
        }
    }*/





    // ClientRpc to broadcast the hit effect on every client.
    [ClientRpc]
    private void HitEffectClientRpc()
    {
        GetComponent<HitEffect>()?.OnHit();
    }
}
