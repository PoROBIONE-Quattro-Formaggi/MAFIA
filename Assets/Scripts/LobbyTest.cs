using System.Collections.Generic;
using TMPro;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;

public class LobbyTest : MonoBehaviour
{
    public TextMeshProUGUI debug;
    public TMP_InputField codeInputField;

    private Lobby _hostLobby;

    private async void Start()
    {
        await UnityServices.InitializeAsync();
        AuthenticationService.Instance.SignedIn += () =>
        {
            debug.text += "\nSigned in as: " + AuthenticationService.Instance.PlayerId;
        };
        await AuthenticationService.Instance.SignInAnonymouslyAsync();
        InvokeRepeating(nameof(HandleLobbyHeartbeat), 0f, 15f);
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
                IsPrivate = false
            };
            var lobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, maxPlayers, createLobbyOptions);
            _hostLobby = lobby;
            debug.text += $"\nLobby Created: {lobby.Name} {lobby.MaxPlayers} {lobby.LobbyCode}";
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
            var code = codeInputField.text;
            await Lobbies.Instance.JoinLobbyByCodeAsync(code);
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
            await Lobbies.Instance.QuickJoinLobbyAsync();
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }
}