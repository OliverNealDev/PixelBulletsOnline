using UnityEngine;
using Unity.Netcode;

public class GameManager : MonoBehaviour
{
    private static GameManager instance;

    void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);
    }

    public static void DeactivateMainMenuCamera()
    {
        GameObject mainMenuCamera = GameObject.FindGameObjectWithTag("MainMenuCamera");
        if (mainMenuCamera != null)
        {
            mainMenuCamera.SetActive(false);
        }
    }
    
    public static void ActivateMainMenuCamera()
    {
        GameObject mainMenuCamera = GameObject.FindGameObjectWithTag("MainMenuCamera");
        if (mainMenuCamera != null)
        {
            mainMenuCamera.SetActive(true);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void RequestDeactivateMainMenuCameraServerRpc()
    {
        DeactivateMainMenuCameraClientRpc();
    }

    [ClientRpc]
    private void DeactivateMainMenuCameraClientRpc()
    {
        DeactivateMainMenuCamera();
    }
}