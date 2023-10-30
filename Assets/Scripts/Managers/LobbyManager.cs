using System.Collections.Generic;
using System.Threading.Tasks;
using DataStorage;
using Unity.Services.Authentication;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;

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

        private static LobbyManager _instance;
        private Lobby _hostLobby;
        private Lobby _joinedLobby;
        private int _lastPlayersCount;
        private float _lastLobbyServiceCall;
        private float _lastHeartbeatSent;
        private bool _polling;
        private bool _sendingHeartbeat;
        private const float LobbyPollPeriod = 1.1f;
        private const float HeartbeatPeriod = 15f;

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
            if (_hostLobby == null) return;
            if (!IsLobbyHost()) return;
            if (Time.time - _lastLobbyServiceCall < LobbyPollPeriod) return;
            if (Time.time - _lastHeartbeatSent < HeartbeatPeriod)
            {
                if (_polling) return;
                try
                {
                    _lastLobbyServiceCall = Time.time;
                    _polling = true;
                    _joinedLobby = await LobbyService.Instance.GetLobbyAsync(_joinedLobby.Id);
                    _polling = false;
                }
                catch (LobbyServiceException e)
                {
                    Debug.Log(e);
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
                    await LobbyService.Instance.SendHeartbeatPingAsync(_hostLobby.Id);
                    _sendingHeartbeat = false;
                }
                catch (LobbyServiceException e)
                {
                    Debug.Log(e);
                }
            }
        }

        private async void HandleLobbyPollForUpdates()
        {
            if (_joinedLobby == null) return;
            if (IsLobbyHost()) return;
            if (Time.time - _lastLobbyServiceCall < LobbyPollPeriod) return;
            try
            {
                _lastLobbyServiceCall = Time.time;
                _joinedLobby = await LobbyService.Instance.GetLobbyAsync(_joinedLobby.Id);
            }
            catch (LobbyServiceException e)
            {
                Debug.Log(e);
                return;
            }

            CheckIfNewHost();
            if (_joinedLobby.Data[PpKeys.KeyStartGame].Value == "0") return;
            var relayCode = _joinedLobby.Data[PpKeys.KeyStartGame].Value;
            _joinedLobby = null;
            PlayerPrefs.SetString(PpKeys.KeyStartGame, relayCode);
            PlayerPrefs.SetInt(PpKeys.KeyIsHost, 0);
            PlayerPrefs.Save();
            SceneChanger.ChangeToGameScene();
        }

        public bool IsLobbyHost()
        {
            return AuthenticationService.Instance.PlayerId == _joinedLobby.HostId;
        }

        private void CheckIfNewHost()
        {
            if (_joinedLobby.Players.Count < _lastPlayersCount)
            {
                if (IsLobbyHost() && _hostLobby == null)
                {
                    _hostLobby = _joinedLobby;
                }
            }

            _lastPlayersCount = _joinedLobby.Players.Count;
        }

        public async void CreateLobbyAsync(
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
                        "PlayerName",
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
                Player = player,
                Data = data
            };
            try
            {
                _hostLobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, maxPlayersInt, createLobbyOptions);
            }
            catch (LobbyServiceException e)
            {
                Debug.Log(e);
                return;
            }

            _joinedLobby = _hostLobby; //Because creator automatically joins the lobby
            var message =
                $"\nLobby '{_hostLobby.Name}' Created. Max players: {_hostLobby.MaxPlayers}. Code: {_hostLobby.LobbyCode}. Private: {_hostLobby.IsPrivate}";
            Debug.Log(message);
        }

        public static async Task<List<Lobby>> GetLobbiesList()
        {
            var filters = new List<QueryFilter>
            {
                new(QueryFilter.FieldOptions.AvailableSlots, "0", QueryFilter.OpOptions.GT)
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
                Debug.Log(e);
                return new List<Lobby>();
            }

            Debug.Log($"\nNum of lobbies: {queryResponse.Results.Count}");
            return queryResponse.Results;
        }

        public void JoinLobby(string lobbyID = null, string code = null, string playerName = "Anonymous")
        {
            var player = new Player
            {
                Data = new Dictionary<string, PlayerDataObject>
                {
                    {
                        "PlayerName", new PlayerDataObject(
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
                Debug.Log($"\nJoined '{_joinedLobby.Name}' lobby.");
            }
            catch (LobbyServiceException e)
            {
                Debug.Log(e);
            }
        }

        private async void JoinLobbyByID(string lobbyID, JoinLobbyByIdOptions joinLobbyByIdOptions)
        {
            try
            {
                _joinedLobby = await Lobbies.Instance.JoinLobbyByIdAsync(lobbyID, joinLobbyByIdOptions);
                Debug.Log($"\nJoined '{_joinedLobby.Name}' lobby.");
            }
            catch (LobbyServiceException e)
            {
                Debug.Log(e);
            }
        }

        public List<Player> GetPlayersListInLobby()
        {
            return _joinedLobby.Players;
        }

        public async void LeaveLobby()
        {
            try
            {
                await LobbyService.Instance.RemovePlayerAsync(_joinedLobby.Id,
                    AuthenticationService.Instance.PlayerId);
                _hostLobby = null;
                _joinedLobby = null;
            }
            catch (LobbyServiceException e)
            {
                Debug.Log(e);
            }
        }

        public async void DeleteLobby()
        {
            if (!IsLobbyHost()) return;
            try
            {
                await LobbyService.Instance.DeleteLobbyAsync(_joinedLobby.Id);
                _hostLobby = null;
                _joinedLobby = null;
            }
            catch (LobbyServiceException e)
            {
                Debug.Log(e);
            }
        }

        public async void StartGame()
        {
            if (!IsLobbyHost()) return;
            var relayCode = "0";
            var maxClientsNum = _hostLobby.Players.Count - 1;
            try
            {
                relayCode = await RelayManager.Instance.GetRelayCode(maxClientsNum);
            }
            catch (LobbyServiceException e)
            {
                Debug.Log(e);
            }

            PlayerPrefs.SetString(PpKeys.KeyStartGame, relayCode);
            PlayerPrefs.SetInt(PpKeys.KeyIsHost, 1);
            PlayerPrefs.Save();
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
                Data = data
            };
            try
            {
                await Lobbies.Instance.UpdateLobbyAsync(_joinedLobby.Id, updateLobbyOption);
            }
            catch (LobbyServiceException e)
            {
                Debug.Log(e);
            }

            _joinedLobby = null;
            _hostLobby = null;
            SceneChanger.ChangeToGameScene();
        }
    }
}