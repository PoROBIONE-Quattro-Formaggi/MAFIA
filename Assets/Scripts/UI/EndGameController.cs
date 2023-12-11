using Managers;
using Third_Party.Toast_UI.Scripts;
using TMPro;
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
            var winnerRole = GameSessionManager.Instance.GetWinnerRole();
            informationText.text = winnerRole switch
            {
                "Mafia" => $"The {winnerRole} wins.",
                "DRAW" => "You left the city.",
                _ => "The Citizens win."
            };
        }

        public async void GoToLobbyClicked()
        {
            if (LobbyManager.Instance.IsLobbyHost())
            {
                if (!await LobbyManager.Instance.EndGame())
                {
                    Toast.Show("Cannot end the game. Try again.");
                    return;
                }

                NetworkCommunicationManager.Instance.GoBackToLobbyClientRpc();
            }

            NetworkCommunicationManager.Instance.LeaveRelay();
        }
    }
}