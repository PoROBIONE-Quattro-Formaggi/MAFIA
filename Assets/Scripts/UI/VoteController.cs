using System;
using System.Collections.Generic;
using DataStorage;
using Managers;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace UI
{
    public class VoteController : MonoBehaviour
    {
        public GameObject votePrompt;
        public TextMeshProUGUI votePromptText;

        // Night vote
        public GameObject voteButton;
        public GameObject voteOptionsParent;
        public GameObject voteOptionPrefab;
        public GameObject goVoteButton;
        public TextMeshProUGUI playerQuoteText;

        private ulong _currentChosenID;
        private string _time = "night";

        private void OnEnable()
        {
            _time = FindObjectOfType<PlayerGameController>(false).Time;
            Debug.Log(_time);
            switch (_time)
            {
                case "night":
                    Debug.Log("OnEnableNight called");
                    OnEnableNight();
                    break;
                case "day":
                    OnEnableDay();
                    break;
            }
        }

        private void OnEnableNight()
        {
            GenerateVotingOptionsNight();
            votePromptText.text = PlayerData.Role switch
            {
                // Assign question to information prompt
                Roles.Mafia => "Who to kill?",
                Roles.Doctor => "Who to save?",
                Roles.Resident => "Who is sus?", // TODO we should display here the 'funny questions' polls I think (?)
                _ => votePromptText.text
            };
            playerQuoteText.text = PlayerPrefs.GetString(PpKeys.KeyPlayerQuote);
        }

        private void OnEnableDay()
        {
            GenerateVotingOptionsDay();
            votePromptText.text = "Who to execute?";
            playerQuoteText.text = PlayerPrefs.GetString(PpKeys.KeyPlayerQuote);
        }

        private void GenerateVotingOptionsNight()
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

        private void GenerateVotingOptionsDay()
        {
            var alivePlayersIDs = GameSessionManager.Instance.GetAlivePlayersIDs(false);
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
            switch (_time)
            {
                case "night":
                    SetPlayerQuoteStringNight(GameSessionManager.Instance.IDToPlayerName[currentClickedID]);
                    break;
                case "day":
                    SetPlayerQuoteStringDay(GameSessionManager.Instance.IDToPlayerName[currentClickedID]);
                    break;
            }
        }

        private void SetPlayerQuoteStringDay(string voteOptionName)
        {
            var playerQuoteString = $"[{PlayerData.Name}] I vote for {voteOptionName} to be executed.";
            PlayerPrefs.SetString(PpKeys.KeyPlayerQuote, playerQuoteString);
            PlayerPrefs.Save();
        }

        private void SetPlayerQuoteStringNight(string voteOptionName)
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
            if (_time == "night")
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
            } else if (_time == "day")
            {
                //TODO: implement day functionality
            }
            
            voteOptionsParent.SetActive(false);
            goVoteButton.SetActive(false);
            playerQuoteText.text = PlayerPrefs.GetString(PpKeys.KeyPlayerQuote);
        }
    }
}