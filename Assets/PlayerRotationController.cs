using Unity.Netcode;
using UnityEngine;

public class PlayerRotationController : NetworkBehaviour
{
    private Rigidbody2D rb;
    public Camera playerCamera;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!IsOwner) return;
        
        TurnUpdate();
    }

    void TurnUpdate()
    {
        Vector3 mouseWorldPos = playerCamera.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, -10));

        // Calculate direction from object to mouse position
        Vector3 direction = (mouseWorldPos - transform.position).normalized;

        // Rotate object to face the mouse
        transform.right = new Vector3(direction.x, direction.y, 0);
    }
}