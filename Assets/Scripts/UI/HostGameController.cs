using System;
using Managers;
using TMPro;
using UnityEngine;

namespace UI
{
    public class HostGameController : MonoBehaviour
    {
        public TextMeshProUGUI mafiaStatus;
        public TextMeshProUGUI doctorStatus;
        public GameObject lastDeath;
        public GameObject forwardButton;

        private bool _isMafiaDoneVoting;
        private bool _isDoctorsDoneVoting;
        

        private void OnEnable()
        {
            NetworkCommunicationManager.Instance.OnOneMafiaVoted += OnOneMafiaVoted;
            NetworkCommunicationManager.Instance.OnOneDoctorVoted += OnOneDoctorVoted;
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

        private void ShowForwardButton()
        {
            forwardButton.SetActive(_isMafiaDoneVoting && _isDoctorsDoneVoting);
        }
        

        private void OnDestroy()
        {
            NetworkCommunicationManager.Instance.OnOneMafiaVoted -= OnOneMafiaVoted;
            NetworkCommunicationManager.Instance.OnOneDoctorVoted -= OnOneDoctorVoted;
        }

        public void OnForwardClicked()
        {
            GameSessionManager.Instance.EndNight();
        }
    }
}
