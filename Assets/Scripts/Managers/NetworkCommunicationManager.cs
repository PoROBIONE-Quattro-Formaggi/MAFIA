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

        public bool IsNetworkSpawned { get; private set; }

        private static NetworkCommunicationManager _instance;

        private void Awake()
        {
            if (_instance == null)
            {
                _instance = this;
                transform.SetParent(null);
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        public override void OnNetworkSpawn()
        {
            InvokeRepeating(nameof(TryToSendRolesToClients), 0f, 0.1f);
        }

        public static void StartHost()
        {
            NetworkManager.Singleton.StartHost();
            NetworkManager.Singleton.OnServerStarted += OnHostStarted;
        }

        private static void OnHostStarted()
        {
            Debug.Log("Server started");
            _instance.IsNetworkSpawned = true;
        }

        private void TryToSendRolesToClients()
        {
            if (GameSessionManager.Instance.IdxRole.Count == 0) return;
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

            CancelInvoke(nameof(TryToSendRolesToClients));
        }

        [ClientRpc]
        private void SendRolesToClientsClientRpc(string role, ClientRpcParams clientRpcParams)
        {
            if (clientRpcParams.Send.TargetClientIds[0] != OwnerClientId) return;
            Toast.Show($"You are {role}");
            Debug.Log($"You are {role}");
        }

        public static List<ulong> GetAllConnectedPlayersIDs()
        {
            return NetworkManager.Singleton.ConnectedClientsIds.ToList();
        }
    }
}