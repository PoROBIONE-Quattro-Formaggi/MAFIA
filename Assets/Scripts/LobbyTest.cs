using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using Random = UnityEngine.Random;

public class LobbyTest : MonoBehaviour
{
    public TextMeshProUGUI debug;
    public TMP_InputField codeInputField;

    private Lobby _hostLobby;
    private Lobby _joinedLobby;
    private string _playerName;

    private async void Start()
    {
        await UnityServices.InitializeAsync();
        _playerName = "SomeNick" + Random.Range(1, 15);
        AuthenticationService.Instance.SignedIn += () => { debug.text += "\nSigned in as: " + _playerName; };
        await AuthenticationService.Instance.SignInAnonymouslyAsync();
        InvokeRepeating(nameof(HandleLobbyHeartbeat), 0f, 15f);
        InvokeRepeating(nameof(HandleLobbyPollForUpdates), 0f, 1.1f);
    }

    public void ClearConsole()
    {
        debug.text = "";
    }

    public async void CreateLobby()
    {
        try
        {
            const string lobbyName = "Name of the Lobby";
            const int maxPlayers = 4;
            var createLobbyOptions = new CreateLobbyOptions
            {
                IsPrivate = false,
                Player = new Player
                {
                    Data = new Dictionary<string, PlayerDataObject>
                    {
                        { "PlayerName", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, _playerName) }
                    }
                },
                Data = new Dictionary<string, DataObject>
                {
                    {
                        "City",
                        new DataObject(DataObject.VisibilityOptions.Public, "Wrocław")
                    }
                }
            };
            var lobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, maxPlayers, createLobbyOptions);
            _hostLobby = lobby;
            _joinedLobby = _hostLobby; //Because the creator automatically joins the lobby
            debug.text += $"\nLobby Created: {lobby.Name} {lobby.MaxPlayers} {lobby.LobbyCode}";
            PrintPlayers(lobby);
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }

    private async void HandleLobbyHeartbeat()
    {
        if (_hostLobby == null) return;
        await LobbyService.Instance.SendHeartbeatPingAsync(_hostLobby.Id);
    }

    private async void HandleLobbyPollForUpdates()
    {
        if (_joinedLobby == null) return;
        var lobby = await LobbyService.Instance.GetLobbyAsync(_joinedLobby.Id);
        _joinedLobby = lobby;
    }

    public async void ListLobbies()
    {
        try
        {
            var queryLobbiesOptions = new QueryLobbiesOptions
            {
                Count = 25,
                Filters = new List<QueryFilter>
                {
                    new(QueryFilter.FieldOptions.AvailableSlots, "0", QueryFilter.OpOptions.GT)
                },
                Order = new List<QueryOrder>
                {
                    new(false, QueryOrder.FieldOptions.Created)
                }
            };
            var queryResponse = await Lobbies.Instance.QueryLobbiesAsync(queryLobbiesOptions);
            debug.text += "\nLobbies found: " + queryResponse.Results.Count;
            foreach (var lobby in queryResponse.Results)
            {
                debug.text += $"\n{lobby.Name} {lobby.MaxPlayers}";
            }
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }

    public async void JoinLobbyByCode()
    {
        try
        {
            var joinLobbyByCodeOptions = new JoinLobbyByCodeOptions
            {
                Player = new Player
                {
                    Data = new Dictionary<string, PlayerDataObject>
                    {
                        { "PlayerName", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, _playerName) }
                    }
                }
            };
            var code = codeInputField.text;
            var lobby = await Lobbies.Instance.JoinLobbyByCodeAsync(code, joinLobbyByCodeOptions);
            _joinedLobby = lobby;
            PrintPlayers(_joinedLobby);
            debug.text += $"\nJoined lobby with code: {code}";
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }

    public async void QuickJoinLobby()
    {
        try
        {
            var lobby = await Lobbies.Instance.QuickJoinLobbyAsync();
            _joinedLobby = lobby;
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }

    private void PrintPlayers()
    {
        PrintPlayers(_joinedLobby);
    }

    private void PrintPlayers(Lobby lobby)
    {
        debug.text += $"\nPlayers in lobby: {lobby.Name}";
        foreach (var player in lobby.Players)
        {
            debug.text += $"\n{player.Id} {player.Data["PlayerName"].Value}";
        }
    }

    private async void UpdateLobbyCity(string city)
    {
        try
        {
            _hostLobby = await Lobbies.Instance.UpdateLobbyAsync(
                _hostLobby.Id,
                new UpdateLobbyOptions
                {
                    Data = new Dictionary<string, DataObject>
                    {
                        { "City", new DataObject(DataObject.VisibilityOptions.Public, city) }
                    }
                }
            );
            _joinedLobby = _hostLobby;
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }

    private async void UpdatePlayerName(string newPlayerName)
    {
        _playerName = newPlayerName;
        await LobbyService.Instance.UpdatePlayerAsync(_joinedLobby.Id, AuthenticationService.Instance.PlayerId,
            new UpdatePlayerOptions
            {
                Data = new Dictionary<string, PlayerDataObject>
                {
                    { "PlayerName", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, _playerName) }
                }
            });
    }

    private async void LeaveLobby()
    {
        await KickPlayer(AuthenticationService.Instance.PlayerId);
    }

    private async Task KickPlayer(string playerID)
    {
        try
        {
            await LobbyService.Instance.RemovePlayerAsync(_joinedLobby.Id, playerID);
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }

    private async void MigrateLobbyHost()
    {
        try
        {
            _hostLobby = await Lobbies.Instance.UpdateLobbyAsync(_hostLobby.Id, new UpdateLobbyOptions
            {
                HostId = _joinedLobby.Players[1].Id
            });
            _joinedLobby = _hostLobby;
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }

    private async void DeleteLobby()
    {
        try
        {
            await LobbyService.Instance.DeleteLobbyAsync(_joinedLobby.Id);
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }
}