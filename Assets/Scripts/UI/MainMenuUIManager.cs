using Managers;
using TMPro;
using UnityEngine;

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
        
        // GET FUNCTIONS
        public string GetName()
        {
            return _playerName;
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
            // Resetting value if player exits and reenters the other lobby
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

        public static void ToggleCarat(TMP_InputField inputField)
        {
            inputField.caretWidth = inputField.text.Length == 0 ? 0 : 2;
        }
        
        public void AnimatePlaceholder(TextMeshProUGUI placeholderText)
        {
            Debug.Log("Animate called");
            placeholderText.text = placeholderText.text.Length switch
            {
                0 => ".",
                1 => ". .",
                3 => ". . .",
                5 => "",
                _ => "."
            };
        }
    }
}