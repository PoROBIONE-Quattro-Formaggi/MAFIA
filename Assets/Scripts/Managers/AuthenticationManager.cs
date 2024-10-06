using Unity.Services.Authentication;
using Unity.Services.Core;
using UnityEngine;

namespace Managers
{
    public class AuthenticationManager : MonoBehaviour
    {
        // TODO HubInitializer for backend and frontend managers
        // TODO HubBackendManager for initialisation and communication:
        // - Get/Set players in lobby
        // - Get player number
        // - etc.
        // FOR PLAYER:
        // - Save/Get player name
        // - Get lobbies
        // - Join 'by code'
        // - Join 'chosen lobby'
        // FOR HOST:
        // - Save/Get lobby data (town name, population etc.)
        // - 'start game clicked' -> Game Relay
        private async void Start()
        {
            if (UnityServices.State == ServicesInitializationState.Uninitialized)
            {
                await UnityServices.InitializeAsync();
            }

            if (!AuthenticationService.Instance.IsSignedIn)
            {
                await AuthenticationService.Instance.SignInAnonymouslyAsync();
            }
        }
    }
}