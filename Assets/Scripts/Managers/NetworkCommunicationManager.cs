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
        public event Action OnOneMafiaVoted;
        public event Action OnOneDoctorVoted;
        public event Action OnDayBegan;
        public event Action OnNightBegan;

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
            Debug.Log($"SEnding serverrpc with player name {playerName}");
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
            var keys = GameSessionManager.Instance.IDToRole.Keys.ToArray();
            var values = GameSessionManager.Instance.IDToRole.Values
                .Select(value => new FixedString32Bytes(value))
                .ToArray();
            SendNewIDToRoleClientRpc(keys, values);
        }

        public static List<ulong> GetAllConnectedPlayersIDs()
        {
            return NetworkManager.Singleton.ConnectedClientsIds.ToList();
        }

        public static ulong GetOwnClientID()
        {
            return NetworkManager.Singleton.LocalClientId;
        }

        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // SERVER RPCs //////////////////////////////////////////////////////////////////////////////////////////////////////
        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        [ServerRpc(RequireOwnership = false)]
        private void AddClientNameToListServerRpc(string playerName, ServerRpcParams rpcParams = default)
        {
            Debug.Log($"Sender clientID: {rpcParams.Receive.SenderClientId}");
            GameSessionManager.Instance.IDToPlayerName[rpcParams.Receive.SenderClientId] = playerName;
            var keys = GameSessionManager.Instance.IDToPlayerName.Keys.ToArray();
            var values = GameSessionManager.Instance.IDToPlayerName.Values
                .Select(value => new FixedString32Bytes(value))
                .ToArray();
            Debug.Log("Sending all names to clientRPC");
            SendNewIDToPlayerNameClientRpc(keys, values);
        }

        [ServerRpc(RequireOwnership = false)]
        public void SetAlibiServerRpc(string alibi, ServerRpcParams rpcParams = default)
        {
            GameSessionManager.Instance.IDToAlibi[rpcParams.Receive.SenderClientId] = alibi;
            var keys = GameSessionManager.Instance.IDToAlibi.Keys.ToArray();
            var values = GameSessionManager.Instance.IDToAlibi.Values
                .Select(value => new FixedString128Bytes(value))
                .ToArray();
            SendNewIDToAlibiClientRpc(keys, values);
        }

        [ServerRpc(RequireOwnership = false)]
        [SuppressMessage("ReSharper", "MemberCanBeMadeStatic.Global")]
        public void DayVoteForServerRpc(ulong votedForID, ServerRpcParams rpcParams = default)
        {
            GameSessionManager.Instance.IDToVotedForID[rpcParams.Receive.SenderClientId] = votedForID;
        }

        [ServerRpc(RequireOwnership = false)]
        public void MafiaVoteForServerRpc(ulong votedForID, ServerRpcParams rpcParams = default)
        {
            Debug.Log($"Received mafia voted for ID: {votedForID}");
            GameSessionManager.Instance.MafiaIDToVotedForID[rpcParams.Receive.SenderClientId] = votedForID;
            var keys = GameSessionManager.Instance.MafiaIDToVotedForID.Keys.ToArray();
            var values = GameSessionManager.Instance.MafiaIDToVotedForID.Values.ToArray();
            //TODO: send to all mafia clients always
            var clientRpcParams = new ClientRpcParams
            {
                Send = new ClientRpcSendParams
                {
                    TargetClientIds = keys.ToList()
                }
            };
            OnOneMafiaVoted?.Invoke();
            SendNewMafiaIDToVotedForIDClientRpc(keys, values, clientRpcParams);
        }

        [ServerRpc(RequireOwnership = false)]
        [SuppressMessage("ReSharper", "MemberCanBeMadeStatic.Global")]
        public void DoctorVoteForServerRpc(ulong votedForID, ServerRpcParams rpcParams = default)
        {
            GameSessionManager.Instance.DoctorIDToVotedForID[rpcParams.Receive.SenderClientId] = votedForID;
            OnOneDoctorVoted?.Invoke();
        }

        [ServerRpc(RequireOwnership = false)]
        [SuppressMessage("ReSharper", "MemberCanBeMadeStatic.Global")]
        public void ResidentVoteForServerRpc(int votedForOption, ServerRpcParams rpcParams = default)
        {
            GameSessionManager.Instance.ResidentIDToVotedForOption[rpcParams.Receive.SenderClientId] = votedForOption;
        }

        [ServerRpc(RequireOwnership = false)]
        public void SetLastWordsServerRpc(string lastWords)
        {
            GameSessionManager.Instance.LastWords = lastWords;
            SendLastWordsClientRpc(lastWords);
        }

        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // CLIENT RPCs //////////////////////////////////////////////////////////////////////////////////////////////////////
        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        [ClientRpc]
        [SuppressMessage("ReSharper", "SuggestBaseTypeForParameter")]
        private void SendNewIDToRoleClientRpc(ulong[] keys, FixedString32Bytes[] values)
        {
            if (IsHost) return;
            for (var i = 0; i < keys.Length; i++)
            {
                GameSessionManager.Instance.IDToRole[keys[i]] = values[i].ToString();
            }

            PlayerData.Role = GameSessionManager.Instance.IDToRole[PlayerData.ClientID];
            OnPlayerRoleAssigned?.Invoke();
        }

        [ClientRpc]
        [SuppressMessage("ReSharper", "MemberCanBeMadeStatic.Local")]
        [SuppressMessage("ReSharper", "SuggestBaseTypeForParameter")]
        private void SendNewIDToPlayerNameClientRpc(ulong[] keys, FixedString32Bytes[] values)
        {
            if (IsHost) return;
            Debug.Log("Writing new player names to dict");
            for (var i = 0; i < keys.Length; i++)
            {
                GameSessionManager.Instance.IDToPlayerName[keys[i]] = values[i].ToString();
            }

            Debug.Log("Finished writing player names to dict");
        }

        [ClientRpc]
        [SuppressMessage("ReSharper", "MemberCanBeMadeStatic.Global")]
        public void SendNewIDToIsPlayerAliveClientRpc(ulong[] keys, bool[] values)
        {
            if (IsHost) return;
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
        [SuppressMessage("ReSharper", "MemberCanBeMadeStatic.Local")]
        [SuppressMessage("ReSharper", "SuggestBaseTypeForParameter")]
        private void SendNewIDToAlibiClientRpc(ulong[] keys, FixedString128Bytes[] values)
        {
            if (IsHost) return;
            for (var i = 0; i < keys.Length; i++)
            {
                GameSessionManager.Instance.IDToAlibi[keys[i]] = values[i].ToString();
            }
        }

        [ClientRpc]
        [SuppressMessage("ReSharper", "UnusedParameter.Local")]
        [SuppressMessage("ReSharper", "MemberCanBeMadeStatic.Local")]
        [SuppressMessage("ReSharper", "SuggestBaseTypeForParameter")]
        private void SendNewMafiaIDToVotedForIDClientRpc(ulong[] keys, ulong[] values,
            ClientRpcParams clientRpcParams = default)
        {
            if (IsHost) return;
            for (var i = 0; i < keys.Length; i++)
            {
                GameSessionManager.Instance.MafiaIDToVotedForID[keys[i]] = values[i];
            }
        }

        [ClientRpc]
        [SuppressMessage("ReSharper", "MemberCanBeMadeStatic.Local")]
        private void SendLastWordsClientRpc(string lastWords)
        {
            if (IsHost) return;
            GameSessionManager.Instance.LastWords = lastWords;
        }

        [ClientRpc]
        [SuppressMessage("ReSharper", "MemberCanBeMadeStatic.Global")]
        public void SendLastKilledNameClientRpc(string lastKilledName)
        {
            if (IsHost) return;
            GameSessionManager.Instance.LastKilledName = lastKilledName;
        }

        [ClientRpc]
        [SuppressMessage("ReSharper", "MemberCanBeMadeStatic.Global")]
        public void ClearDataFromLastNightVotingClientRpc()
        {
            if (IsHost) return;
            GameSessionManager.Instance.MafiaIDToVotedForID.Clear();
            GameSessionManager.Instance.DoctorIDToVotedForID.Clear();
            GameSessionManager.Instance.IDToAlibi.Clear();
            GameSessionManager.Instance.LastKilledName = "";
            GameSessionManager.Instance.CurrentNightResidentsQuestion = "";
            GameSessionManager.Instance.CurrentNightResidentsAnswerOptions.Clear();
            GameSessionManager.Instance.NightResidentsPollChosenAnswer = "";
        }

        [ClientRpc]
        [SuppressMessage("ReSharper", "MemberCanBeMadeStatic.Global")]
        public void ClearDataFromLastDayVotingClientRpc()
        {
            if (IsHost) return;
            GameSessionManager.Instance.IDToVotedForID.Clear();
            GameSessionManager.Instance.LastKilledName = "";
            GameSessionManager.Instance.LastWords = "";
        }

        [ClientRpc]
        [SuppressMessage("ReSharper", "MemberCanBeMadeStatic.Global")]
        public void SendNightResidentsPollChosenAnswerClientRpc(string answer)
        {
            if (IsHost) return;
            GameSessionManager.Instance.NightResidentsPollChosenAnswer = answer;
        }

        [ClientRpc]
        [SuppressMessage("ReSharper", "MemberCanBeMadeStatic.Global")]
        public void SendNightResidentsQuestionClientRpc(string question)
        {
            if (IsHost) return;
            GameSessionManager.Instance.CurrentNightResidentsQuestion = question;
        }

        [ClientRpc]
        [SuppressMessage("ReSharper", "MemberCanBeMadeStatic.Global")]
        [SuppressMessage("ReSharper", "ParameterTypeCanBeEnumerable.Global")]
        public void SendNightResidentsOptionsClientRpc(FixedString64Bytes[] options)
        {
            if (IsHost) return;
            var reconvertedOptions = options.Select(v => v.ToString()).ToList();
            GameSessionManager.Instance.CurrentNightResidentsAnswerOptions = reconvertedOptions;
        }

        [ClientRpc]
        public void SendNarratorCommentClientRpc(FixedString64Bytes comment)
        {
            if (IsHost) return;
            GameSessionManager.Instance.NarratorComment = comment.ToString();
        }

        [ClientRpc]
        public void BeginDayForClientsClientRpc()
        {
            if (IsHost) return;
            OnDayBegan?.Invoke();
        }

        [ClientRpc]
        public void BeginNightForClientsClientRpc()
        {
            if (IsHost) return;
            OnNightBegan?.Invoke();
        }
    }
}