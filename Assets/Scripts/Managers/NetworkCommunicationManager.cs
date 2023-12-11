using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using DataStorage;
using Third_Party.Toast_UI.Scripts;
using UI;
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

        public bool IsPlayerRoleAssigned { get; private set; }
        public event Action OnPlayerRoleAssigned;
        public event Action OnOneMafiaVoted;
        public event Action OnOneDoctorVoted;
        public event Action OnOneResidentDayVoted;
        public event Action OnDayBegan;
        public event Action OnEveningBegan;
        public event Action OnNightBegan;
        public event Action OnGameEnded;
        private int _counter;
        private int _playerCounter;

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
            NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;
            NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnectedForHost;
            NetworkManager.Singleton.OnTransportFailure += OnTransportFailure;
            OnPlayerRoleAssigned += () =>
            {
                NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
                NetworkManager.Singleton.OnClientConnectedCallback += OnClientReconnected;
                IsPlayerRoleAssigned = true;
            };
        }

        private void OnDisable()
        {
            UnsubscribeAllNetworkEvents();
        }

        private void UnsubscribeAllNetworkEvents()
        {
            IsPlayerRoleAssigned = false;
            NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnected;
            NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnectedForHost;
            NetworkManager.Singleton.OnTransportFailure -= OnTransportFailure;
            NetworkManager.Singleton.OnServerStarted -= OnHostStarted;
            NetworkManager.Singleton.OnServerStopped -= OnHostStopped;
            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientReconnected;
        }

        private void OnTransportFailure()
        {
            NetworkManager.Shutdown();
            GameSessionManager.Instance.ReconnectToGame();
            ScreenChanger.Instance.ChangeToErrorScreen();
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

        private void OnHostStopped(bool obj)
        {
            Debug.Log("Server stopped");
            EmergencyEndGameServerRpc();
            LeaveRelay();
        }

        private void OnClientConnected(ulong clientId)
        {
            if (IsHost)
            {
                _playerCounter++;
                return;
            }

            IsPlayerRoleAssigned = false;
            var oldID = PlayerData.ClientID;
            Debug.Log($"[NetworkCommunicationManager] OnClientConnected, ClientID: {clientId}");
            var playerName = PlayerPrefs.GetString(PpKeys.KeyPlayerName);
            PlayerData.Name = playerName;
            PlayerData.IsAlive = true;
            PlayerData.ClientID = clientId;
            GameSessionManager.Instance.CurrentTimeOfDay = TimeIsAManMadeSocialConstruct.Night;
            Debug.Log($"[NetworkCommunicationManager] Sending ServerRPC with player name {playerName}");
            AddClientNameToListServerRpc(playerName);
            ReassignPlayerDataAfterReconnectionServerRpc(oldID, PlayerData.ClientID);
        }

        private void OnClientReconnected(ulong clientId)
        {
            if (IsHost)
            {
                _playerCounter++;
                return;
            }

            var oldID = PlayerData.ClientID;
            PlayerData.ClientID = clientId;
            ReassignPlayerDataAfterReconnectionAfterRolesAssignedServerRpc(oldID, clientId);
        }

        private void OnClientDisconnected(ulong clientId)
        {
            if (IsHost) return;
            Debug.Log("On client disconnected called");
            LobbyManager.Instance.IsCurrentlyInGame = false;
            if (!PlayerData.IsAlive)
            {
                FinaliseLeavingRelay();
            }
            else
            {
                Toast.Show("You were disconnected. Trying to reconnect.");
            }
        }

        private void OnClientDisconnectedForHost(ulong clientId)
        {
            if (!IsHost) return;
            if (GameSessionManager.Instance.IDToIsPlayerAlive.TryGetValue(clientId, out var isAlive))
            {
                if (isAlive)
                {
                    _playerCounter--;
                }
            }

            InvokeRepeating(nameof(EmergencyGoToMainMenuIfApplicable), 0f, 1f);
        }

        private void EmergencyGoToMainMenuIfApplicable()
        {
            if (PlayerPrefs.GetInt(PpKeys.KeyPlayersNumber) + 1 <= _playerCounter)
            {
                CancelInvoke(nameof(EmergencyGoToMainMenuIfApplicable));
                _counter = 0;
                return;
            }

            switch (_counter)
            {
                case >= 20:
                    CancelInvoke(nameof(EmergencyGoToMainMenuIfApplicable));
                    GameSessionManager.Instance.EmergencyEndGame();
                    return;
                case 10:
                    Toast.Show("Lost one citizen. Looking for them...");
                    break;
            }

            _counter += 1;
        }

        public static bool StartHost()
        {
            return NetworkManager.Singleton.StartHost();
        }

        public static bool StartClient()
        {
            return NetworkManager.Singleton.StartClient();
        }

        public void LeaveRelay()
        {
            PlayerData.IsAlive = false;
            LobbyManager.Instance.IsCurrentlyInGame = false;
            KillMeServerRpc();
            if (NetworkManager.Singleton.IsConnectedClient)
            {
                NetworkManager.Singleton.Shutdown();
            }

            FinaliseLeavingRelay();
        }

        private static async void FinaliseLeavingRelay()
        {
            LobbyManager.Instance.IsGameEnded = true;
            GameSessionManager.Instance.ClearAllDataForEndGame();
            if (LobbyManager.Instance.GetLobbyName() != "")
            {
                await LobbyManager.Instance.LeaveLobby();
            }

            SceneChanger.ChangeToMainScene();
        }

        private void SendRolesToClients()
        {
            GameSessionManager.Instance.OnPlayersAssignedToRoles -= SendRolesToClients;
            var keys = GameSessionManager.Instance.IDToRole.Keys.ToArray();
            var values = GameSessionManager.Instance.IDToRole.Values
                .Select(value => new FixedString32Bytes(value))
                .ToArray();
            OnPlayerRoleAssigned?.Invoke();
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
            GameSessionManager.Instance.IDToPlayerName[rpcParams.Receive.SenderClientId] = playerName;
            var keys = GameSessionManager.Instance.IDToPlayerName.Keys.ToArray();
            var values = GameSessionManager.Instance.IDToPlayerName.Values
                .Select(value => new FixedString32Bytes(value))
                .ToArray();
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
            OnOneResidentDayVoted?.Invoke();
        }

        [ServerRpc(RequireOwnership = false)]
        public void MafiaVoteForServerRpc(ulong votedForID, ServerRpcParams rpcParams = default)
        {
            GameSessionManager.Instance.MafiaIDToVotedForID[rpcParams.Receive.SenderClientId] = votedForID;
            var keys = GameSessionManager.Instance.MafiaIDToVotedForID.Keys.ToArray();
            var values = GameSessionManager.Instance.MafiaIDToVotedForID.Values.ToArray();
            var mafiaIDs = GameSessionManager.Instance.GetAlivePlayersIDs()
                .Where(id => GameSessionManager.Instance.IDToRole[id] == Roles.Mafia).ToList();
            var clientRpcParams = new ClientRpcParams
            {
                Send = new ClientRpcSendParams
                {
                    TargetClientIds = mafiaIDs
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

        [ServerRpc(RequireOwnership = false)]
        [SuppressMessage("ReSharper", "MemberCanBeMadeStatic.Local")]
        private void ReassignPlayerDataAfterReconnectionServerRpc(ulong oldId, ulong newId)
        {
            if (oldId != ulong.MaxValue)
            {
                GameSessionManager.Instance.IDToPlayerName.Remove(oldId);
                GameSessionManager.Instance.IDToIsPlayerAlive.Remove(oldId);
            }

            GameSessionManager.Instance.OnNewClientConnected(newId);
        }

        [ServerRpc(RequireOwnership = false)]
        [SuppressMessage("ReSharper", "MemberCanBeMadeStatic.Local")]
        private void ReassignPlayerDataAfterReconnectionAfterRolesAssignedServerRpc(ulong oldId, ulong newId)
        {
            // ROLES
            var role = GameSessionManager.Instance.IDToRole[oldId];
            GameSessionManager.Instance.IDToRole.Remove(oldId);
            GameSessionManager.Instance.IDToRole[newId] = role;
            var keysForRoles = GameSessionManager.Instance.IDToRole.Keys.ToArray();
            var valuesForRoles = GameSessionManager.Instance.IDToRole.Values
                .Select(value => new FixedString32Bytes(value))
                .ToArray();
            OnPlayerRoleAssigned?.Invoke();
            SendNewIDToRoleClientRpc(keysForRoles, valuesForRoles);

            // NAMES
            var playerName = GameSessionManager.Instance.IDToPlayerName[oldId];
            GameSessionManager.Instance.IDToPlayerName.Remove(oldId);
            GameSessionManager.Instance.IDToPlayerName[newId] = playerName;
            var keysForNames = GameSessionManager.Instance.IDToPlayerName.Keys.ToArray();
            var valuesForNames = GameSessionManager.Instance.IDToPlayerName.Values
                .Select(value => new FixedString32Bytes(value))
                .ToArray();
            SendNewIDToPlayerNameClientRpc(keysForNames, valuesForNames);

            // IS ALIVES
            var isAlive = GameSessionManager.Instance.IDToIsPlayerAlive[oldId];
            GameSessionManager.Instance.IDToIsPlayerAlive.Remove(oldId);
            GameSessionManager.Instance.IDToIsPlayerAlive[newId] = isAlive;
            var keysForIsPlayerAlive = GameSessionManager.Instance.IDToIsPlayerAlive.Keys.ToArray();
            var valuesForIsPlayerAlive = GameSessionManager.Instance.IDToIsPlayerAlive.Values.ToArray();
            SendNewIDToIsPlayerAliveClientRpc(keysForIsPlayerAlive, valuesForIsPlayerAlive);

            // ALIBIS
            // ReSharper disable once CanSimplifyDictionaryRemovingWithSingleCall
            if (GameSessionManager.Instance.IDToAlibi.TryGetValue(oldId, out var alibi))
            {
                GameSessionManager.Instance.IDToAlibi.Remove(oldId);
                GameSessionManager.Instance.IDToAlibi[newId] = alibi;
                var keysForAlibis = GameSessionManager.Instance.IDToAlibi.Keys.ToArray();
                var valuesForAlibis = GameSessionManager.Instance.IDToAlibi.Values
                    .Select(value => new FixedString128Bytes(value))
                    .ToArray();
                SendNewIDToAlibiClientRpc(keysForAlibis, valuesForAlibis);
            }

            // ROLES VOTES
            switch (role)
            {
                case Roles.Mafia:
                    // ReSharper disable once CanSimplifyDictionaryRemovingWithSingleCall
                    if (GameSessionManager.Instance.MafiaIDToVotedForID.TryGetValue(oldId, out var mafiaVotedForID))
                    {
                        GameSessionManager.Instance.MafiaIDToVotedForID.Remove(oldId);
                        GameSessionManager.Instance.MafiaIDToVotedForID[newId] = mafiaVotedForID;
                        var keysForMafiaVotedFor = GameSessionManager.Instance.MafiaIDToVotedForID.Keys.ToArray();
                        var valuesForMafiaVotedFor = GameSessionManager.Instance.MafiaIDToVotedForID.Values.ToArray();
                        var mafiaIDs = GameSessionManager.Instance.GetAlivePlayersIDs()
                            .Where(id => GameSessionManager.Instance.IDToRole[id] == Roles.Mafia).ToList();
                        var clientRpcParamsWithMafiaIDsOnly = new ClientRpcParams
                        {
                            Send = new ClientRpcSendParams
                            {
                                TargetClientIds = mafiaIDs
                            }
                        };
                        SendNewMafiaIDToVotedForIDClientRpc(keysForMafiaVotedFor, valuesForMafiaVotedFor,
                            clientRpcParamsWithMafiaIDsOnly);
                    }

                    break;
                case Roles.Doctor:
                    // ReSharper disable once CanSimplifyDictionaryRemovingWithSingleCall
                    if (GameSessionManager.Instance.DoctorIDToVotedForID.TryGetValue(oldId,
                            out var doctorVotedForID))
                    {
                        GameSessionManager.Instance.DoctorIDToVotedForID.Remove(oldId);
                        GameSessionManager.Instance.DoctorIDToVotedForID[newId] = doctorVotedForID;
                    }

                    break;
            }

            // ReSharper disable once CanSimplifyDictionaryRemovingWithSingleCall
            if (GameSessionManager.Instance.IDToVotedForID.TryGetValue(oldId, out var votedForID))
            {
                GameSessionManager.Instance.IDToVotedForID.Remove(oldId);
                GameSessionManager.Instance.IDToVotedForID[newId] = votedForID;
            }

            // ReSharper disable once CanSimplifyDictionaryRemovingWithSingleCall
            if (GameSessionManager.Instance.ResidentIDToVotedForOption.TryGetValue(oldId,
                    out var votedForOption))
            {
                GameSessionManager.Instance.ResidentIDToVotedForOption.Remove(oldId);
                GameSessionManager.Instance.ResidentIDToVotedForOption[newId] = votedForOption;
            }

            SendLastWordsClientRpc(GameSessionManager.Instance.LastWords);
            SendLastKilledNameClientRpc(GameSessionManager.Instance.LastKilledName);
            SendNightResidentsPollChosenAnswerClientRpc(GameSessionManager.Instance.NightResidentsPollChosenAnswer);
            var options = GameSessionManager.Instance.CurrentNightResidentsAnswerOptions
                .Select(value => new FixedString64Bytes(value))
                .ToArray();
            SendNightResidentsOptionsClientRpc(options);
            SendNarratorCommentClientRpc(GameSessionManager.Instance.NarratorComment);
            SetTimeForClientsClientRpc(GameSessionManager.Instance.CurrentTimeOfDay);
        }

        [ServerRpc(RequireOwnership = false)]
        private void KillMeServerRpc(ServerRpcParams rpcParams = default)
        {
            var prevPlayerNumber = PlayerPrefs.GetInt(PpKeys.KeyPlayersNumber);
            var nextPlayerNumber = prevPlayerNumber - 1;
            PlayerPrefs.SetInt(PpKeys.KeyPlayersNumber, nextPlayerNumber);
            PlayerPrefs.Save();
            GameSessionManager.Instance.IDToIsPlayerAlive[rpcParams.Receive.SenderClientId] = false;
            var keys = GameSessionManager.Instance.IDToIsPlayerAlive.Keys.ToArray();
            var values = GameSessionManager.Instance.IDToIsPlayerAlive.Values.ToArray();
            SendNewIDToIsPlayerAliveClientRpc(keys, values);
        }

        [ServerRpc(RequireOwnership = false)]
        public void EmergencyEndGameServerRpc()
        {
            GameSessionManager.Instance.EmergencyEndGame();
        }

        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // CLIENT RPCs //////////////////////////////////////////////////////////////////////////////////////////////////////
        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        [ClientRpc]
        [SuppressMessage("ReSharper", "SuggestBaseTypeForParameter")]
        private void SendNewIDToRoleClientRpc(ulong[] keys, FixedString32Bytes[] values)
        {
            if (IsHost) return;
            GameSessionManager.Instance.IDToRole.Clear();
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
            GameSessionManager.Instance.IDToPlayerName.Clear();
            for (var i = 0; i < keys.Length; i++)
            {
                GameSessionManager.Instance.IDToPlayerName[keys[i]] = values[i].ToString();
            }
        }

        [ClientRpc]
        [SuppressMessage("ReSharper", "MemberCanBeMadeStatic.Global")]
        public void SendNewIDToIsPlayerAliveClientRpc(ulong[] keys, bool[] values)
        {
            if (IsHost) return;
            GameSessionManager.Instance.IDToIsPlayerAlive.Clear();
            for (var i = 0; i < keys.Length; i++)
            {
                GameSessionManager.Instance.IDToIsPlayerAlive[keys[i]] = values[i];
            }

            if (GameSessionManager.Instance.IDToIsPlayerAlive.TryGetValue(PlayerData.ClientID, out var isAlive))
            {
                PlayerData.IsAlive = isAlive;
            }
        }

        [ClientRpc]
        [SuppressMessage("ReSharper", "MemberCanBeMadeStatic.Local")]
        [SuppressMessage("ReSharper", "SuggestBaseTypeForParameter")]
        private void SendNewIDToAlibiClientRpc(ulong[] keys, FixedString128Bytes[] values)
        {
            if (IsHost) return;
            GameSessionManager.Instance.IDToAlibi.Clear();
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
            GameSessionManager.Instance.MafiaIDToVotedForID.Clear();
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
            GameSessionManager.Instance.CurrentTimeOfDay = TimeIsAManMadeSocialConstruct.Day;
            OnDayBegan?.Invoke();
        }

        [ClientRpc]
        public void BeginEveningForClientsClientRpc()
        {
            if (IsHost) return;
            GameSessionManager.Instance.CurrentTimeOfDay = TimeIsAManMadeSocialConstruct.Evening;
            OnEveningBegan?.Invoke();
        }

        [ClientRpc]
        public void BeginNightForClientsClientRpc()
        {
            if (IsHost) return;
            GameSessionManager.Instance.CurrentTimeOfDay = TimeIsAManMadeSocialConstruct.Night;
            OnNightBegan?.Invoke();
        }

        [ClientRpc]
        private void SetTimeForClientsClientRpc(string newTime)
        {
            if (IsHost) return;
            GameSessionManager.Instance.CurrentTimeOfDay = newTime;
        }

        [ClientRpc]
        public void EndGameForClientsClientRpc(string winnerRole)
        {
            if (IsHost) return;
            GameSessionManager.Instance.WinnerRole = winnerRole;
            OnGameEnded?.Invoke();
        }

        [ClientRpc]
        public void GoBackToLobbyClientRpc()
        {
            if (IsHost) return;
            LeaveRelay();
        }
    }
}