using System;
using System.Collections.Generic;
using Managers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class MainMenuUIManager : MonoBehaviour
    {
        public static MainMenuUIManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<MainMenuUIManager>();
                }

                return _instance;
            }
        }

        public TMP_InputField codeInputField;
        public TMP_InputField townName;
        public TMP_InputField maxPlayers;
        public TMP_InputField inputPlayerName;
        public GameObject lobbyButtonsParent;


        private static MainMenuUIManager _instance;
        private string _lobbyToJoinID;
        private string _lobbyToJoinCode;
        private bool _isPrivate = true;
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
        }

        public void OnCreateLobbyClicked()
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

            LobbyManager.Instance.CreateLobbyAsync("Narrator", townName.text, maxPlayersInt, _isPrivate, "");
            ScreenChanger.Instance.ChangeToLobbyHostScreen();
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
            var lobbies = await LobbyManager.GetLobbiesList();
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
            ScreenChanger.Instance.ChangeToSetNameScreen();
        }

        public void OnLobbyCodeValueChanged()
        {
            if (codeInputField.text.Length != 6) return;
            _lobbyToJoinCode = codeInputField.text;
            ScreenChanger.Instance.ChangeToSetNameScreen();
        }

        public void OnPlayerNameEntered()
        {
            var lobbyID = _lobbyToJoinID;
            var lobbyCode = _lobbyToJoinCode;
            var playerName = inputPlayerName.text;
            LobbyManager.Instance.JoinLobby(lobbyID: lobbyID, code: lobbyCode, playerName: playerName);
            // Resetting values if player exits and reenters the other lobby
            _lobbyToJoinID = null;
            _lobbyToJoinCode = null;
            ScreenChanger.Instance.ChangeToLobbyPlayerScreen();
        }

        public void LeaveLobby()
        {
            if (LobbyManager.Instance.IsLobbyHost())
            {
                ScreenChanger.Instance.ChangeToCreateLobbyScreen();
            }
            else
            {
                ScreenChanger.Instance.ChangeToJoinLobbyScreen();
            }

            if (LobbyManager.Instance.GetPlayersListInLobby().Count == 1)
            {
                LobbyManager.Instance.DeleteLobby();
            }
            else
            {
                LobbyManager.Instance.LeaveLobby();
            }
        }
    }
}