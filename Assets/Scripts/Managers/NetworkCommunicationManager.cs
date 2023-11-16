using System.Collections.Generic;
using System.Linq;
using DataStorage;
using Third_Party.Toast_UI.Scripts;
using Unity.Netcode;
using UnityEngine;

namespace Managers
{
    public class NetworkCommunicationManager : NetworkBehaviour
    {
        public static NetworkCommunicationManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<NetworkCommunicationManager>();
                }

                return _instance;
            }
        }

        private static NetworkCommunicationManager _instance;

        // CLIENT FIELDS
        private string _playerName;
        private string _role;

        private void Awake()
        {
            if (_instance == null)
            {
                _instance = this;
            }
            else
            {
                Destroy(gameObject);
            }

            NetworkManager.Singleton.OnServerStarted += OnHostStarted;
            NetworkManager.Singleton.OnServerStopped += OnHostStopped;
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
        }

        private void OnHostStarted()
        {
            if (!IsHost) return;
            Debug.Log("[NetworkCommunicationManager] OnHostStarted");
            _playerName = PlayerPrefs.GetString(PpKeys.KeyPlayerName);
            GameSessionManager.Instance.IdxPlayerName[OwnerClientId] = _playerName;
            GameSessionManager.Instance.OnPlayersAssignedToRoles += SendRolesToClients;
        }

        private void OnClientConnected(ulong clientId)
        {
            if (IsHost) return;
            Debug.Log($"[NetworkCommunicationManager] OnClientConnected, ClientID: {clientId}");
            _playerName = PlayerPrefs.GetString(PpKeys.KeyPlayerName);
            AddClientNameToListServerRpc(_playerName);
        }

        [ServerRpc(RequireOwnership = false)]
        // ReSharper disable once MemberCanBeMadeStatic.Local
        private void AddClientNameToListServerRpc(string playerName, ServerRpcParams rpcParams = default)
        {
            GameSessionManager.Instance.IdxPlayerName[rpcParams.Receive.SenderClientId] = playerName;
            Debug.Log("All current player names:");
            foreach (var idxPlayerName in GameSessionManager.Instance.IdxPlayerName)
            {
                Debug.Log($"ID: {idxPlayerName.Key} - {idxPlayerName.Value}");
            }
        }

        private static void OnHostStopped(bool obj)
        {
            Debug.Log("Server stopped");
        }

        public static bool StartHost()
        {
            return NetworkManager.Singleton.StartHost();
        }

        public static bool StartClient()
        {
            return NetworkManager.Singleton.StartClient();
        }

        private void SendRolesToClients()
        {
            GameSessionManager.Instance.OnPlayersAssignedToRoles -= SendRolesToClients;
            foreach (var clientId in GameSessionManager.Instance.ClientsIds)
            {
                Debug.Log($"Trying to send RPC to {clientId}");
                var clientRpcParams = new ClientRpcParams
                {
                    Send = new ClientRpcSendParams
                    {
                        TargetClientIds = new List<ulong> { clientId }
                    }
                };
                var role = GameSessionManager.Instance.IdxRole[clientId];
                Debug.Log("Sending RPC");
                SendRolesToClientsClientRpc(role, clientRpcParams);
            }
        }

        [ClientRpc]
        // ReSharper disable once MemberCanBeMadeStatic.Local, UnusedParameter.Local 
        private void SendRolesToClientsClientRpc(string role, ClientRpcParams clientRpcParams)
        {
            _role = role;
            Toast.Show($"You are {role}");
            Debug.Log($"You are {role}");
        }

        public static List<ulong> GetAllConnectedPlayersIDs()
        {
            return NetworkManager.Singleton.ConnectedClientsIds.ToList();
        }
    }
}