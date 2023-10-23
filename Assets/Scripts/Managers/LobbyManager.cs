using System.Collections.Generic;
using System.Threading.Tasks;
using DataStorage;
using TMPro;
using Unity.Services.Authentication;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;

namespace Managers
{
    public class LobbyManager : MonoBehaviour
    {
        public TextMeshProUGUI debug;
        public TMP_InputField codeInputField;

        private Lobby _hostLobby;
        private Lobby _joinedLobby;

        private void Start()
        {
            InvokeRepeating(nameof(HandleLobbyHeartbeat), 0f, 15f);
            InvokeRepeating(nameof(HandleLobbyPollForUpdates), 0f, 1.1f);
        }

        private async void HandleLobbyHeartbeat()
        {
            if (_hostLobby == null) return;
            await LobbyService.Instance.SendHeartbeatPingAsync(_hostLobby.Id);
        }

        private async void HandleLobbyPollForUpdates()
        {
            if (_joinedLobby == null) return;
            _joinedLobby = await LobbyService.Instance.GetLobbyAsync(_joinedLobby.Id);
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

        public async void CreateLobby(
            string playerName,
            string lobbyName,
            int maxPlayers,
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
                _hostLobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, maxPlayers, createLobbyOptions);
            }
            catch (LobbyServiceException e)
            {
                debug.text += $"\n{e}";
                return;
            }

            _joinedLobby = _hostLobby; //Because creator automatically joins the lobby
            debug.text +=
                $"\nLobby '{_hostLobby.Name}' Created. Max players: {_hostLobby.MaxPlayers}. Code: {_hostLobby.LobbyCode}";
        }

        public async Task<List<Lobby>> GetLobbiesList()
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
                Count = 25,
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
                debug.text += $"\n{e}";
                return new List<Lobby>();
            }

            debug.text += $"\nNum of lobbies: {queryResponse.Results.Count}";
            return queryResponse.Results;
        }

        public async void JoinLobbyByCode(string code, string playerName = "Anonymous")
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
            var joinLobbyByCodeOptions = new JoinLobbyByCodeOptions
            {
                Player = player
            };
            try
            {
                _joinedLobby = await Lobbies.Instance.JoinLobbyByCodeAsync(code, joinLobbyByCodeOptions);
            }
            catch (LobbyServiceException e)
            {
                debug.text += $"\n{e}";
                return;
            }

            debug.text += $"\nJoined '{_joinedLobby.Name}' lobby.";
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
                debug.text += $"\n{e}";
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
                debug.text += $"\n{e}";
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
                debug.text += $"\n{e}";
            }

            _joinedLobby = null;
            _hostLobby = null;
            SceneChanger.ChangeToGameScene();
        }
    }
}