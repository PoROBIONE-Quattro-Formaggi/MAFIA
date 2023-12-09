using System;
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
        public GameObject votePrompt;
        public TextMeshProUGUI votePromptText;

        // Night vote
        public GameObject voteButton;
        public GameObject voteOptionsParent;
        public GameObject voteOptionNightPrefab;
        public GameObject voteOptionDayPrefab;
        public GameObject goVoteButton;
        public TextMeshProUGUI playerQuoteText;
        public Animator voteAnimator;

        private ulong _currentChosenID;

        private void ThrowToGame()
        {
            ScreenChanger.Instance.ChangeToPlayerGameScreen();
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
            NetworkCommunicationManager.Instance.OnDayBegan += ThrowToGame;
            NetworkCommunicationManager.Instance.OnEveningBegan += ThrowToGame;
        }

        private void OnEnableNight()
        {
            GenerateVotingOptionsNight();
            voteAnimator.Play("night");
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
            voteAnimator.Play("day");
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
            ClearVotingOptions();

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
                var voteOption = Instantiate(voteOptionNightPrefab, voteOptionsParent.transform);
                voteOption.GetComponentInChildren<TextMeshProUGUI>().text =
                    $"<b>[{idToPlayerName[playerID]}]</b>";
                voteOption.SetActive(true);
                var toggle = voteOption.GetComponent<Toggle>();
                toggle.group = voteOptionsParent.GetComponent<ToggleGroup>();
                toggle.onValueChanged.AddListener(delegate { OnVoteOptionClicked(playerID, toggle); });
            }

            voteOptionsParent.SetActive(true);
        }

        private void GenerateVotingOptionsDay()
        {
            ClearVotingOptions();

            var alivePlayersIDs = GameSessionManager.Instance.GetAlivePlayersIDs(false);
            var idToPlayerName = GameSessionManager.Instance.IDToPlayerName;
            var idToAlibis = GameSessionManager.Instance.GetAlibis();

            Debug.Log("BEFORE FOREACH LOOP IN GENERATE OPTIONS DAY");
            foreach (var playerID in alivePlayersIDs)
            {
                var voteOption = Instantiate(voteOptionDayPrefab, voteOptionsParent.transform);
                var alibi = DefaultAlibis.GetRandomAlibi();
                if (idToAlibis.TryGetValue(playerID, out var playerSetAlibi))
                {
                    alibi = playerSetAlibi;
                }

                voteOption.GetComponentInChildren<TextMeshProUGUI>().text =
                    $"<b>[{idToPlayerName[playerID]}]</b> {alibi}";
                voteOption.SetActive(true);
                var toggle = voteOption.GetComponent<Toggle>();
                toggle.group = voteOptionsParent.GetComponent<ToggleGroup>();
                toggle.onValueChanged.AddListener(delegate { OnVoteOptionClicked(playerID, toggle); });
            }
            Debug.Log("AFTER FOREACH LOOP IN GENERATE OPTIONS DAY");
            voteOptionsParent.SetActive(true);
        }

        private void ClearVotingOptions()
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
            playerQuoteText.text = playerQuoteString;
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
            playerQuoteText.text = playerQuoteString;
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
                        //TODO turn on later
                        // GameSessionManager.Instance.ResidentVoteFor(intVoteOption);
                        break;
                }
            }
            else if (GameSessionManager.Instance.GetCurrentTimeOfDay() == TimeIsAManMadeSocialConstruct.Day)
            {
                GameSessionManager.Instance.DayVoteFor(_currentChosenID);
            }

            voteOptionsParent.SetActive(false);
            goVoteButton.SetActive(false);
            var quote = PlayerPrefs.GetString(PpKeys.KeyPlayerQuote) + ".";
            playerQuoteText.text = quote;
            PlayerPrefs.SetString(PpKeys.KeyPlayerQuote, quote);
            PlayerPrefs.Save();
        }
        
        private void OnDisable()
        {
            ClearVotingOptions();
            NetworkCommunicationManager.Instance.OnDayBegan -= ThrowToGame;
            NetworkCommunicationManager.Instance.OnEveningBegan -= ThrowToGame;
        }
    }
}