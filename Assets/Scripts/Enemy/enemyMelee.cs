using UnityEngine;
using System.Collections;
using Unity.Netcode;

public class EnemyMelee : NetworkBehaviour
{
    public float movementSpeed = 5f; // Speed of the enemy
    private Rigidbody2D rb;
    private float tickRate = 1f / 20f; // 20 ticks per second

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        //if (!IsServer) return;
        
        //StartCoroutine(TickMovement()); // Start the ticking system
    }

    /*IEnumerator TickMovement()
    {
        while (true)
        {
            GameObject nearestPlayer = FindNearestPlayer();
            if (nearestPlayer != null)
            {
                // If nearest Unicell is null, attempt to find a new nearest Unicell to fight, otherwise change state
                if (NearestUnicell == null)
                {
                    var Unicell = FindNearestUnicell(unicell, unicell.ProximityRadius);
                    if (Unicell != null)
                    {
                        unicell.NearestUnicell = Unicell;
                    }
                    else
                    {
                        DecideUnicellState(unicell);
                        break;
                    }
                }

                // Sets target to a variable so that FixedUpdate can set unicell rotation at physics tickrate
                Vector3 CurrentLookingTarget = unicell.NearestUnicell.transform.position - unicell.transform.position;
                if (CurrentLookingTarget != unicell.LookingAtTarget || !unicell.isLookingAtSomething)
                {
                    unicell.LookingAtTarget = CurrentLookingTarget;
                    unicell.isLookingAtSomething = true;
                }

                Direction = unicell.NearestUnicell.transform.position - unicell.transform.position;
                if (Direction.x > 0)
                {
                    Direction.x = unicell.Speed;
                }
                else
                {
                    Direction.x = -unicell.Speed;
                }
                if (Direction.y > 0)
                {
                    Direction.y = unicell.Speed;
                }
                else
                {
                    Direction.y = -unicell.Speed;
                }
            }

            yield return new WaitForSeconds(tickRate); // Wait for the next tick
        }
    }

    GameObject FindNearestPlayer()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        GameObject closest = null;
        float minDistance = Mathf.Infinity;
        Vector2 currentPos = transform.position;

        foreach (GameObject player in players)
        {
            float distance = Vector2.Distance(currentPos, player.transform.position);
            if (distance < minDistance)
            {
                minDistance = distance;
                closest = player;
            }
        }

        return closest;
    }*/
}