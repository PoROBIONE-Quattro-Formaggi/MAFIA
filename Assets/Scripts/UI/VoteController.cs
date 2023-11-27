using System.Collections.Generic;
using DataStorage;
using Managers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class VoteController : MonoBehaviour
    {
        public GameObject nightVotePrompt;
        public TextMeshProUGUI nightVotePromptText;

        // Night vote
        public GameObject voteButton;
        public GameObject voteOptionsParent;
        public GameObject voteOptionPrefab;
        public GameObject goVoteButton;
        public TextMeshProUGUI playerQuoteText;

        private ulong _currentChosenID;

        private void OnEnable()
        {
            GenerateVotingOptions();
            nightVotePromptText.text = PlayerData.Role switch
            {
                // Assign question to information prompt
                Roles.Mafia => "Who to kill?",
                Roles.Doctor => "Who to save?",
                Roles.Resident => "Who is sus?", // TODO we should display here the 'funny questions' polls I think (?)
                _ => nightVotePromptText.text
            };
            voteOptionsParent.SetActive(true);
            playerQuoteText.text = PlayerPrefs.GetString(PpKeys.KeyPlayerQuote);
        }

        private void GenerateVotingOptions()
        {
            List<ulong> alivePlayersIDs;
            switch (PlayerData.Role)
            {
                case Roles.Mafia:
                    alivePlayersIDs = GameSessionManager.Instance.GetAliveNonMafiaPlayersIDs();
                    break;
                case Roles.Doctor:
                case Roles.Resident:
                    alivePlayersIDs = GameSessionManager.Instance.GetAlivePlayersIDs(false);
                    break;
                default:
                    alivePlayersIDs = new List<ulong>();
                    break;
            }

            var idToPlayerName = GameSessionManager.Instance.IDToPlayerName;
            foreach (var playerID in alivePlayersIDs)
            {
                var voteOption = Instantiate(voteOptionPrefab, voteOptionsParent.transform);
                // voteOption.GetComponentInChildren<TextMeshProUGUI>().text = idToPlayerName[playerID];
                voteOption.GetComponentInChildren<TextMeshProUGUI>().text =
                    $"{idToPlayerName[playerID]} - {playerID.ToString()}";
                voteOption.SetActive(true);
                var toggle = voteOption.GetComponent<Toggle>();
                toggle.group = voteOptionsParent.GetComponent<ToggleGroup>();
                toggle.onValueChanged.AddListener(delegate { OnVoteOptionClicked(playerID, toggle); });
            }

            voteOptionsParent.SetActive(true);
        }

        private void OnDisable()
        {
            // CLEAR LOBBIES LIST BEFORE REFRESH
            var buttonsDisplayedNo = voteOptionsParent.transform.childCount;
            for (var i = buttonsDisplayedNo - 1; i >= 0; i--)
            {
                DestroyImmediate(voteOptionsParent.transform.GetChild(i).gameObject);
            }

            voteButton.SetActive(false);
        }

        private void OnVoteOptionClicked(ulong currentClickedID, Toggle toggle)
        {
            if (!toggle.isOn) return;
            voteButton.SetActive(true);
            _currentChosenID = currentClickedID;
            SetPlayerQuoteString(GameSessionManager.Instance.IDToPlayerName[currentClickedID]);
        }

        private void SetPlayerQuoteString(string voteOptionName)
        {
            var playerQuoteString = $"[{PlayerData.Name}] ";

            playerQuoteString += PlayerData.Role switch
            {
                // Assign question to information prompt
                Roles.Mafia => $"I vote for to kill {voteOptionName}",
                Roles.Doctor => $"I vote to save {voteOptionName}",
                Roles.Resident =>
                    $"I think that {voteOptionName} is sus", // TODO we should display here the 'funny questions' polls I think (?)
                _ => playerQuoteString
            };
            PlayerPrefs.SetString(PpKeys.KeyPlayerQuote, playerQuoteString);
            PlayerPrefs.Save();
        }


        public void OnVoteClicked()
        {
            switch (PlayerData.Role)
            {
                case Roles.Mafia:
                    GameSessionManager.Instance.MafiaVoteFor(_currentChosenID);
                    break;
                case Roles.Doctor:
                    GameSessionManager.Instance.DoctorVoteFor(_currentChosenID);
                    break;
                case Roles.Resident:
                    //TODO 
                    // GameSessionManager.Instance.ResidentVoteFor(intVoteOption);
                    break;
            }

            voteOptionsParent.SetActive(false);
            goVoteButton.SetActive(false);
            playerQuoteText.text = PlayerPrefs.GetString(PpKeys.KeyPlayerQuote);
        }
    }
}