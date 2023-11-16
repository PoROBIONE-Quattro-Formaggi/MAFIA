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

        private static MainMenuUIManager _instance;
        private string _lobbyToJoinID;
        private string _lobbyToJoinCode;
        private string _playerName;


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

        // SET FUNCTIONS
        public void SetName(string playerName)
        {
            _playerName = playerName;
        }

        public void SetCode(string code)
        {
            _lobbyToJoinCode = code;
        }
        

        public void HandleJoinLobbyClicked(string lobbyID)
        {
            if (lobbyID == "id")
            {
                lobbyID = null;
            }
            Debug.Log($"lobbyID: {lobbyID}, code: {_lobbyToJoinCode}, playerName: {_playerName}");
            var lobbyCode = _lobbyToJoinCode;
            var playerName = _playerName;
            LobbyManager.Instance.JoinLobby(lobbyID: lobbyID, code: lobbyCode, playerName: playerName);
            // Resetting values if player exits and reenters the other lobby
            _lobbyToJoinID = null;
            _lobbyToJoinCode = null;
            InvokeRepeating(nameof(WaitForLobbyToJoin), 0f, 0.1f);
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
        
        // HELPER FUNCTIONS
        public static void ToggleCapitalize(KeyboardController keyboard, TMP_InputField inputField)
        {
            // Toggle caps for each word typed
            if (inputField.text.Length == 0 || inputField.text[^1] == ' ' || keyboard.caps)
            {
                keyboard.OnCapsPressed();
            }
        }
        
        public void ToggleCarat(TMP_InputField inputField)
        {
            inputField.caretWidth = inputField.text.Length == 0 ? 0 : 2;
        }
    }
}