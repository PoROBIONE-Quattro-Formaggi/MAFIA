using Backend.Hub.Controllers;
using UnityEngine;

namespace Backend.Hub.Managers
{
    public class HubBackendManager : MonoBehaviour
    {
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

        [SerializeField] private LobbyController lobbyController;
        [SerializeField] private RelayController relayController;
        [SerializeField] private NetworkManagerSpawner networkManagerSpawner;

        public void Initialize()
        {
            AuthenticationController.Initialize();
            lobbyController.Initialize();
            relayController.Initialize();
            networkManagerSpawner.Initialize();
        }
    }
}