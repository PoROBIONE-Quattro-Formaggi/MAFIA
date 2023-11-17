using DataStorage;
using Managers;
using UnityEngine;
using TMPro;

namespace UI
{
    public class ClientGameSessionUIManager : MonoBehaviour
    {
        public GameObject infoBar;
        public GameObject alibiPrompt;
        public GameObject alibiInput;
        public GameObject goVoteButton;
        public GameObject okButton;
        public TextMeshProUGUI rolePromptText;
        public GameObject rolePrompt;
        public GameObject nightVotePrompt;
        public TextMeshProUGUI nightVotePromptText;

        // Night vote
        public GameObject voteButton;
        public GameObject voteOptionsParent;
        public GameObject voteOptionPrefab;

        private void OnEnable()
        {
            NetworkCommunicationManager.Instance.OnPlayerRoleAssigned += SetPlayerRoleToPrompt;
        }

        private void SetPlayerRoleToPrompt()
        {
            rolePromptText.text = "You are " + PlayerData.Role;
            EnableRoleInformation();
        }

        public void OnOkButtonClicked()
        {
            DisableRoleInformation();
            EnableNight();
        }

        private void DisableRoleInformation()
        {
            okButton.SetActive(false);
            rolePrompt.SetActive(false);
        }

        private void EnableRoleInformation()
        {
            rolePrompt.SetActive(true);
            okButton.SetActive(true);
        }

        private void EnableNight()
        {
            // TODO: set information for information prompt + actually animate prompt
            infoBar.SetActive(true);
            goVoteButton.SetActive(true);
        }

        private void DisableNight()
        {
            infoBar.SetActive(false);
            goVoteButton.SetActive(false);
        }

        public void OnGoVoteButtonClicked()
        {
            DisableNight();
            EnableNightVote();
        }

        private void EnableNightVote()
        {
            nightVotePromptText.text = PlayerData.Role switch
            {
                // Assign question to information prompt
                "Mafia" => "Who to kill?",
                "Doctor" => "Who to save?",
                "Resident" => "Who is sus?",
                _ => nightVotePromptText.text
            };

            nightVotePrompt.SetActive(true);
            // var playerNames = GameSessionManager.Instance.IDToPlayerName.Select(keyVal => keyVal.Value).ToList(); TODO change
            var playerNames = NetworkCommunicationManager.Instance.GetAllAlivePlayersNames();

            foreach (var playerName in playerNames)
            {
                var voteOption = Instantiate(voteOptionPrefab, voteOptionsParent.transform);
                voteOption.GetComponentInChildren<TextMeshProUGUI>().text = playerName;
            }

            voteOptionsParent.SetActive(true);
            voteButton.SetActive(true);
        }

        private void GenerateVotingOptions()
        {
            //TODO finish this function
        }
    }
}