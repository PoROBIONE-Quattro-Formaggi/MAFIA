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
            informationText.text = $"The {GameSessionManager.Instance.GetWinnerRole()} wins.";
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
            SceneChanger.ChangeToMainSceneToLobbyHostScreen();
            NetworkManager.Singleton.Shutdown();
        }
    }
}