using UnityEngine;
using Unity.Netcode;
using Unity.Services.Core;
using Unity.Services.Authentication;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using Unity.Networking.Transport.Relay;
using System;
using System.Threading.Tasks;
using Unity.Netcode.Transports.UTP;
using Random = System.Random;

public class RelayManager : MonoBehaviour
{
    private const int MAX_PLAYERS = 4;

    private async void Start()
    {
        // Initialize Unity Services and sign in
        await UnityServices.InitializeAsync();
        await AuthenticationService.Instance.SignInAnonymouslyAsync();

        float randomdelay = UnityEngine.Random.Range(0f, 5f);
        // Check for an open session; join one if available, otherwise create a new one
        Invoke("CheckAndJoinSessionAsync", randomdelay);
        //await CheckAndJoinSessionAsync();
    }

    /// <summary>
    /// Checks if an open session is available.
    /// If yes, attempts to join it.
    /// Otherwise, creates a new Relay allocation.
    /// </summary>
    private async Task CheckAndJoinSessionAsync()
    {
        // In a real game, this would be replaced with your matchmaking or lobby query.
        string joinCode = await CheckForOpenSessionAsync();

        if (!string.IsNullOrEmpty(joinCode))
        {
            Debug.Log("Open session found. Attempting to join...");
            try
            {
                await JoinRelayAsync(joinCode);
            }
            catch (Exception e)
            {
                Debug.LogWarning("Failed to join the existing session. Creating a new relay allocation. Exception: " + e);
                await CreateRelayAsync();
            }
        }
        else
        {
            Debug.Log("No open session found. Creating a new relay allocation...");
            //await CreateRelayAsync();
            await CreateRelayWithoutJoinCodeAsync();
        }
    }

    /// <summary>
    /// This stub simulates checking for an open session.
    /// Replace with your actual lobby/matchmaking query.
    /// </summary>
    /// <returns>A join code if an open session is available, or an empty string if not.</returns>
    private async Task<string> CheckForOpenSessionAsync()
    {
        // Simulate an async call (for example, querying a lobby service)
        await Task.Delay(500);
        
        // For testing, return an empty string to simulate no open sessions.
        // Return a valid join code (like "ABCD1234") to simulate an available session.
        return "";
    }

    /// <summary>
    /// Creates a Relay allocation without requesting a join code.
    /// This is useful if you're managing free rooms via a separate lobby system.
    /// </summary>
    public async Task CreateRelayWithoutJoinCodeAsync()
    {
        try
        {
            // Create the allocation on Relay
            Allocation allocation = await RelayService.Instance.CreateAllocationAsync(MAX_PLAYERS);
            Debug.Log("Relay allocation created (without join code).");

            // Build RelayServerData using the allocation information.
            // Use the eight-parameter constructor so that the connection is marked as using WebSockets.
            RelayServerData relayServerData = new RelayServerData(
                allocation.RelayServer.IpV4,               // Relay server IP
                (ushort)allocation.RelayServer.Port,         // Relay server port (cast to ushort)
                allocation.AllocationIdBytes,                // Allocation ID bytes
                allocation.ConnectionData,                   // Connection Data
                new byte[0],                                 // HostConnectionData: empty for host allocations
                allocation.Key,                              // HMAC key
                true,                                        // isSecure (WSS)
                true                                         // isWebSocket: explicitly use WebSockets
            );

            // Configure the UnityTransport with this Relay data.
            UnityTransport transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
            transport.SetRelayServerData(relayServerData);

            // Start the host (which will now be connected via Relay using WebSockets).
            NetworkManager.Singleton.StartHost();

            // Optionally, store or publish the allocation info (or a custom room identifier) 
            // to your matchmaking/lobby service so clients can find this room.
        }
        catch (System.Exception ex)
        {
            Debug.LogError("Failed to create Relay allocation without join code: " + ex);
        }
    }
    
    /// <summary>
    /// Creates a new Relay allocation and starts the host.
    /// </summary>
    private async Task CreateRelayAsync()
    {
        try
        {
            // Create an allocation for the host
            Allocation allocation = await RelayService.Instance.CreateAllocationAsync(MAX_PLAYERS);
            string joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
            Debug.Log($"Relay Server Created! Join Code: {joinCode}");

            // Prepare the RelayServerData for the host.
            // Note: For a host, HostConnectionData may not be provided so we pass an empty byte array.
            RelayServerData relayServerData = new RelayServerData(
                allocation.RelayServer.IpV4,        // The actual host, e.g., "123.45.67.89"
                (ushort)allocation.RelayServer.Port,  // Cast the port to ushort
                allocation.AllocationIdBytes,
                allocation.ConnectionData,
                new byte[0],                          // For a host allocation, no host connection data is provided
                allocation.Key,
                true,  // isSecure – typically true for WSS
                true   // isWebSocket – forces usage of the WebSocket interface
            );



            // Configure the UnityTransport component with the Relay data
            UnityTransport transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
            transport.SetRelayServerData(relayServerData);

            // Start the host
            NetworkManager.Singleton.StartHost();
        }
        catch (RelayServiceException e)
        {
            Debug.LogError("Relay exception while creating allocation: " + e);
        }
    }

    /// <summary>
    /// Joins an existing Relay allocation using the provided join code.
    /// </summary>
    /// <param name="joinCode">The join code for the existing session.</param>
    private async Task JoinRelayAsync(string joinCode)
    {
        try
        {
            // Join an existing allocation using the join code
            JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(joinCode);
            Debug.Log("Joined Relay Server!");

            // Prepare the RelayServerData for the client.
            RelayServerData relayServerData = new RelayServerData(
                joinAllocation.RelayServer.IpV4,        // Use the provided relay server IP address
                (ushort)joinAllocation.RelayServer.Port,  // Cast the port to ushort
                joinAllocation.AllocationIdBytes,
                joinAllocation.ConnectionData,
                joinAllocation.HostConnectionData,        // For join allocations, this is provided
                joinAllocation.Key,
                true,  // isSecure
                true   // isWebSocket
            );



            // Configure the UnityTransport component with the Relay data
            UnityTransport transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
            transport.SetRelayServerData(relayServerData);

            // Start the client
            NetworkManager.Singleton.StartClient();
        }
        catch (RelayServiceException e)
        {
            Debug.LogError("Relay exception while joining allocation: " + e);
            throw;  // Re-throw to allow CheckAndJoinSessionAsync to handle it
        }
    }
}
