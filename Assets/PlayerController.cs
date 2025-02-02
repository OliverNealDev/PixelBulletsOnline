using Unity.Netcode;
using UnityEngine;

public class PlayerController : NetworkBehaviour
{
    public GameObject BitrockManager;
    public GameObject EnemyManager;
    
    private int xp;
    private int xpThreshold;
    private int level;

    private void Start()
    {
        xp = 0;
        level = 1;
        xpThreshold = 10; // Base threshold for level 1
        
        Debug.Log(IsServer);
        Debug.Log(IsHost);
        Debug.Log(IsSessionOwner);
        Debug.Log(IsClient);
        Debug.Log(IsOwner);
        Debug.Log(IsLocalPlayer);

        if (IsServer)
        {
            Debug.Log("Spawning server managers because I am a server");
            Instantiate(BitrockManager, new Vector3(0, 0, 0), Quaternion.identity);
            Instantiate(EnemyManager, new Vector3(0, 0, 0), Quaternion.identity);
        }
        else
        {
            Debug.Log("Not spawning server managers because I am not a server");
        }
    }

    // Function to send XP to a player
    [ClientRpc]
    public void GrantXPClientRpc(int amount)
    {
        if (!IsOwner) return;
        
        xp += amount;
        CheckLevelUp();
        
        Debug.Log("I received xp");
    }

    private void CheckLevelUp()
    {
        while (xp >= xpThreshold) 
        {
            LevelUp();
        }
    }

    private void LevelUp()
    {
        xp -= xpThreshold;
        level++;
        xpThreshold = Mathf.RoundToInt(xpThreshold * 1.75f);
        Debug.Log($"Level Up! New Level: {level}, Next Threshold: {xpThreshold}");
        
        GetComponent<PlayerClassController>().ChangeClass(PlayerClassController.Upgrades.Twins);
    }
}