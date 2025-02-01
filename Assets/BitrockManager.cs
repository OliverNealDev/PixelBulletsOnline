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
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (!IsServer) return;
        
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
        if (!IsServer) return;
        
        for (int i = 0; i < bitrocks.Count - 1; i++)
        {
            if (bitrocks[i] == null)
            {
                bitrocks.RemoveAt(i);
            }
        }
        
        currentBitrockCount = bitrocks.Count;
    }

    void SpawnBitrocks(int Amount)
    {
        if (!IsServer) return;
        
        // Prevent too many objects spawning in one execution
        if (Amount > 50)
        {
            Amount = 50;
        }

        for (int i = 0; i < Amount; i++)
        {
            int rand = Random.Range(0, 101);
            
            // Common 16x16 bitrock
            if (rand < 51)
            {
                randomBitrock = bitrock16Prefab;
            }
            
            // Uncommon 24x24 bitrock
            else if (rand < 81)
            {
                randomBitrock = bitrock24Prefab;
            }
            
            // Rare 32x32 bitrock
            else if (rand < 101)
            {
                randomBitrock = bitrock32Prefab;
            }

            // Catch an error if this happens
            else
            {
                Debug.LogError("unknown bitrock picked!"); return;
            }

            Vector2 randomBitrockSpawnPosition = new Vector2(Random.Range(-100f, 100f), Random.Range(-100f, 100f));
            
            GameObject bitrock = Instantiate(randomBitrock, randomBitrockSpawnPosition, Quaternion.identity);
            bitrock.GetComponent<NetworkObject>().Spawn();
            bitrocks.Add(bitrock);
        }
    }
}
