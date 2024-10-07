using System.Collections.Generic;
using System.Threading.Tasks;
using Backend.Hub.Controllers;
using DataStorage;
using Unity.Services.Lobbies.Models;
using UnityEngine;

namespace Backend.Hub.Managers
{
    public class HubBackendManager : MonoBehaviour
    {
        [SerializeField] private NetworkManagerSpawner networkManagerSpawner;
        [SerializeField] private LobbyController lobbyController;
        [SerializeField] private RelayController relayController;

        public async void Initialize()
        {
            await AuthenticationController.Initialize();
            networkManagerSpawner.Initialize();
            lobbyController.Initialize();
            relayController.Initialize();
        }

        ////////////////////////////////////////////////////////////////////////
        // LOBBY CONTROLLER ////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////

        public void SetPlayerName(string playerName)
        {
            PlayerData.Name = playerName;
        }

        public string GetPlayerName()
        {
            return PlayerData.Name;
        }

        public bool IsLobbyHost()
        {
            return lobbyController.IsLobbyHost();
        }

        public async Task<bool> CreateLobbyAsync(
            string lobbyName,
            int maxPlayersInt,
            bool isPrivate
        )
        {
            return await lobbyController.CreateLobbyAsync("Narrator", lobbyName, maxPlayersInt, isPrivate, "");
        }

        public async Task<List<Lobby>> GetLobbiesList()
        {
            return await LobbyController.GetLobbiesList();
        }

        public List<string> GetPlayersNamesInLobby()
        {
            return lobbyController.GetPlayersNamesInLobby();
        }

        public string GetLobbyName()
        {
            return lobbyController.GetLobbyName();
        }

        public string GetLobbyCode()
        {
            return lobbyController.GetLobbyCode();
        }

        public int GetMaxPlayers()
        {
            return lobbyController.GetMaxPlayers();
        }

        public async Task<bool> JoinLobbyByID(string lobbyID)
        {
            return await lobbyController.JoinLobby(lobbyID: lobbyID, code: null, playerName: PlayerData.Name);
        }

        public async Task<bool> JoinLobbyByCode(string lobbyCode)
        {
            return await lobbyController.JoinLobby(lobbyID: null, code: lobbyCode, playerName: PlayerData.Name);
        }

        public bool IsLobbyJoined()
        {
            return lobbyController.IsLobbyJoined();
        }

        public async Task<bool> LeaveLobby()
        {
            return await lobbyController.LeaveLobby();
        }

        public async Task<bool> StartGame()
        {
            return await lobbyController.StartGame();
        }

        public async Task<bool> EndGame()
        {
            return await lobbyController.EndGame();
        }

        ////////////////////////////////////////////////////////////////////////
        // RELAY CONTROLLER ////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////
    }
}