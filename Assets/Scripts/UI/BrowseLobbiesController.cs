using Managers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class BrowseLobbiesController : MonoBehaviour
    {
        [Header("Buttons")] public GameObject lobbyButtonsParent;
        public GameObject lobbyButtonRightPrefab;
        public GameObject lobbyButtonLeftPrefab;

        private void OnEnable()
        {
            AssignLobbiesToButtons();
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
                var lobbyButton = Instantiate(i % 2 == 0 ? lobbyButtonRightPrefab : lobbyButtonLeftPrefab,
                    lobbyButtonsParent.transform);
                var lobbyId = lobbies[i].Id;
                lobbyButton.GetComponent<Button>().onClick
                    .AddListener(() => MainMenuUIManager.Instance.HandleJoinLobbyClicked(lobbyId));
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
    }
}