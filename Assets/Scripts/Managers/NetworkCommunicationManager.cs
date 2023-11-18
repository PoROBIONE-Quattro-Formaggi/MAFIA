using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using DataStorage;
using Unity.Collections;
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

        public static List<ulong> GetAllConnectedPlayersIDs()
        {
            return NetworkManager.Singleton.ConnectedClientsIds.ToList();
        }

        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // SERVER RPCs //////////////////////////////////////////////////////////////////////////////////////////////////////
        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        [ServerRpc(RequireOwnership = false)]
        // ReSharper disable once MemberCanBeMadeStatic.Local
        private void AddClientNameToListServerRpc(string playerName, ServerRpcParams rpcParams = default)
        {
            GameSessionManager.Instance.IDToPlayerName[rpcParams.Receive.SenderClientId] = playerName;
        }

        [ServerRpc(RequireOwnership = false)]
        // ReSharper disable once MemberCanBeMadeStatic.Global
        public void SetAlibiServerRpc(string alibi, ServerRpcParams rpcParams = default)
        {
            GameSessionManager.Instance.IDToAlibi[rpcParams.Receive.SenderClientId] = alibi;
        }

        [ServerRpc(RequireOwnership = false)]
        // ReSharper disable once MemberCanBeMadeStatic.Global
        public void DayVoteForServerRpc(ulong votedForID, ServerRpcParams rpcParams = default)
        {
            GameSessionManager.Instance.IDToVotedForID[rpcParams.Receive.SenderClientId] = votedForID;
        }

        [ServerRpc(RequireOwnership = false)]
        // ReSharper disable once MemberCanBeMadeStatic.Global
        public void MafiaVoteForServerRpc(ulong votedForID, ServerRpcParams rpcParams = default)
        {
            GameSessionManager.Instance.MafiaIDToVotedForID[rpcParams.Receive.SenderClientId] = votedForID;
        }

        [ServerRpc(RequireOwnership = false)]
        // ReSharper disable once MemberCanBeMadeStatic.Global
        public void DoctorVoteForServerRpc(ulong votedForID, ServerRpcParams rpcParams = default)
        {
            GameSessionManager.Instance.DoctorIDToVotedForID[rpcParams.Receive.SenderClientId] = votedForID;
        }

        [ServerRpc(RequireOwnership = false)]
        // ReSharper disable once MemberCanBeMadeStatic.Global
        public void ResidentVoteForServerRpc(int votedForOption, ServerRpcParams rpcParams = default)
        {
            GameSessionManager.Instance.ResidentIDToVotedForOption[rpcParams.Receive.SenderClientId] = votedForOption;
        }

        [ServerRpc(RequireOwnership = false)]
        // ReSharper disable once MemberCanBeMadeStatic.Global
        public void SetLastWordsServerRpc(string lastWords)
        {
            GameSessionManager.Instance.LastWords = lastWords;
            SendLastWordsClientRpc(lastWords);
        }

        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // CLIENT RPCs //////////////////////////////////////////////////////////////////////////////////////////////////////
        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        [ClientRpc]
        // ReSharper disable once MemberCanBeMadeStatic.Local, UnusedParameter.Local 
        private void SendRolesToClientsClientRpc(string role, ClientRpcParams clientRpcParams)
        {
            PlayerData.Role = role;
            OnPlayerRoleAssigned?.Invoke();
        }

        [ClientRpc]
        // ReSharper disable once MemberCanBeMadeStatic.Global
        public void SendNewIDToRoleClientRpc(ulong[] keys, FixedString32Bytes[] values)
        {
            for (var i = 0; i < keys.Length; i++)
            {
                GameSessionManager.Instance.IDToRole[keys[i]] = values[i].ToString();
            }
        }

        [ClientRpc]
        // ReSharper disable once MemberCanBeMadeStatic.Global
        public void SendNewIDToPlayerNameClientRpc(ulong[] keys, FixedString32Bytes[] values)
        {
            for (var i = 0; i < keys.Length; i++)
            {
                GameSessionManager.Instance.IDToPlayerName[keys[i]] = values[i].ToString();
            }
        }

        [ClientRpc]
        // ReSharper disable once MemberCanBeMadeStatic.Global
        public void SendNewIDToIsPlayerAliveClientRpc(ulong[] keys, bool[] values)
        {
            for (var i = 0; i < keys.Length; i++)
            {
                GameSessionManager.Instance.IDToIsPlayerAlive[keys[i]] = values[i];
            }

            PlayerData.IsAlive = GameSessionManager.Instance.IDToIsPlayerAlive[PlayerData.ClientID];
            if (!PlayerData.IsAlive)
            {
                // TODO Player death here
            }
        }

        [ClientRpc]
        // ReSharper disable once MemberCanBeMadeStatic.Global
        public void SendNewIDToAlibiClientRpc(ulong[] keys, FixedString128Bytes[] values)
        {
            for (var i = 0; i < keys.Length; i++)
            {
                GameSessionManager.Instance.IDToAlibi[keys[i]] = values[i].ToString();
            }
        }

        [ClientRpc]
        [SuppressMessage("ReSharper", "UnusedParameter.Global")]
        [SuppressMessage("ReSharper", "MemberCanBeMadeStatic.Global")]
        public void SendNewMafiaIDToVotedForIDClientRpc(ulong[] keys, ulong[] values,
            ClientRpcParams clientRpcParams = default)
        {
            for (var i = 0; i < keys.Length; i++)
            {
                GameSessionManager.Instance.MafiaIDToVotedForID[keys[i]] = values[i];
            }
        }

        [ClientRpc]
        // ReSharper disable once MemberCanBeMadeStatic.Global
        public void SendLastWordsClientRpc(string lastWords)
        {
            GameSessionManager.Instance.LastWords = lastWords;
        }
        
        [ClientRpc]
        // ReSharper disable once MemberCanBeMadeStatic.Global
        public void SendLastKilledNameClientRpc(string lastKilledName)
        {
            GameSessionManager.Instance.LastKilledName = lastKilledName;
        }
    }
}