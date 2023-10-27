using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DataStorage;
using TMPro;
using UI;
using Unity.Services.Authentication;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.UI;

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

        public TMP_InputField codeInputField;
        public TMP_InputField townName;
        public TMP_InputField maxPlayers;
        public TMP_InputField inputPlayerName;
        public GameObject lobbyButtonsParent;

        private static LobbyManager _instance;
        private Lobby _hostLobby;
        private Lobby _joinedLobby;
        private bool _isPrivate = true;
        private float _lastLobbyServiceCall;
        private string _lobbyToJoinID;
        private const float LobbyPollPeriod = 1.1f;
        private readonly List<GameObject> _lobbyButtons = new();

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
            foreach (var lobbyButtonTransform in lobbyButtonsParent.GetComponentsInChildren<Transform>(true))
            {
                if (!lobbyButtonTransform.gameObject.name.Contains("Lobby")) continue;
                _lobbyButtons.Add(lobbyButtonTransform.gameObject);
            }

            InvokeRepeating(nameof(HandleLobbyHeartbeat), 0f, 15f);
            InvokeRepeating(nameof(HandleLobbyPollForUpdates), 0f, 0.1f);
        }

        private async void HandleLobbyHeartbeat()
        {
            if (_hostLobby == null) return;
            try
            {
                _lastLobbyServiceCall = Time.time;
                await LobbyService.Instance.SendHeartbeatPingAsync(_hostLobby.Id);
            }
            catch (LobbyServiceException e)
            {
                Debug.Log(e);
            }
        }

        private async void HandleLobbyPollForUpdates()
        {
            if (_joinedLobby == null) return;
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

            if (_joinedLobby.Data[PpKeys.KeyStartGame].Value == "0") return;
            if (IsLobbyHost()) return;
            var relayCode = _joinedLobby.Data[PpKeys.KeyStartGame].Value;
            _joinedLobby = null;
            PlayerPrefs.SetString(PpKeys.KeyStartGame, relayCode);
            PlayerPrefs.SetInt(PpKeys.KeyIsHost, 0);
            PlayerPrefs.Save();
            SceneChanger.ChangeToGameScene();
        }

        private bool IsLobbyHost()
        {
            return AuthenticationService.Instance.PlayerId == _joinedLobby.HostId;
        }

        public void CreateLobby()
        {
            var maxPlayersInt = 5;
            try
            {
                maxPlayersInt = int.Parse(maxPlayers.text);
            }
            catch (Exception)
            {
                Debug.Log("Can't convert to int");
            }

            CreateLobbyAsync("Narrator", townName.text, maxPlayersInt, _isPrivate, "");
        }


        private async void CreateLobbyAsync(
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

        public void SetPublic()
        {
            _isPrivate = false;
        }

        public void SetPrivate()
        {
            _isPrivate = true;
        }

        public async void AssignLobbiesToButtons()
        {
            var lobbies = await GetLobbiesList();
            for (var i = 0; i < lobbies.Count; i++)
            {
                foreach (var lobbyButtonChild in _lobbyButtons[i].GetComponentsInChildren<TextMeshProUGUI>(true))
                {
                    lobbyButtonChild.text = lobbyButtonChild.gameObject.name switch
                    {
                        "CityName" => lobbies[i].Name,
                        "Population" => $"POPULATION {lobbies[i].Players.Count} / {lobbies[i].MaxPlayers}",
                        _ => lobbyButtonChild.text
                    };
                }

                var lobbyId = lobbies[i].Id;
                _lobbyButtons[i].GetComponent<Button>().onClick
                    .AddListener(() => HandleJoinLobbyClicked(lobbyId));
                _lobbyButtons[i].SetActive(true);
            }
        }

        private void HandleJoinLobbyClicked(string lobbyID)
        {
            _lobbyToJoinID = lobbyID;
            Debug.Log($"HandleJoinLobbyClicked: {_lobbyToJoinID}");
            ScreenChanger.Instance.ChangeToSetNameScreen();
        }

        public void PlayerNameEntered()
        {
            var lobbyID = _lobbyToJoinID;
            Debug.Log($"PlayerNameEntered: {lobbyID}");
            var nickname = inputPlayerName.text;
            JoinLobby(lobbyID: lobbyID, playerName: nickname);
            // ScreenChanger.Instance.ChangeToLobbyScreen //TODO
        }

        private static async Task<List<Lobby>> GetLobbiesList()
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

        private void JoinLobby(string lobbyID = null, string code = null, string playerName = "Anonymous")
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

            Debug.Log($"\nJoined '{_joinedLobby.Name}' lobby.");
        }

        private async void JoinLobbyByCode(string code, JoinLobbyByCodeOptions joinLobbyByCodeOptions)
        {
            try
            {
                _joinedLobby = await Lobbies.Instance.JoinLobbyByCodeAsync(code, joinLobbyByCodeOptions);
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