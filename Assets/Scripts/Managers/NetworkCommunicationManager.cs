using System;
using System.Collections.Generic;
using System.Linq;
using DataStorage;
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

        public event Action OnPlayerRoleAssigned;

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
            var playerName = PlayerPrefs.GetString(PpKeys.KeyPlayerName);
            PlayerData.Name = playerName;
            PlayerData.IsAlive = true;
            PlayerData.ClientID = OwnerClientId;
            PlayerData.Role = Roles.Narrator;
            GameSessionManager.Instance.OnPlayersAssignedToRoles += SendRolesToClients;
        }

        private void OnClientConnected(ulong clientId)
        {
            if (IsHost) return;
            Debug.Log($"[NetworkCommunicationManager] OnClientConnected, ClientID: {clientId}");
            var playerName = PlayerPrefs.GetString(PpKeys.KeyPlayerName);
            PlayerData.Name = playerName;
            PlayerData.IsAlive = true;
            PlayerData.ClientID = clientId;
            AddClientNameToListServerRpc(playerName);
        }

        [ServerRpc(RequireOwnership = false)]
        // ReSharper disable once MemberCanBeMadeStatic.Local
        private void AddClientNameToListServerRpc(string playerName, ServerRpcParams rpcParams = default)
        {
            GameSessionManager.Instance.IDToPlayerName[rpcParams.Receive.SenderClientId] = playerName;
            Debug.Log("All current player names:");
            foreach (var idxPlayerName in GameSessionManager.Instance.IDToPlayerName)
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
            foreach (var clientId in GameSessionManager.Instance.ClientsIDs)
            {
                Debug.Log($"Trying to send RPC to {clientId}");
                var clientRpcParams = new ClientRpcParams
                {
                    Send = new ClientRpcSendParams
                    {
                        TargetClientIds = new List<ulong> { clientId }
                    }
                };
                var role = GameSessionManager.Instance.IDToRole[clientId];
                Debug.Log("Sending RPC");
                SendRolesToClientsClientRpc(role, clientRpcParams);
            }
        }

        [ClientRpc]
        // ReSharper disable once MemberCanBeMadeStatic.Local, UnusedParameter.Local 
        private void SendRolesToClientsClientRpc(string role, ClientRpcParams clientRpcParams)
        {
            PlayerData.Role = role;
            OnPlayerRoleAssigned?.Invoke();
            Debug.Log($"You are {role}");
        }

        public static List<ulong> GetAllConnectedPlayersIDs()
        {
            return NetworkManager.Singleton.ConnectedClientsIds.ToList();
        }

        public List<string> GetAllAlivePlayersNames()
        {
            List<string> playersNames = new();
            // TODO: rewrite needed fields in GameSessionManager into ObservableDictionary as showed by ChatGPT
            // TODO: on any change (observed by the server) send ClientRpc to all clients to update their versions of that fields.
            // TODO: For list of strings use FixedString32Bytes array
            // TODO: for dictionary send keys and values separately
            // TODO: in the code, relate to fields in GameSessionManager directly as they will be constantly copied from the server versions
            return playersNames;
        }
    }
}