using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Unity.Services;
using Unity.Services.Authentication;
using Unity.Services.Core;
using UnityEngine.Serialization;
using Unity.Services.CloudSave;
using Unity.Services.CloudSave.Models;
using UnityEngine.Rendering;


public class AuthenticationManager : MonoBehaviour
{
    public GameObject loginUsernameInputField;
    public GameObject loginPasswordInputField;
    
    public GameObject registerUsernameInputField;
    public GameObject registerPasswordInputField;

    public TextMeshProUGUI usernameText;
    private string CachedUsername;

    async void Start()
    {
        await InitializeUnityServices();
    }
    
    private async Task InitializeUnityServices()
    {
        try
        {
            if (UnityServices.State != ServicesInitializationState.Initialized) // ✅ Check initialization state correctly
            {
                await UnityServices.InitializeAsync();
                Debug.Log("Unity Services Initialized");

                // Sign in anonymously for authentication NO DO NOT DO THAT.
                //await SignInAnonymously();
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Unity Services failed to initialize: {ex.Message}");
        }
    }
    
    public async void OnLoginButtonClicked()
    {
        bool isSignedIn = await SignInWithUsernamePasswordAsync(loginUsernameInputField.GetComponent<TMP_InputField>().text, loginPasswordInputField.GetComponent<TMP_InputField>().text);
        if (isSignedIn)
        {
            Debug.Log("User successfully signed in.");
            UIController.instance.SwitchMenu(MenuState.MainMenu);
            AssignUsernameToText();
        }
        else
        {
            Debug.Log("Sign-in failed.");
        }
    }
    public async void OnRegisterButtonClicked()
    {
        CachedUsername = loginUsernameInputField.GetComponent<TMP_InputField>().text;
        bool isSignedUp = await SignUpWithUsernamePasswordAsync(registerUsernameInputField.GetComponent<TMP_InputField>().text, registerPasswordInputField.GetComponent<TMP_InputField>().text);
        if (isSignedUp)
        {
            Debug.Log("User successfully signed up.");
            UIController.instance.SwitchMenu(MenuState.MainMenu);
            //await SaveUsername(CachedUsername);
            AssignUsernameToText();
        }
        else
        {
            Debug.Log("Sign-up failed.");
        }

    }
    
    async Task<bool> SignInWithUsernamePasswordAsync(string username, string password)
    {
        try
        {
            await AuthenticationService.Instance.SignInWithUsernamePasswordAsync(username, password);
            Debug.Log("SignIn is successful.");
            //await SaveUsername(username);
            return true;
        }
        catch (AuthenticationException ex)
        {
            // Compare error code to AuthenticationErrorCodes
            // Notify the player with the proper error message
            Debug.LogException(ex);
        }
        catch (RequestFailedException ex)
        {
            // Compare error code to CommonErrorCodes
            // Notify the player with the proper error message
            Debug.LogException(ex);
        }
        
        return false;
    }
    async Task<bool> SignUpWithUsernamePasswordAsync(string username, string password)
    {
        try
        {
            await AuthenticationService.Instance.SignUpWithUsernamePasswordAsync(username, password);
            Debug.Log("SignUp is successful.");
            Debug.Log("username is " + username);
            await SaveUsername(username);
            return true;
        }
        catch (AuthenticationException ex)
        {
            // Compare error code to AuthenticationErrorCodes
            // Notify the player with the proper error message
            Debug.LogException(ex);
        }
        catch (RequestFailedException ex)
        {
            // Compare error code to CommonErrorCodes
            // Notify the player with the proper error message
            Debug.LogException(ex);
        }

        return false;
    }

    async Task SaveUsername(string username)
    {
        var data = new Dictionary<string, object> { { "username", username } };
        await CloudSaveService.Instance.Data.Player.SaveAsync(data);
        Debug.Log("Username saved: " + username);
    }
    
    async Task<string> GetUsername()
    {
        try
        {
            var savedData = await CloudSaveService.Instance.Data.Player.LoadAsync(new HashSet<string> { "username" });

            if (savedData.TryGetValue("username", out Item usernameItem))
            {
                // Deserialize the object properly
                string json = JsonConvert.SerializeObject(usernameItem.Value);
                string usernameString = JsonConvert.DeserializeObject<string>(json);

                if (!string.IsNullOrEmpty(usernameString))
                {
                    Debug.Log("Retrieved Username: " + usernameString);
                    return usernameString;
                }
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError("Error retrieving username: " + ex.Message);
        }
        return "Guest"; // Default if no username is found
    }
    
    async void AssignUsernameToText()
    {
        usernameText.text = await GetUsername();
    }
}
