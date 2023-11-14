using System;
using System.Collections.Generic;
using System.Linq;
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

        public event Action OnNetworkReady;

        private static NetworkCommunicationManager _instance;

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
        }

        private void OnHostStarted()
        {
            transform.SetParent(null);
            DontDestroyOnLoad(gameObject);
            GameSessionManager.Instance.OnPlayersAssignedToRoles += SendRolesToClients;
            OnNetworkReady?.Invoke();
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
            if (GameSessionManager.Instance.IdxRole.Count == 0) return;
            var clientRpcParamsTemp = new ClientRpcParams
            {
                Send = new ClientRpcSendParams
                {
                    TargetClientIds = new List<ulong> { OwnerClientId }
                }
            };
            var roleTemp = GameSessionManager.Instance.IdxRole[OwnerClientId];
            Debug.Log("Sending special RPC");
            SendRolesToClientsClientRpc(roleTemp, clientRpcParamsTemp);
            foreach (var clientId in GameSessionManager.Instance.ClientsIds)
            {
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
            Toast.Show($"You are {role}");
            Debug.Log($"You are {role}");
        }

        public static IEnumerable<ulong> GetAllConnectedPlayersIDs()
        {
            return NetworkManager.Singleton.ConnectedClientsIds.ToList();
        }
    }
}