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

        private Animator _voteAnimator;
        //private string _time;

        private void Start()
        {
            _voteAnimator = GetComponent<Animator>();
        }

        private void OnEnable()
        {
            switch (GameSessionManager.Instance.GetCurrentTimeOfDay())
            {
                case TimeIsAManMadeSocialConstruct.Night:
                    OnEnableNight();
                    break;
                case TimeIsAManMadeSocialConstruct.Day:
                    OnEnableDay();
                    break;
                case TimeIsAManMadeSocialConstruct.Evening:
                    OnEnableEvening();
                    break;
            }
        }

        private void OnEnableNight()
        {
            GenerateVotingOptionsNight();
            _voteAnimator.Play("night");
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
            _voteAnimator.Play("day");
            GenerateVotingOptionsDay();
            votePromptText.text = "Who to execute?";
            playerQuoteText.text = PlayerPrefs.GetString(PpKeys.KeyPlayerQuote);
        }

        private void OnEnableEvening()
        {
            //TODO: implement
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
            GenerateVotingOptions(alivePlayersIDs);
        }
        
        private void GenerateVotingOptionsDay()
        {
            var alivePlayersIDs = GameSessionManager.Instance.GetAlivePlayersIDs(false);
            GenerateVotingOptions(alivePlayersIDs);
        }

        private void GenerateVotingOptions(List<ulong> alivePlayersIDs)
        {
            var idToPlayerName = GameSessionManager.Instance.IDToPlayerName;
            foreach (var playerID in alivePlayersIDs)
            {
                var voteOption = Instantiate(voteOptionPrefab, voteOptionsParent.transform);
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
            switch (GameSessionManager.Instance.GetCurrentTimeOfDay())
            {
                case TimeIsAManMadeSocialConstruct.Night:
                    SetPlayerQuoteStringNight(GameSessionManager.Instance.IDToPlayerName[currentClickedID]);
                    break;
                case TimeIsAManMadeSocialConstruct.Day:
                    SetPlayerQuoteStringDay(GameSessionManager.Instance.IDToPlayerName[currentClickedID]);
                    break;
            }
            playerQuoteText.text = PlayerPrefs.GetString(PpKeys.KeyPlayerQuote);
        }

        private void SetPlayerQuoteStringDay(string voteOptionName)
        {
            var playerQuoteString = $"[{PlayerData.Name}] I vote for {voteOptionName} to be executed";
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
            if (GameSessionManager.Instance.GetCurrentTimeOfDay() == TimeIsAManMadeSocialConstruct.Night)
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
            } else if (GameSessionManager.Instance.GetCurrentTimeOfDay() == TimeIsAManMadeSocialConstruct.Day)
            {
                GameSessionManager.Instance.DayVoteFor(_currentChosenID);
            }
            
            voteOptionsParent.SetActive(false);
            goVoteButton.SetActive(false);
            var quote = PlayerPrefs.GetString(PpKeys.KeyPlayerQuote) + ".";
            PlayerPrefs.SetString(PpKeys.KeyPlayerQuote, quote);
            PlayerPrefs.Save();
            playerQuoteText.text = quote;
        }
    }
}