using Unity.Netcode;
using UnityEngine;

public class PlayerMovementController : NetworkBehaviour
{
    public float movementSpeed;
    private Rigidbody2D rb;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void FixedUpdate()
    {
        if (!IsOwner) return;
        
        MoveUpdate();
    }

    void MoveUpdate()
    {
        Vector2 moveInput = Vector2.zero;

        if (Input.GetKey(KeyCode.W))
        {
            moveInput.y += 1;
        }
        if (Input.GetKey(KeyCode.A))
        {
            moveInput.x -= 1;
        }
        if (Input.GetKey(KeyCode.S))
        {
            moveInput.y -= 1;
        }
        if (Input.GetKey(KeyCode.D))
        {
            moveInput.x += 1;
        }

        // Normalize input direction to prevent diagonal speed boost
        if (moveInput != Vector2.zero)
        {
            moveInput = moveInput.normalized;
        }

        // Apply acceleration while preserving smooth movement
        rb.linearVelocityX += moveInput.x * movementSpeed * Time.deltaTime;
        rb.linearVelocityY += moveInput.y * movementSpeed * Time.deltaTime;
    }


}
