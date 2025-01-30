using UnityEngine;
using UnityEngine.UI;

public enum MenuState
{
    LoginMenu,
    RegisterMenu,
    MainMenu,
    OptionsMenu,
    PlayMenu,
    Garage,
    HostMenu,
    JoinMenu,
    InGame,
    PauseMenu
}

public class UIController : MonoBehaviour
{
    public static UIController instance;
    
    public Camera menuCamera;

    public GameObject TopBarPanel;

    public GameObject LoginPanel;
    public GameObject RegisterPanel;
    
    public GameObject mainMenuPanel;
    public GameObject optionsMenuPanel;
    public GameObject playMenuPanel;
    public GameObject garagePanel;
    public GameObject HostPanel;
    public GameObject JoinPanel;
    
    public GameObject InGameUI;
    public GameObject PausePanel;

    public MenuState currentState;

    public GameObject FullscreenToggle;

    public GameObject Hulls;
    public GameObject Turrets;

    public MenuState StartingState;

    private bool wasInMenu;
    private bool wasinPauseMenu;

    void Awake()
    {
        instance = this;
    }
    
    void Start()
    {
        SwitchMenu(StartingState);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Debug.Log(currentState);
            
            if (currentState == MenuState.InGame)
            {
                SwitchMenu(MenuState.PauseMenu);
                
                // Lock the cursor to the center of the screen
                Cursor.lockState = CursorLockMode.None;

                // Hide the cursor
                Cursor.visible = false;
            }
            else if (currentState == MenuState.PauseMenu)
            {
                SwitchMenu(MenuState.InGame);
                
                // Lock the cursor to the center of the screen
                Cursor.lockState = CursorLockMode.Locked;

                // Hide the cursor
                Cursor.visible = true;
            }
        }
    }
    
    public void SwitchMenu(MenuState newState)
    {
        // Checks if transitioning from mainMenu so Garage transition returns to correct menu (playing - mainmenu)
        if (mainMenuPanel.activeSelf)
        {
            wasInMenu = true;
        }
        else
        {
            wasInMenu = false;
        }
        
        // Checks if transitioning from playMenu so pause transition returns to correct menu (mainmenu - playmenu)
        if (PausePanel.activeSelf)
        {
            wasinPauseMenu = true;
        }
        else
        {
            wasinPauseMenu = false;
        }
        
        LoginPanel.SetActive(false);
        RegisterPanel.SetActive(false);
        mainMenuPanel.SetActive(false);
        optionsMenuPanel.SetActive(false);
        playMenuPanel.SetActive(false);
        garagePanel.SetActive(false);
        HostPanel.SetActive(false);
        JoinPanel.SetActive(false);
        InGameUI.SetActive(false);
        PausePanel.SetActive(false);

        currentState = newState;

        switch (currentState)
        {
            case MenuState.LoginMenu:
                TopBarPanel.SetActive(true);
                LoginPanel.SetActive(true);
                break;
            case MenuState.RegisterMenu:
                TopBarPanel.SetActive(true);
                RegisterPanel.SetActive(true);
                break;
            case MenuState.MainMenu:
                TopBarPanel.SetActive(true);
                mainMenuPanel.SetActive(true);
                break;
            case MenuState.OptionsMenu:
                TopBarPanel.SetActive(true);
                optionsMenuPanel.SetActive(true);
                break;
            case MenuState.PlayMenu:
                TopBarPanel.SetActive(true);
                playMenuPanel.SetActive(true);
                break;
            case MenuState.Garage:
                TopBarPanel.SetActive(true);
                garagePanel.SetActive(true);
                break;
            case MenuState.HostMenu:
                TopBarPanel.SetActive(true);
                HostPanel.SetActive(true);
                break;
            case MenuState.JoinMenu:
                TopBarPanel.SetActive(true);
                JoinPanel.SetActive(true);
                break;
            case MenuState.InGame:
                TopBarPanel.SetActive(false);
                InGameUI.SetActive(true);
                break;
            case MenuState.PauseMenu:
                //DecideLeaveSessionActivity.instance.ToggleLeaveSessionActivity(true); // when did this happen??
                TopBarPanel.SetActive(true);
                PausePanel.SetActive(true);
                break;
        }
    }

    // Generic Buttons
    public void OnBackToMainMenuButtonClicked()
    {
        if (wasInMenu)
        {
            SwitchMenu(MenuState.MainMenu);

        }
        else
        {
            SwitchMenu(MenuState.PauseMenu);
        }
    }

    public void OnBackToPlayMenuButtonClicked()
    {
        if (wasinPauseMenu)
        {
            SwitchMenu(MenuState.PauseMenu);
        }
        else
        {
            SwitchMenu(MenuState.PlayMenu);
        }
    }
    
    // Login Menu
    public void OnRegisterButtonClicked()
    {
        SwitchMenu(MenuState.RegisterMenu);
    }

    public void OnBackToLoginMenuButtonClicked()
    {
        SwitchMenu(MenuState.LoginMenu);
    }
    
    // Main Menu
    public void OnPlayButtonClicked()
    {
        SwitchMenu(MenuState.PlayMenu);
    }
    public void OnOptionsButtonClicked()
    {
        SwitchMenu(MenuState.OptionsMenu);
    }
    public void OnGarageButtonClicked()
    {
        SwitchMenu(MenuState.Garage);
    }
    public void OnQuitButtonClicked()
    {
        Application.Quit();
    }
    
    // Play Menu
    public void HostButtonClicked()
    {
        SwitchMenu(MenuState.HostMenu);
    }

    public void JoinButtonClicked()
    {
        SwitchMenu(MenuState.JoinMenu);
    }
    
    // Options Menu
    public void OnFullscreenToggleClicked()
    {
        if (FullscreenToggle.GetComponent<Toggle>().isOn)
        {
            Screen.fullScreen = true;
        }
        else
        {
            Screen.fullScreen = false;
        }
    }
    
    // Garage Menu
    public void OnHullsButtonClicked()
    {
        Hulls.SetActive(true);
        Turrets.SetActive(false);
    }
    
    // Garage Menu
    public void OnTurretsButtonClicked()
    {
        Turrets.SetActive(true);
        Hulls.SetActive(false);
    }

    public void OnReturnButtonClicked()
    {
        if (wasInMenu)
        {
            SwitchMenu(MenuState.MainMenu);
        }
        else
        {
            SwitchMenu(MenuState.PauseMenu);
        }
    }
    
    // In Game
    public void OnContinueButtonClicked()
    {
        SwitchMenu(MenuState.InGame);
    }

    public void OnPauseButtonClicked()
    {
        SwitchMenu(MenuState.PauseMenu);
    }

    public void OnSessionSuccessfullyLeft()
    {
        // Deactivate Player from here (maybe tempoary)
        GameObject playerCameraObject = GameObject.FindGameObjectWithTag("PlayerCamera");
        if (playerCameraObject != null)
        {
            playerCameraObject.SetActive(false);
            Debug.Log("Player camera active set to " + playerCameraObject.activeSelf);
        }
        else
        {
            Debug.LogWarning("Player camera not found");
        }
        
        // Activate Menu Camera from here (maybe temporary)
        menuCamera.gameObject.SetActive(true);
        
        SwitchMenu(MenuState.MainMenu);
    }
}

