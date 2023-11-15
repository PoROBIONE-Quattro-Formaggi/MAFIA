using System;
using DataStorage;
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

        // CREATE LOBBY
        public TMP_InputField townName;
        public TMP_InputField maxPlayers;
        public Button privateLobbyButton;
        public Button publicLobbyButton;
        
        // ENTER NAME
        public TMP_InputField inputPlayerName;
        public GameObject confirmNameButton;

        // BROWSE LOBBY BUTTONS
        public GameObject lobbyButtonsParent;
        public GameObject lobbyButtonRight;
        public GameObject lobbyButtonLeft;
        
        // LOBBY
        public GameObject lobbyScreen;

        private static MainMenuUIManager _instance;
        private string _lobbyToJoinID;
        private string _lobbyToJoinCode;
        private string _playerName;
        private bool _isPrivate = true;
        private Image _privateLobbyButtonBg;
        private Image _publicLobbyButtonBg;
        private TextMeshProUGUI _privateLobbyButtonText;
        private TextMeshProUGUI _publicLobbyButtonText;


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

            _privateLobbyButtonBg = privateLobbyButton.GetComponent<Image>();
            _publicLobbyButtonBg = publicLobbyButton.GetComponent<Image>();
            _privateLobbyButtonText = privateLobbyButton.GetComponentInChildren<TextMeshProUGUI>();
            _publicLobbyButtonText = publicLobbyButton.GetComponentInChildren<TextMeshProUGUI>();
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
            _privateLobbyButtonBg.color = Colours.NightWhite;
            _privateLobbyButtonText.color = Colours.NightBlack;
            _publicLobbyButtonBg.color = Colours.NightBlack;
            _publicLobbyButtonText.color = Colours.NightWhite;
        }

        public void SetPrivate()
        {
            _isPrivate = true;
            _privateLobbyButtonBg.color = Colours.NightBlack;
            _privateLobbyButtonText.color = Colours.NightWhite;
            _publicLobbyButtonBg.color = Colours.NightWhite;
            _publicLobbyButtonText.color = Colours.NightBlack;
        }

        public async void AssignLobbiesToButtons()
        {
            // CLEAR LOBBIES LIST BEFORE REFRESH
            var buttonsDisplayedNo = lobbyButtonsParent.transform.childCount;
            for (var i = buttonsDisplayedNo - 1; i >= 0; i--)
            {
                DestroyImmediate(lobbyButtonsParent.transform.GetChild(i).gameObject);
            }


            var lobbies = await LobbyManager.GetLobbiesList();
            var parentHeight = lobbyButtonsParent.GetComponent<RectTransform>().sizeDelta.y;

            for (var i = 0; i < lobbies.Count && i <= parentHeight / 96 - 1; i++)
            {
                var lobbyButton = Instantiate(i % 2 == 0 ? lobbyButtonRight : lobbyButtonLeft,
                    lobbyButtonsParent.transform);
                var lobbyId = lobbies[i].Id;
                lobbyButton.GetComponent<Button>().onClick.AddListener(() => HandleJoinLobbyClicked(lobbyId));
                foreach (var lobbyButtonChild in lobbyButton.GetComponentsInChildren<TextMeshProUGUI>(true))
                {
                    lobbyButtonChild.text = lobbyButtonChild.gameObject.name switch
                    {
                        "CityName" => lobbies[i].Name,
                        "Population" => $"POPULATION {lobbies[i].Players.Count} / {lobbies[i].MaxPlayers}",
                        _ => lobbyButtonChild.text
                    };
                }
            }
        }

        public void HandleJoinLobbyClicked(string lobbyID)
        {
            var lobbyId = lobbyID;
            var lobbyCode = _lobbyToJoinCode;
            var playerName = _playerName;
            LobbyManager.Instance.JoinLobby(lobbyID: lobbyId, code: lobbyCode, playerName: playerName);
            // Resetting values if player exits and reenters the other lobby
            _lobbyToJoinID = null;
            _lobbyToJoinCode = null;
            InvokeRepeating(nameof(WaitForLobbyToJoin), 0f, 0.1f);
        }

        public void OnPlayerNameValueChanged()
        {
            // TODO validate player name correctly later
            
            confirmNameButton.SetActive(inputPlayerName.text != "");
            if (!inputPlayerName.text.EndsWith("\n") || inputPlayerName.text.Length <= 2) return;
            OnPlayerNameEnterButtonClicked();
        }

        public void OnPlayerNameEnterButtonClicked()
        {
            _playerName = inputPlayerName.text.Trim();
            ScreenChanger.Instance.ChangeToBrowseLobbiesScreen();
        }

        private void WaitForLobbyToJoin()
        {
            if (!LobbyManager.Instance.IsLobbyJoined()) return;
            ScreenChanger.Instance.ChangeToLobbyPlayerScreen();
            CancelInvoke(nameof(WaitForLobbyToJoin));
        }

        public void LeaveLobby()
        {
            if (LobbyManager.Instance.IsLobbyHost())
            {
                ScreenChanger.Instance.ChangeToCreateLobbyScreen();
            }
            else
            {
                ScreenChanger.Instance.ChangeToBrowseLobbiesScreen();
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