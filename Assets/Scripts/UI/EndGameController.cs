using Managers;
using Third_Party.Toast_UI.Scripts;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class EndGameController : MonoBehaviour
    {
        public TextMeshProUGUI informationText;
        public Button goToLobbyButton;

        private void OnEnable()
        {
            string winnerRole = GameSessionManager.Instance.GetWinnerRole();
            if (winnerRole == "Mafia"){
                informationText.text = $"The {winnerRole} wins.";
            } else {
                informationText.text = "The Citizens win.";
            }
            if (NetworkCommunicationManager.Instance.IsHost)
            {
                goToLobbyButton.gameObject.SetActive(true);
            }
        }

        public async void GoToLobbyClicked()
        {
            if (!await LobbyManager.Instance.EndGame())
            {
                Toast.Show("Cannot end the game. Try again.");
                return;
            }

            LobbyManager.Instance.IsCurrentlyInGame = false;
            NetworkCommunicationManager.Instance.GoBackToLobbyClientRpc();
            LobbyManager.Instance.LeaveLobby();
            SceneChanger.ChangeToMainScene();
            // SceneChanger.ChangeToMainSceneToLobbyHostScreen(); TODO back to lobby functionality maybe later
            NetworkManager.Singleton.Shutdown();
        }
    }
}