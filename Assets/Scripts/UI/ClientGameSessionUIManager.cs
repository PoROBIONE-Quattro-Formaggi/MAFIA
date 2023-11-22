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
        public GameObject exitVoteButton;

        public RoleController roleController;
        
        

        private void OnEnable()
        {
            Debug.Log("GameScreen Enabled");
            NetworkCommunicationManager.Instance.OnPlayerRoleAssigned += SetPlayerRoleToPrompt;
        }

        private void SetPlayerRoleToPrompt()
        {
            roleController.DisplayRole(PlayerData.Role);
        }

        // BUTTON ONCLICK FUNCTIONS
        public void OnOkButtonClicked()
        {
            DisableRoleInformation();
            EnableNight();
        }

        public void OnExitVotingButtonClicked()
        {
            DisableNightVote();
            EnableNight();
        }
        
        public void OnGoVoteButtonClicked()
        {
            DisableNight();
            EnableNightVote();
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

        private void EnableNightVote()
        {
            nightVotePromptText.text = PlayerData.Role switch
            {
                // Assign question to information prompt
                "Mafia" => "Who to kill?",
                "Doctor" => "Who to save?",
                "Resident" => "Who is sus?", // TODO we should display here the 'funny questions' polls I think (?)
                _ => nightVotePromptText.text
            };

            nightVotePrompt.SetActive(true);
            exitVoteButton.SetActive(true);
            GenerateVotingOptions();
        }

        private void DisableNightVote()
        {
            // CLEAR LOBBIES LIST BEFORE REFRESH
            var buttonsDisplayedNo = voteOptionsParent.transform.childCount;
            for (var i = buttonsDisplayedNo - 1; i >= 0; i--)
            {
                DestroyImmediate(voteOptionsParent.transform.GetChild(i).gameObject);
            }
            nightVotePrompt.SetActive(false);
            voteOptionsParent.SetActive(false);
            voteButton.SetActive(false);
            exitVoteButton.SetActive(false);
        }

        private void GenerateVotingOptions()
        {
            //var playerNames = GameSessionManager.Instance.GetAlivePlayersNames();
            //TODO if you don't want to show the user himself to vote for:
            var playerNames = GameSessionManager.Instance.GetAlivePlayersNames(false);
            
            Debug.Log($"player names returned: {playerNames.Count}");


            foreach (var playerName in playerNames)
            {
                var voteOption = Instantiate(voteOptionPrefab, voteOptionsParent.transform);
                Debug.Log(playerName);
                voteOption.GetComponentInChildren<TextMeshProUGUI>().text = playerName;
                voteOption.SetActive(true);
            }
            voteOptionsParent.SetActive(true);
        }
    }
}