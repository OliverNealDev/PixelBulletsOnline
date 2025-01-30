using Unity.Netcode;
using UnityEngine;

public class PlayerCamera : NetworkBehaviour
{
    [Header("Camera Settings")]
    [SerializeField] private Camera playerCameraPrefab; // Reference to the camera prefab
    private Camera playerCamera; // The camera instance for the local player
    public GameObject menuCamera; // The camera for all players before joining a game

    [Header("Player Transform")]
    [SerializeField] private Transform playerTransform; // Reference to the player’s transform
    public Transform desireTurretTransform; // Reference to the turret for camera focus

    [Header("Camera Offset Settings")]
    public Vector3 currentOffset = new Vector3(0, 4.5f, 2.5f); // Offset behind and above the turret
    public Vector3 minOffset = new Vector3(0, 3, 1); // Minimum camera offset
    public Vector3 maxOffset = new Vector3(0, 9, 13); // Maximum camera offset
    public float scrollSpeed = 2; // Scroll speed for zooming

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

    void LateUpdate()
    {
        // Camera follow logic for the local player
        if (IsLocalPlayer && playerCamera != null)
        {
            if (desireTurretTransform == null)
            {
                Debug.LogWarning("Tank turret is not assigned to CameraFollowTarget.");
                return;
            }

            // Calculate target position based on turret's rotation and offset
            Vector3 desiredPosition = desireTurretTransform.position + desireTurretTransform.rotation * currentOffset;

            // Smoothly move the camera to the target position
            Vector3 smoothedPosition = Vector3.Lerp(playerCamera.transform.position, desiredPosition, smoothSpeed);
            playerCamera.transform.position = smoothedPosition;

            // Force desired position to avoid "jerky" movement with lerp
            playerCamera.transform.position = desiredPosition;

            // Ensure the camera always looks at the turret
            playerCamera.transform.LookAt(desireTurretTransform.position);
        }
    }

    void Update()
    {
        // Handle camera zoom with the mouse scroll wheel
        float scrollInput = Input.GetAxis("Mouse ScrollWheel");
        if (Mathf.Abs(scrollInput) > 0.01f) // Small threshold to ignore tiny scrolls
        {
            Zoom(scrollInput);
        }

        /*if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (!Cursor.visible)
            {
                // Lock the cursor to the center of the screen
                Cursor.lockState = CursorLockMode.None;

                // Hide the cursor
                Cursor.visible = true;
            }
            else
            {
                // Lock the cursor to the center of the screen
                Cursor.lockState = CursorLockMode.Locked;

                // Hide the cursor
                Cursor.visible = false;
            }
        }*/
    }

    // Zoom function for the camera
    private void Zoom(float scrollInput)
    {
        currentOffset.y = Mathf.Clamp(currentOffset.y - scrollInput * scrollSpeed, minOffset.y, maxOffset.y);
        currentOffset.z = Mathf.Clamp(currentOffset.z - ((scrollInput * scrollSpeed) * 2), minOffset.z, maxOffset.z);
    }

    private void DeactivateMenuCamera()
    {
        if (!IsOwner) return;
         
        GameObject menuCameraObject = GameObject.FindGameObjectWithTag("MainMenuCamera");
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

            // Set the player’s transform as the target for the camera
            playerTransform = transform; // 'transform' is the player's transform (attached to this object)

            // Position the camera relative to the player
            playerCamera.transform.position = playerTransform.position + new Vector3(0, 5, -10); // Offset behind and above the player
            playerCamera.transform.LookAt(playerTransform); // Make the camera face the player

            // Enable the camera
            playerCamera.gameObject.SetActive(true);

            // Lock the cursor to the center of the screen
            Cursor.lockState = CursorLockMode.Locked;

            // Hide the cursor
            Cursor.visible = false;
        }
    }
}
