using System.Threading.Tasks;
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
            if (winnerRole == "Mafia")
            {
                informationText.text = $"The {winnerRole} wins.";
            }
            else if (winnerRole == "DRAW")
            {
                informationText.text = "You left the city.";
            }
            else
            {
                informationText.text = "The Citizens win.";
            }
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