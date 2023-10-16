using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Relay;
using UnityEngine;

public class TestRelay : MonoBehaviour
{
    // TODO Unity Services like in lobby - maybe make one class or sum
    public static TestRelay Instance { get; private set; }

    private async void Start()
    {
        Instance = this;
        await UnityServices.InitializeAsync();
        AuthenticationService.Instance.SignedIn += OnSignedIn;
        await AuthenticationService.Instance.SignInAnonymouslyAsync();
    }

    private static void OnSignedIn()
    {
        Debug.Log("Signed in as " + AuthenticationService.Instance.PlayerId);
    }

    public static async Task<string> CreateRelay()
    {
        try
        {
            // if 4-player game 1 host plus 3 players so 3 connections
            var allocation = await RelayService.Instance.CreateAllocationAsync(3);
            // Use this code to join relay like in lobbies
            var joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
            var relayServerData = new RelayServerData(allocation, "dtls");
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);
            NetworkManager.Singleton.StartHost();
            return joinCode;
        }
        catch (RelayServiceException e)
        {
            Debug.Log(e);
            return "0";
        }
    }

    public static async void JoinRelay(string joinCode)
    {
        try
        {
            var joinAllocation = await RelayService.Instance.JoinAllocationAsync(joinCode);
            var relayServerData = new RelayServerData(joinAllocation, "dtls");
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);
            NetworkManager.Singleton.StartClient();
        }
        catch (RelayServiceException e)
        {
            Debug.Log(e);
        }
    }
}