using System;
using System.Collections.Generic;
using Unity.Mathematics;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

public class EnemyManager : NetworkBehaviour
{
    private List<Vector2> playerPositions = new List<Vector2>();
    
    public GameObject enemyMeleeOne;

    public float enemySpawnCooldown;
    private float enemySpawnTimer;
    public int enemySpawnAmountPerPlayer;
    
    private void Update()
    {
        if (!IsServer) return;
        
        enemySpawnTimer += Time.deltaTime;
        if (enemySpawnTimer >= enemySpawnCooldown)
        {
            //SpawnEnemies(enemyMeleeOne, enemySpawnAmountPerPlayer);
            enemySpawnTimer = 0;
        }
    }

    void SpawnEnemies(GameObject enemyType, int enemyAmountPerPlayer)
    {
        FindPlayers();
        
        foreach (Vector2 playerPosition in playerPositions)
        {
            for (int i = 0; i < enemyAmountPerPlayer; i++)
            {
                Vector2 enemySpawnPosition = GetRandomPositionAwayFromPlayer(playerPosition);
                GameObject enemy = Instantiate(enemyMeleeOne, enemySpawnPosition, quaternion.identity);
                enemy.GetComponent<NetworkObject>().Spawn();
            }
        }
    }
    
    Vector2 GetRandomPositionAwayFromPlayer(Vector2 playerPosition)
    {
        float distance = Random.Range(5f, 10f); // Random distance between 5 and 10
        float angle = Random.Range(0f, 360f);   // Random angle in any direction

        // Convert polar coordinates to Cartesian
        Vector2 offset = new Vector2(
            Mathf.Cos(angle * Mathf.Deg2Rad) * distance,
            Mathf.Sin(angle * Mathf.Deg2Rad) * distance
        );

        return playerPosition + offset; // New position
    }
    
    void FindPlayers()
    {
        playerPositions.Clear(); // Clear previous positions

        GameObject[] players = GameObject.FindGameObjectsWithTag("Player"); // Find all players

        foreach (GameObject player in players)
        {
            Vector2 position = player.transform.position; // Get position as Vector2
            playerPositions.Add(position);
        }
    }
}
