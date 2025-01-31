using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;

public class PlayerCamera : NetworkBehaviour
{
    [Header("Camera Settings")]
    [SerializeField] private Camera playerCameraPrefab; // Reference to the camera prefab
    public Camera playerCamera; // The camera instance for the local player
    public GameObject menuCamera; // The camera for all players before joining a game

    [Header("Player Transform")]
    [SerializeField] private Transform playerTransform; // Reference to the player’s transform
    //public Transform desireTurretTransform; // Reference to the turret for camera focus

    [Header("Camera Offset Settings")]
    public float currentOthorgraphicSize = 5; // Offset behind and above the turret
    public float minOthorgraphicSize = 1; // Minimum camera offset
    public float maxOthorgraphicSize = 10; // Maximum camera offset
    public float scrollSpeed = 2; // Scroll speed for zooming
    public float desiredOrthographicSize;

    [Header("Camera Movement Settings")]
    public float smoothSpeed = 0.125f; // Smooth speed for following

    void Start()
    {
        // Spawn the camera only for the local player
        if (IsLocalPlayer)
        {
            SpawnCamera();

            DeactivateMenuCamera();
            UIController.instance.SwitchMenu(MenuState.InGame);

            // Notify the Game Manager to deactivate the main menu camera
            //FindObjectOfType<GameManager>()?.RequestDeactivateMainMenuCameraServerRpc();
        }
    }

    void FixedUpdate()
    {
        // Camera follow logic for the local player
        if (IsLocalPlayer && playerCamera != null)
        {
            // Calculate target position based on turret's rotation and offset
            Vector2 desiredPosition = playerTransform.position;

            // Smoothly move the camera to the target position
            Vector2 smoothedPosition = Vector2.Lerp(playerCamera.transform.position, desiredPosition, smoothSpeed);
            playerCamera.transform.position = new Vector3(smoothedPosition.x, smoothedPosition.y, -10);

            // Force desired position to avoid "jerky" movement with lerp
            //playerCamera.transform.position = new Vector3(desiredPosition.x, desiredPosition.y, -10);;
        }
    }

    void Update()
    {
        if (!IsOwner) return;
        
        // Handle camera zoom with the mouse scroll wheel
        float scrollInput = Input.GetAxis("Mouse ScrollWheel");
        if (Mathf.Abs(scrollInput) > 0.01f) // Small threshold to ignore tiny scrolls
        {
            // Set the target zoom level
            desiredOrthographicSize -= scrollInput * scrollSpeed;
            desiredOrthographicSize = Mathf.Clamp(desiredOrthographicSize, minOthorgraphicSize, maxOthorgraphicSize);
        }

        // Smoothly interpolate the zoom level
        playerCamera.orthographicSize = Mathf.Lerp(playerCamera.orthographicSize, desiredOrthographicSize, smoothSpeed * Time.deltaTime);
    }

    // Zoom function for the camera
    private void Zoom(float scrollInput)
    {
        // Update current zoom level
        currentOthorgraphicSize -= scrollInput * scrollSpeed;

        // Clamp to keep zoom within min/max bounds
        currentOthorgraphicSize = Mathf.Clamp(currentOthorgraphicSize, minOthorgraphicSize, maxOthorgraphicSize);

        // Apply the zoom to the camera
        playerCamera.orthographicSize = currentOthorgraphicSize;
    }


    private void DeactivateMenuCamera()
    {
        if (!IsOwner) return;
         
        GameObject menuCameraObject = GameObject.FindGameObjectWithTag("MainCamera");
        if (menuCameraObject != null)
        {
            menuCameraObject.SetActive(false);
            Debug.Log("Menu camera active set to " + menuCameraObject.activeSelf);
        }
        else
        {
            Debug.LogWarning("Menu camera not found");
        }
    }
    
    // Function to spawn the camera for the local player
    private void SpawnCamera()
    {
        if (playerCamera == null)
        {
            // Instantiate the camera prefab for the local player
            playerCamera = Instantiate(playerCameraPrefab);
            GetComponent<PlayerRotationController>().playerCamera = playerCamera;

            // Set the player’s transform as the target for the camera
            playerTransform = transform; // 'transform' is the player's transform (attached to this object)

            // Position the camera relative to the player
            playerCamera.transform.position = new Vector3(playerTransform.position.x, playerTransform.position.y, -10); // Offset behind and above the player
            //playerCamera.transform.LookAt(playerTransform); // Make the camera face the player

            // Enable the camera
            playerCamera.gameObject.SetActive(true);

            // Lock the cursor to the center of the screen
            //Cursor.lockState = CursorLockMode.Confined;

            // Hide the cursor
            Cursor.visible = true;
        }
    }
}
