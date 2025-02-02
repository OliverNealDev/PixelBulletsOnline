using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class BitrockManager : NetworkBehaviour
{
    public GameObject bitrock16Prefab;
    public GameObject bitrock24Prefab;
    public GameObject bitrock32Prefab;

    public int targetBitrockCount;
    private List<GameObject> bitrocks = new List<GameObject>();
    private int currentBitrockCount;
    
    public float bitrockSpawnCooldown;
    private float bitrockSpawnTimer;
    
    private GameObject randomBitrock;
    
    
    
    void Update()
    {
        //if (!IsOwnedByServer) Debug.Log("not the owned by server as bitrock manager");
        //if (!IsOwnedByServer) return;
        
        bitrockSpawnTimer += Time.deltaTime;
        if (bitrockSpawnTimer >= bitrockSpawnCooldown)
        {
            bitrockSpawnTimer -= bitrockSpawnCooldown;
            CountBitrocks();
            SpawnBitrocks(targetBitrockCount - currentBitrockCount);
        }
    }

    void CountBitrocks()
    {
        //if (!IsOwnedByServer) return;
        
        // Loop backwards so removal does not skip any entries.
        for (int i = bitrocks.Count - 1; i >= 0; i--)
        {
            if (bitrocks[i] == null)
            {
                bitrocks.RemoveAt(i);
            }
        }
        
        currentBitrockCount = bitrocks.Count;
    }

    void SpawnBitrocks(int amount)
    {
        //if (!IsOwnedByServer) return;
        
        if (amount > 50) amount = 50;

        for (int i = 0; i < amount; i++)
        {
            int rand = Random.Range(0, 101);
            if (rand < 51)
            {
                randomBitrock = bitrock16Prefab;
            }
            else if (rand < 81)
            {
                randomBitrock = bitrock24Prefab;
            }
            else if (rand < 101)
            {
                randomBitrock = bitrock32Prefab;
            }
            else
            {
                Debug.LogError("unknown bitrock picked!");
                return;
            }

            Vector2 randomBitrockSpawnPosition = new Vector2(Random.Range(-100f, 100f), Random.Range(-100f, 100f));
            GameObject bitrock = Instantiate(randomBitrock, randomBitrockSpawnPosition, Quaternion.identity);
            NetworkObject bitrockNetObj = bitrock.GetComponent<NetworkObject>();

            // Force the server as the owner by using the server's client ID (typically 0)
            bitrockNetObj.SpawnWithOwnership(NetworkManager.ServerClientId);

            //Debug.Log("I spawned a bitrock!");
            //Debug.Log("Bitrock OwnerClientId: " + bitrockNetObj.OwnerClientId);

            bitrocks.Add(bitrock);

        }
    }
}
