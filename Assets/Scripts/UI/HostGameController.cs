using System;
using DataStorage;
using Managers;
using TMPro;
using UnityEngine;

namespace UI
{
    public class HostGameController : MonoBehaviour
    {
        public TextMeshProUGUI mafiaStatus;
        public TextMeshProUGUI doctorStatus;
        public TextMeshProUGUI townStatus;
        public TextMeshProUGUI executionStatus;
        public TextMeshProUGUI lastDeath;
        public GameObject forwardButton;

        private bool _isMafiaDoneVoting;
        private bool _isDoctorsDoneVoting;
        

        private void OnEnable()
        {
            NetworkCommunicationManager.Instance.OnOneMafiaVoted += OnOneMafiaVoted;
            NetworkCommunicationManager.Instance.OnOneDoctorVoted += OnOneDoctorVoted;
            NetworkCommunicationManager.Instance.OnOneResidentDayVoted += OnOneResidentDayVoted;
            NetworkCommunicationManager.Instance.OnGameEnded += EndGame;
        }

        private void OnOneMafiaVoted()
        {
            var currentMafiaVoteCount = GameSessionManager.Instance.GetCurrentAmountOfMafiaThatVoted();
            var currentAliveMafiaCount = GameSessionManager.Instance.GetAmountOfAliveMafia();

            if (currentMafiaVoteCount == currentAliveMafiaCount)
            {
                mafiaStatus.text = "The mafia has voted.";
                _isMafiaDoneVoting = true;
                ShowForwardButton();
            }
            else
                mafiaStatus.text = $"Currently {currentAliveMafiaCount} out of {currentAliveMafiaCount} have voted.";
        }

        private void OnOneDoctorVoted()
        {
            var currentDoctorVoteCount = GameSessionManager.Instance.GetCurrentAmountOfDoctorsThatVoted();
            var currentAliveDoctorCount = GameSessionManager.Instance.GetAmountOfAliveDoctors();

            if (currentDoctorVoteCount == currentAliveDoctorCount)
            {
                doctorStatus.text = "The hospital staff has voted.";
                _isDoctorsDoneVoting = true;
                ShowForwardButton();
            }
            else
                doctorStatus.text = $"Currently {currentAliveDoctorCount} out of {currentAliveDoctorCount} have voted.";
        }

        private void OnOneResidentDayVoted()
        {
            var currentResidentsDayVoteCount = GameSessionManager.Instance.GetCurrentAmountOfResidentsThatDayVoted();
            var currentAlivePlayers = GameSessionManager.Instance.GetAmountOfAliveResidents();

            if (currentResidentsDayVoteCount == currentAlivePlayers)
            {
                townStatus.text = "The town has voted.";
                forwardButton.SetActive(true);
            }
            else
            {
                townStatus.text = $"Currently {currentResidentsDayVoteCount} out of {currentAlivePlayers} have voted.";
            }
        }

        // used at night
        private void ShowForwardButton()
        {
            forwardButton.SetActive(_isMafiaDoneVoting && _isDoctorsDoneVoting);
        }

        public void OnForwardClicked()
        {
            switch (GameSessionManager.Instance.GetCurrentTimeOfDay())
            {
                case TimeIsAManMadeSocialConstruct.Night:
                    GameSessionManager.Instance.EndNight();
                    forwardButton.SetActive(false);
                    lastDeath.text = $"{GameSessionManager.Instance.GetLastKilledName()} was killed by the mafia.";
                    Sunrise();
                    break;
                case TimeIsAManMadeSocialConstruct.Day:
                    GameSessionManager.Instance.EndDay();
                    lastDeath.text = $"{GameSessionManager.Instance.GetLastKilledName()} was executed by the town.";
                    Sunset();
                    break;
                case TimeIsAManMadeSocialConstruct.Evening:
                    GameSessionManager.Instance.EndEvening();
                    forwardButton.SetActive(false);
                    MoonRise();
                    break;
            }
        }

        // TRANSITION FROM NIGHT TO DAY
        private void Sunrise()
        {
            mafiaStatus.gameObject.SetActive(false);
            mafiaStatus.text = "The mafia has not voted.";
            doctorStatus.gameObject.SetActive(false);
            doctorStatus.text = "The doctor has not voted.";

            townStatus.gameObject.SetActive(true);
        }

        // TRANSITION FROM DAY TO EVENING
        private void Sunset()
        {
            townStatus.gameObject.SetActive(false);
            townStatus.text = "The town has not voted.";
            
            executionStatus.text = $"{GameSessionManager.Instance.GetLastKilledName()} was executed by the town.";
            executionStatus.gameObject.SetActive(true);
        }

        // TRANSITION FROM EVENING TO NIGHT
        private void MoonRise()
        {
            executionStatus.gameObject.SetActive(false);
            executionStatus.text = $"_ was executed by the town.";
            
            mafiaStatus.gameObject.SetActive(true);
            doctorStatus.gameObject.SetActive(true);
        }
        
        private void EndGame()
        {
            ScreenChanger.Instance.ChangeToEndGameScreen();
        }
        
        private void OnDestroy()
        {
            NetworkCommunicationManager.Instance.OnOneMafiaVoted -= OnOneMafiaVoted;
            NetworkCommunicationManager.Instance.OnOneDoctorVoted -= OnOneDoctorVoted;
            NetworkCommunicationManager.Instance.OnOneResidentDayVoted += OnOneResidentDayVoted;
            NetworkCommunicationManager.Instance.OnGameEnded -= EndGame;
        }
    }
}
