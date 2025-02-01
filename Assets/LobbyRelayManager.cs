using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Services.Core;
using Unity.Services.Authentication;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using Unity.Networking.Transport.Relay;
using Random = UnityEngine.Random;

public class LobbyManager : MonoBehaviour
{
    private const int MAX_PLAYERS = 4;
    private const string LOBBY_NAME = "TestLobby";
    // The key used for storing the Relay join code in lobby data.
    private const string LOBBY_JOIN_CODE_KEY = "joinCode";

    private Lobby currentLobby;

    private async void Start()
    {
        // Initialize Unity Services and sign in
        await UnityServices.InitializeAsync();
        await AuthenticationService.Instance.SignInAnonymouslyAsync();
        Debug.Log("Signed in with Unity Services as " + AuthenticationService.Instance.PlayerId);

        // Give a short delay to allow any existing lobby to be registered.
        await Task.Delay(Random.Range(1000, 10000));

        // Check for an open lobby.
        await CheckAndJoinLobbyAsync();
    }

    /// <summary>
    /// Checks for an open lobby.
    /// If one is found, joins it (and then uses its Relay join code to join the Relay allocation).
    /// Otherwise, creates a new Relay allocation and lobby.
    /// </summary>
    private async Task CheckAndJoinLobbyAsync()
    {
        try
        {
            QueryResponse queryResponse = await LobbyService.Instance.QueryLobbiesAsync();
            Debug.Log($"Found {queryResponse.Results.Count} lobby/lobbies.");

            if (queryResponse.Results.Count > 0)
            {
                // For simplicity, join the first lobby.
                Lobby lobbyToJoin = queryResponse.Results[0];
                Debug.Log("Open lobby found: " + lobbyToJoin.Id);

                currentLobby = await LobbyService.Instance.JoinLobbyByIdAsync(lobbyToJoin.Id);
                Debug.Log("Joined lobby: " + currentLobby.Id);

                // Check if the lobby contains a Relay join code.
                if (currentLobby.Data != null &&
                    currentLobby.Data.TryGetValue(LOBBY_JOIN_CODE_KEY, out DataObject joinCodeData) &&
                    !string.IsNullOrEmpty(joinCodeData.Value))
                {
                    string relayJoinCode = joinCodeData.Value;
                    Debug.Log("Relay join code retrieved from lobby: " + relayJoinCode);
                    await JoinRelayAsync(relayJoinCode);
                }
                else
                {
                    Debug.LogWarning("Lobby does not yet have a Relay join code. Creating a new open lobby as host.");
                    await CreateOpenLobbyAsync();
                }
            }
            else
            {
                Debug.Log("No open lobby found. Creating a new open lobby as host.");
                await CreateOpenLobbyAsync();
            }
        }
        catch (Exception ex)
        {
            Debug.LogError("Error querying or joining lobby: " + ex);
            // In case of error, fallback to creating a new open lobby as host.
            await CreateOpenLobbyAsync();
        }
    }

    /// <summary>
    /// Creates a new Relay allocation and then creates an open lobby without forcing a join code.
    /// After creation, the lobby is updated with the Relay join code so that clients can join the Relay allocation.
    /// </summary>
    private async Task CreateOpenLobbyAsync()
    {
        try
        {
            // Create a Relay allocation for the host.
            Allocation allocation = await RelayService.Instance.CreateAllocationAsync(MAX_PLAYERS);
            string relayJoinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
            Debug.Log("Created Relay allocation with join code: " + relayJoinCode);

            // Create RelayServerData using the default UDP/DTLS settings (not using WebSockets).
            RelayServerData relayServerData = new RelayServerData(
                allocation.RelayServer.IpV4,               // Relay server IP
                (ushort)allocation.RelayServer.Port,         // Port (cast to ushort)
                allocation.AllocationIdBytes,                // Allocation ID bytes
                allocation.ConnectionData,                   // Connection data
                new byte[0],                                 // For a host allocation, host connection data is empty
                allocation.Key,                              // HMAC key
                true                                         // isSecure (using DTLS)
            );

            // Configure the UnityTransport with the Relay data.
            UnityTransport transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
            transport.SetRelayServerData(relayServerData);

            // Create a lobby WITHOUT initially forcing a join code.
            CreateLobbyOptions createOptions = new CreateLobbyOptions
            {
                Data = new Dictionary<string, DataObject>()
                // Intentionally leaving the lobby data empty to create an open lobby.
            };

            currentLobby = await LobbyService.Instance.CreateLobbyAsync(LOBBY_NAME, MAX_PLAYERS, createOptions);
            Debug.Log("Created open lobby with ID: " + currentLobby.Id);

            // Now update the lobby to include the Relay join code so that clients can join the Relay allocation.
            var updateOptions = new UpdateLobbyOptions
            {
                Data = new Dictionary<string, DataObject>
                {
                    { LOBBY_JOIN_CODE_KEY, new DataObject(DataObject.VisibilityOptions.Public, relayJoinCode) }
                }
            };
            currentLobby = await LobbyService.Instance.UpdateLobbyAsync(currentLobby.Id, updateOptions);
            Debug.Log("Updated lobby with Relay join code.");

            // Start the host.
            NetworkManager.Singleton.StartHost();
            Debug.Log("Started as host.");

            //UpdateLobbyStatus();
        }
        catch (Exception ex)
        {
            Debug.LogError("Error creating open lobby: " + ex);
        }
    }

    /// <summary>
    /// Joins an existing Relay allocation using the provided join code, then starts the client.
    /// </summary>
    /// <param name="relayJoinCode">The join code stored in the lobby data.</param>
    private async Task JoinRelayAsync(string relayJoinCode)
    {
        try
        {
            JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(relayJoinCode);
            Debug.Log("Joined Relay allocation successfully.");

            // Create RelayServerData for the client (using default UDP/DTLS).
            RelayServerData relayServerData = new RelayServerData(
                joinAllocation.RelayServer.IpV4,
                (ushort)joinAllocation.RelayServer.Port,
                joinAllocation.AllocationIdBytes,
                joinAllocation.ConnectionData,
                joinAllocation.HostConnectionData,
                joinAllocation.Key,
                true
            );

            UnityTransport transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
            transport.SetRelayServerData(relayServerData);

            // Start the client.
            NetworkManager.Singleton.StartClient();
            Debug.Log("Started as client.");
        }
        catch (Exception ex)
        {
            Debug.LogError("Error joining Relay allocation: " + ex);
        }
    }

    
    /*private async void UpdateLobbyStatus()
    {
        while (currentLobby != null)
        {
            try
            {
                currentLobby = await LobbyService.Instance.GetLobbyAsync(currentLobby.Id);

                if (currentLobby.Players.Count == 0)
                {
                    Debug.Log("Lobby is empty, shutting it down...");
                    await LobbyService.Instance.DeleteLobbyAsync(currentLobby.Id);
                    currentLobby = null;
                    break;
                }
            }
            catch (Exception ex)
            {
                Debug.LogError("Error updating lobby: " + ex);
            }

            await Task.Delay(5000); // Check every 5 seconds
        }
    }*/

}
