using UnityEngine;
using System.Collections;
using Unity.Netcode;

public class EnemyMelee : NetworkBehaviour
{
    public float movementSpeed = 5f; // Speed of the enemy
    public float rotationSpeed = 10f;
    public float maxSpeed = 5f;
    public float meleeDamage = 10f;
    private Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        //if (!IsServer) return;
        
        //StartCoroutine(TickMovement()); // Start the ticking system
    }

    void FixedUpdate()
    {
        GameObject nearestPlayer = FindNearestPlayer();
        if (nearestPlayer != null)
        {
            // Sets target to a variable so that FixedUpdate can set unicell rotation at physics tickrate
            if (rb.angularVelocity < 360) rb.angularVelocity += rotationSpeed;

            Vector2 moveInput = Vector2.zero;

            var direction = nearestPlayer.transform.position - transform.position;
            if (direction.x > 0)
            {
                moveInput.x = 1;
            }
            else
            {
                moveInput.x = -1;
            }
            if (direction.y > 0)
            {
                moveInput.y = 1;
            }
            else
            {
                moveInput.y = -1;
            }

            // Normalize input direction to prevent diagonal speed boost
            if (moveInput != Vector2.zero)
            {
                moveInput = moveInput.normalized;
            }

            // Apply acceleration while preserving smooth movement
            rb.linearVelocity += moveInput * movementSpeed * Time.fixedDeltaTime;

            // Limit the max speed
            if (rb.linearVelocity.magnitude > maxSpeed)
            {
                rb.linearVelocity = rb.linearVelocity.normalized * maxSpeed;
            }
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
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            // Deal damage to player
            collision.gameObject.GetComponent<Health>()?.ReceiveDamage(meleeDamage);

            // Calculate bounce direction (away from the player)
            Vector2 bounceDirection = (transform.position - collision.transform.position).normalized;

            // Apply force to bounce away
            float bounceForce = 5f; // Adjust as needed
            rb.AddForce(bounceDirection * bounceForce, ForceMode2D.Impulse);
        }
    }

}