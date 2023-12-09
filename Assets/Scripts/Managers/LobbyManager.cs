using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DataStorage;
using Third_Party.Toast_UI.Scripts;
using UI;
using Unity.Services.Authentication;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using Random = System.Random;

namespace Managers
{
    public class LobbyManager : MonoBehaviour
    {
        public static LobbyManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<LobbyManager>();
                }

                return _instance;
            }
        }

        public bool IsCurrentlyInGame { get; set; }
        public event Action OnHostMigrated;

        private static LobbyManager _instance;
        private Lobby _hostLobby;
        private Lobby _joinedLobby;
        private string _playerName;
        private int _lastPlayersCount;
        private float _lastLobbyServiceCall;
        private float _lastHeartbeatSent;
        private bool _polling;
        private bool _sendingHeartbeat;
        private const float LobbyPollPeriod = 1.1f;
        private const float HeartbeatPeriod = 15f;
        private const string PlayerName = "PlayerName";

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

        private void Start()
        {
            InvokeRepeating(nameof(HandleLobbyHeartbeatAndHostLobbyPoll), 0f, 0.1f);
            InvokeRepeating(nameof(HandleLobbyPollForUpdates), 0f, 0.1f);
        }

        private async void HandleLobbyHeartbeatAndHostLobbyPoll()
        {
            if (_hostLobby == null || _joinedLobby == null) return;
            if (!IsLobbyHost()) return;
            if (Time.time - _lastLobbyServiceCall < LobbyPollPeriod) return;
            if (Time.time - _lastHeartbeatSent < HeartbeatPeriod)
            {
                if (_polling) return;
                try
                {
                    _lastLobbyServiceCall = Time.time;
                    _polling = true;
                    if (_hostLobby == null || _joinedLobby == null) return;
                    var lobby = await LobbyService.Instance.GetLobbyAsync(_joinedLobby.Id);
                    _joinedLobby = lobby;
                    _hostLobby = lobby;
                    _polling = false;
                }
                catch (LobbyServiceException e)
                {
                    if (e.Reason == LobbyExceptionReason.LobbyNotFound)
                    {
                        _joinedLobby = null;
                        _hostLobby = null;
                    }

                    if (e.Reason != LobbyExceptionReason.RateLimited)
                    {
                        Debug.LogError(e);
                    }

                    _polling = false;
                }
            }
            else
            {
                if (_sendingHeartbeat) return;
                try
                {
                    _lastLobbyServiceCall = Time.time;
                    _lastHeartbeatSent = Time.time;
                    _sendingHeartbeat = true;
                    if (_hostLobby == null || _joinedLobby == null) return;
                    await LobbyService.Instance.SendHeartbeatPingAsync(_hostLobby.Id);
                    _sendingHeartbeat = false;
                }
                catch (LobbyServiceException e)
                {
                    if (e.Reason == LobbyExceptionReason.LobbyNotFound)
                    {
                        _joinedLobby = null;
                        _hostLobby = null;
                    }

                    if (e.Reason != LobbyExceptionReason.RateLimited)
                    {
                        Debug.LogError(e);
                    }

                    _sendingHeartbeat = false;
                }
            }
        }

        private async void HandleLobbyPollForUpdates()
        {
            if (_joinedLobby == null) return;
            CheckIfNewHost();
            if (IsLobbyHost()) return;
            if (Time.time - _lastLobbyServiceCall < LobbyPollPeriod) return;
            try
            {
                _lastLobbyServiceCall = Time.time;
                if (_joinedLobby == null) return;
                var lobby = await LobbyService.Instance.GetLobbyAsync(_joinedLobby.Id);
                _joinedLobby = lobby;
            }
            catch (LobbyServiceException e)
            {
                if (e.Reason == LobbyExceptionReason.LobbyNotFound)
                {
                    _joinedLobby = null;
                }

                if (e.Reason != LobbyExceptionReason.RateLimited)
                {
                    Debug.LogError(e);
                }

                return;
            }

            var relayCode = _joinedLobby.Data[PpKeys.KeyStartGame].Value;
            if (relayCode == "0") return;
            if (IsCurrentlyInGame) return;
            IsCurrentlyInGame = true;
            var playersNumber = _joinedLobby.Players.Count;
            SetPlayerPrefsForGameSession(relayCode, 0, playersNumber, _playerName);
            SceneChanger.ChangeToGameScene();
        }

        public bool IsLobbyHost()
        {
            if (_joinedLobby == null) return false;
            return AuthenticationService.Instance.PlayerId == _joinedLobby.HostId;
        }

        private void CheckIfNewHost()
        {
            if (_joinedLobby.Players.Count < _lastPlayersCount)
            {
                if (IsLobbyHost() && _hostLobby == null)
                {
                    _hostLobby = _joinedLobby;
                    Toast.Show("You are new lobby host");
                    OnHostMigrated?.Invoke();
                }
            }

            _lastPlayersCount = _joinedLobby.Players.Count;
        }

        public async Task<bool> CreateLobbyAsync(
            string playerName,
            string lobbyName,
            int maxPlayersInt,
            bool isPrivate,
            string cityName
        )
        {
            var player = new Player
            {
                Data = new Dictionary<string, PlayerDataObject>
                {
                    {
                        PlayerName,
                        new PlayerDataObject(
                            PlayerDataObject.VisibilityOptions.Member,
                            playerName
                        )
                    }
                }
            };
            var data = new Dictionary<string, DataObject>
            {
                {
                    "City",
                    new DataObject(
                        DataObject.VisibilityOptions.Public,
                        cityName
                    )
                },
                {
                    PpKeys.KeyStartGame,
                    new DataObject(
                        DataObject.VisibilityOptions.Member,
                        "0"
                    )
                }
            };
            PlayerPrefs.SetString(PpKeys.KeyStartGame, "0");
            PlayerPrefs.Save();
            var createLobbyOptions = new CreateLobbyOptions
            {
                IsPrivate = isPrivate,
                IsLocked = false,
                Player = player,
                Data = data
            };
            try
            {
                _hostLobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, maxPlayersInt, createLobbyOptions);
            }
            catch (LobbyServiceException e)
            {
                Debug.LogError(e);
                Toast.Show("Cannot create lobby. Try again.");
                return false;
            }

            _joinedLobby = _hostLobby; //Because creator automatically joins the lobby
            var message =
                $"\nLobby '{_hostLobby.Name}' Created. Max players: {_hostLobby.MaxPlayers}. Code: {_hostLobby.LobbyCode}. Private: {_hostLobby.IsPrivate}";
            Debug.Log(message);
            return true;
        }

        public static async Task<List<Lobby>> GetLobbiesList()
        {
            var filters = new List<QueryFilter>
            {
                new(QueryFilter.FieldOptions.AvailableSlots, "0", QueryFilter.OpOptions.GT),
                new(QueryFilter.FieldOptions.IsLocked, false.ToString(), QueryFilter.OpOptions.EQ)
            };
            var order = new List<QueryOrder>
            {
                new(false, QueryOrder.FieldOptions.Created)
            };
            var queryLobbiesOptions = new QueryLobbiesOptions
            {
                Count = 6,
                Filters = filters,
                Order = order
            };
            QueryResponse queryResponse;
            try
            {
                queryResponse = await Lobbies.Instance.QueryLobbiesAsync(queryLobbiesOptions);
            }
            catch (LobbyServiceException e)
            {
                Debug.LogError(e);
                Toast.Show("Cannot get lobbies. Try again.");
                return new List<Lobby>();
            }

            Debug.Log($"\nNum of lobbies: {queryResponse.Results.Count}");
            return queryResponse.Results;
        }

        public List<string> GetPlayersNamesInLobby()
        {
            return _joinedLobby == null
                ? new List<string>()
                : _joinedLobby.Players.Select(player => player.Data[PlayerName].Value).ToList();
        }

        public string GetLobbyName()
        {
            return _joinedLobby == null ? "" : _joinedLobby.Name;
        }

        public string GetLobbyCode()
        {
            return _joinedLobby == null ? "" : _joinedLobby.LobbyCode;
        }

        public int GetMaxPlayers()
        {
            return _joinedLobby?.MaxPlayers ?? 0;
        }

        public void JoinLobby(string lobbyID = null, string code = null, string playerName = "Anonymous")
        {
            if (playerName == "")
            {
                playerName = "Anonymous";
            }
            _playerName = playerName;
            var player = new Player
            {
                Data = new Dictionary<string, PlayerDataObject>
                {
                    {
                        PlayerName, new PlayerDataObject(
                            PlayerDataObject.VisibilityOptions.Member,
                            playerName
                        )
                    }
                }
            };

            if (lobbyID == null && code != null)
            {
                var joinLobbyByCodeOptions = new JoinLobbyByCodeOptions
                {
                    Player = player
                };
                JoinLobbyByCode(code, joinLobbyByCodeOptions);
            }
            else if (lobbyID != null && code == null)
            {
                var joinLobbyByIdOptions = new JoinLobbyByIdOptions
                {
                    Player = player
                };
                JoinLobbyByID(lobbyID, joinLobbyByIdOptions);
            }
        }

        private async void JoinLobbyByCode(string code, JoinLobbyByCodeOptions joinLobbyByCodeOptions)
        {
            try
            {
                _joinedLobby = await Lobbies.Instance.JoinLobbyByCodeAsync(code, joinLobbyByCodeOptions);
                ChangePlayerNameIfTheSameIsPresent(joinLobbyByCodeOptions.Player.Data[PlayerName].Value);
                Debug.Log($"\nJoined '{_joinedLobby.Name}' lobby.");
            }
            catch (LobbyServiceException e)
            {
                Toast.Show("Cannot join to lobby. Try again.");
                Debug.LogError(e);
            }
        }

        private async void JoinLobbyByID(string lobbyID, JoinLobbyByIdOptions joinLobbyByIdOptions)
        {
            try
            {
                _joinedLobby = await Lobbies.Instance.JoinLobbyByIdAsync(lobbyID, joinLobbyByIdOptions);
                ChangePlayerNameIfTheSameIsPresent(joinLobbyByIdOptions.Player.Data[PlayerName].Value);
                Debug.Log($"\nJoined '{_joinedLobby.Name}' lobby.");
            }
            catch (LobbyServiceException e)
            {
                Toast.Show("Cannot join to lobby. Try again.");
                Debug.LogError(e);
            }
        }

        private async void ChangePlayerNameIfTheSameIsPresent(string currentName)
        {
            if (_joinedLobby.Players.Count(p =>
                    p.Data.ContainsKey(PlayerName) &&
                    p.Data[PlayerName].Value == currentName) <= 1) return;

            var randomNumber = new Random().Next(100);
            var newName = $"{currentName}({randomNumber})";
            _playerName = newName;
            FindObjectOfType<PlayerLobbyController>(true).SetWelcomePrompt(newName);
            var updatePlayerOptions = new UpdatePlayerOptions
            {
                Data = new Dictionary<string, PlayerDataObject>
                {
                    {
                        PlayerName, new PlayerDataObject(
                            PlayerDataObject.VisibilityOptions.Member,
                            newName
                        )
                    }
                }
            };
            try
            {
                var playerID = AuthenticationService.Instance.PlayerId;
                await LobbyService.Instance.UpdatePlayerAsync(_joinedLobby.Id, playerID, updatePlayerOptions);
            }
            catch (LobbyServiceException e)
            {
                Toast.Show("Error with updating the lobby.");
                Debug.LogError(e);
            }
        }

        public bool IsLobbyJoined()
        {
            return _joinedLobby != null;
        }

        public List<Player> GetPlayersListInLobby()
        {
            return _joinedLobby.Players;
        }

        public void LeaveLobby()
        {
            if (GetPlayersListInLobby().Count <= 1)
            {
                HandleDeleteLobby();
            }
            else
            {
                HandleLeaveLobby();
            }
        }

        private async void HandleLeaveLobby()
        {
            Debug.Log("Leaving lobby");
            try
            {
                await LobbyService.Instance.RemovePlayerAsync(_joinedLobby.Id,
                    AuthenticationService.Instance.PlayerId);
                _hostLobby = null;
                _joinedLobby = null;
            }
            catch (LobbyServiceException e)
            {
                Debug.LogError(e);
                _hostLobby = null;
                _joinedLobby = null;
            }

            Debug.Log($"Lobby state: {_joinedLobby}");
        }

        private async void HandleDeleteLobby()
        {
            if (!IsLobbyHost()) return;
            Debug.Log("Deleting lobby");
            try
            {
                await LobbyService.Instance.DeleteLobbyAsync(_joinedLobby.Id);
                _hostLobby = null;
                _joinedLobby = null;
            }
            catch (LobbyServiceException e)
            {
                Debug.LogError(e);
                _hostLobby = null;
                _joinedLobby = null;
            }

            Debug.Log($"Lobby state: {_joinedLobby}");
        }

        public async Task<bool> StartGame()
        {
            if (_joinedLobby == null) return false;
            if (!IsLobbyHost()) return false;
            var maxClientsNum = _hostLobby.Players.Count;
            if (maxClientsNum < 5)
            {
                Toast.Show($"Cannot start the game. {maxClientsNum} out of 5 players required.");
                return false;
            }

            string relayCode;
            try
            {
                relayCode = await RelayManager.Instance.GetRelayCode(maxClientsNum);
            }
            catch (Exception e)
            {
                Toast.Show("Cannot start the game. Try again.");
                Debug.LogError(e);
                return false;
            }

            if (relayCode == ErrorCodes.JoinRelayErrorCode)
            {
                Toast.Show("Cannot start the game. Try again.");
                return false;
            }

            SetPlayerPrefsForGameSession(relayCode, 1, _hostLobby.Players.Count - 1, Roles.Narrator);
            var data = new Dictionary<string, DataObject>
            {
                {
                    PpKeys.KeyStartGame,
                    new DataObject(
                        DataObject.VisibilityOptions.Member,
                        relayCode
                    )
                }
            };
            var updateLobbyOption = new UpdateLobbyOptions
            {
                Data = data,
                IsLocked = true
            };
            try
            {
                await Lobbies.Instance.UpdateLobbyAsync(_joinedLobby.Id, updateLobbyOption);
            }
            catch (LobbyServiceException e)
            {
                Debug.LogError(e);
                Toast.Show("Cannot start the game. Try again.");
                return false;
            }

            SceneChanger.ChangeToGameScene();
            return true;
        }

        public async Task<bool> EndGame()
        {
            if (_joinedLobby == null) return false;
            if (!IsLobbyHost()) return false;
            SetPlayerPrefsForGameSession("0", 1, _hostLobby.Players.Count - 1, Roles.Narrator);
            var data = new Dictionary<string, DataObject>
            {
                {
                    PpKeys.KeyStartGame,
                    new DataObject(
                        DataObject.VisibilityOptions.Member,
                        "0"
                    )
                }
            };
            var updateLobbyOption = new UpdateLobbyOptions
            {
                Data = data,
                IsLocked = true
            };
            try
            {
                await Lobbies.Instance.UpdateLobbyAsync(_joinedLobby.Id, updateLobbyOption);
            }
            catch (LobbyServiceException e)
            {
                Debug.LogError(e);
                return false;
            }

            return true;
        }

        private static void SetPlayerPrefsForGameSession(
            string relayCode,
            int isHost,
            int playersNumber,
            string playerName)
        {
            PlayerPrefs.SetString(PpKeys.KeyStartGame, relayCode);
            PlayerPrefs.SetInt(PpKeys.KeyIsHost, isHost);
            PlayerPrefs.SetInt(PpKeys.KeyPlayersNumber, playersNumber);
            PlayerPrefs.SetString(PpKeys.KeyPlayerName, playerName);
            PlayerPrefs.Save();
        }
    }
}