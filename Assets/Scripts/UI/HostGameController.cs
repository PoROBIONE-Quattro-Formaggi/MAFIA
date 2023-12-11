using System;
using System.Linq;
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
        public TextMeshProUGUI commentPrompt;
        public GameObject forwardButton;
        public GameObject endGameScreen;
        public TMP_InputField input;
        public TextMeshProUGUI inputPlaceholder;
        public Animator hostGameAnimator;
        public ScreenChanger screenChanger;
        

        private bool _isMafiaDoneVoting;
        private bool _isDoctorsDoneVoting;
        private bool _isRolesAssigned;

        private void Start()
        {
            NetworkCommunicationManager.Instance.OnPlayerRoleAssigned += SetRolesAssigned;
        }


        private void OnEnable()
        {
            NetworkCommunicationManager.Instance.OnOneMafiaVoted += OnOneMafiaVoted;
            NetworkCommunicationManager.Instance.OnOneDoctorVoted += OnOneDoctorVoted;
            NetworkCommunicationManager.Instance.OnOneResidentDayVoted += OnOneResidentDayVoted;
            GameSessionManager.Instance.OnHostEndGame += EndGame;

            switch (GameSessionManager.Instance.GetCurrentTimeOfDay())
            {
                case TimeIsAManMadeSocialConstruct.Night:
                    hostGameAnimator.Play("night");
                    if (_isRolesAssigned)
                    {
                        OnOneMafiaVoted();
                        OnOneDoctorVoted();
                    }
                    break;
                case TimeIsAManMadeSocialConstruct.Day:
                    hostGameAnimator.Play("day");
                    if (_isRolesAssigned)
                    {
                        OnOneResidentDayVoted();
                    }
                    break;
                case TimeIsAManMadeSocialConstruct.Evening:
                    hostGameAnimator.Play("day");
                    break;
            }
        }

        private void OnOneMafiaVoted()
        {
            var currentMafiaVoteCount = GameSessionManager.Instance.GetCurrentAmountOfMafiaThatVoted();
            var currentAliveMafiaCount = GameSessionManager.Instance.GetAmountOfAliveMafia();

            Debug.Log($"currentMafiaVoteCount: {currentMafiaVoteCount}, currentAliveMafiaCount: {currentAliveMafiaCount}");
            if (currentMafiaVoteCount == currentAliveMafiaCount)
            {
                mafiaStatus.text = "<b>The mafia has voted.</b>";
                _isMafiaDoneVoting = true;
                ShowForwardButton();
            }
            else
                mafiaStatus.text = $"Currently <b>{currentAliveMafiaCount} out of {currentAliveMafiaCount} mafiosi</b> have voted.";
        }

        private void OnOneDoctorVoted()
        {
            var currentDoctorVoteCount = GameSessionManager.Instance.GetCurrentAmountOfDoctorsThatVoted();
            var currentAliveDoctorCount = GameSessionManager.Instance.GetAmountOfAliveDoctors();

            if (currentDoctorVoteCount == currentAliveDoctorCount)
            {
                doctorStatus.text = "<b>The hospital staff has voted.</b>";
                _isDoctorsDoneVoting = true;
                ShowForwardButton();
            }
            else
                doctorStatus.text = $"Currently <b>{currentAliveDoctorCount} out of {currentAliveDoctorCount} doctors</b> have voted.";
        }

        private void OnOneResidentDayVoted()
        {
            var currentResidentsDayVoteCount = GameSessionManager.Instance.GetCurrentAmountOfResidentsThatDayVoted();
            var currentAlivePlayers = GameSessionManager.Instance.GetAmountOfAliveResidents();

            if (currentResidentsDayVoteCount == currentAlivePlayers)
            {
                townStatus.text = "<b>The town has voted.</b>";
                forwardButton.SetActive(true);
            }
            else
            {
                townStatus.text = $"Currently <b>{currentResidentsDayVoteCount} out of {currentAlivePlayers} citizens</b> have voted.";
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
                    lastDeath.text = $"<b>[The Narrator]</b> {GameSessionManager.Instance.GetLastKilledName()} was killed by the mafia.";
                    Sunrise();
                    break;
                case TimeIsAManMadeSocialConstruct.Day:
                    GameSessionManager.Instance.EndDay();
                    lastDeath.text = $"<b>[The Narrator]</b> {GameSessionManager.Instance.GetLastKilledName()} was executed by the town.";
                    Sunset();
                    break;
                case TimeIsAManMadeSocialConstruct.Evening:
                    if (input.text != "")
                    {
                        GameSessionManager.Instance.SetNarratorComment($"{lastDeath.text.Trim('\n')}.");
                    }
                    GameSessionManager.Instance.EndEvening();
                    forwardButton.SetActive(false);
                    DisableInput();
                    lastDeath.text = $"<b>[The Narrator]</b> {GameSessionManager.Instance.GetLastKilledName()} was executed by the town.";
                    MoonRise();
                    break;
            }
        }

        public void OnInputValueChanged()
        {
            lastDeath.text = $"<b>[The Narrator]</b> " + input.text;

            if (input.text.Length == 0)
            {
                InvokeRepeating(nameof(AnimatePlaceholder), 0f, 0.5f);
            } 
            else
            {
                CancelInvoke(nameof(AnimatePlaceholder));
            }

            if (input.text.EndsWith('\n'))
            {
                OnForwardClicked();
            }
        }
        
        private void AnimatePlaceholder()
        {
            AnimatePlaceholder(inputPlaceholder);
        }
        
        private void AnimatePlaceholder(TextMeshProUGUI placeholderText)
        {
            placeholderText.text = placeholderText.text.Length switch
            {
                0 => ".",
                1 => ". .",
                3 => ". . .",
                5 => "",
                _ => "."
            };
        }

        public void OnInputSelected()
        {
            if (input.text.Length != 0) return;
            InvokeRepeating(nameof(AnimatePlaceholder), 0f, 0.5f);
        }

        public void OnInputDeselected()
        {
            CancelInvoke(nameof(AnimatePlaceholder));
            if (input.text.Length == 0)
            {
                inputPlaceholder.text = ". . .";
                inputPlaceholder.gameObject.SetActive(true);
            }
        }

        // TRANSITION FROM NIGHT TO DAY
        private void Sunrise()
        {
            mafiaStatus.gameObject.SetActive(false);
            mafiaStatus.text = "<b>The mafia has not voted.</b>";
            doctorStatus.gameObject.SetActive(false);
            doctorStatus.text = "<b>The medical staff has not voted.</b>";
            
            hostGameAnimator.Play("sunrise");

            townStatus.gameObject.SetActive(true);
        }

        // TRANSITION FROM DAY TO EVENING
        private void Sunset()
        {
            townStatus.gameObject.SetActive(false);
            townStatus.text = "<b>The town has not voted.</b>";
            
            executionStatus.text = $"{GameSessionManager.Instance.GetLastKilledName()} was executed by the town.";
            executionStatus.gameObject.SetActive(true);
            EnableInput();
        }

        private void EnableInput()
        {
            commentPrompt.gameObject.SetActive(true);
            input.SetTextWithoutNotify("");
            input.gameObject.SetActive(true);
        }

        private void DisableInput()
        {
            commentPrompt.gameObject.SetActive(false);
            input.gameObject.SetActive(false);
        }

        // TRANSITION FROM EVENING TO NIGHT
        private void MoonRise()
        {
            executionStatus.gameObject.SetActive(false);
            executionStatus.text = $"_ was executed by the town.";
            
            hostGameAnimator.Play("sunset");
            
            if (GameSessionManager.Instance.GetAmountOfAliveDoctors() == 0)
            {
                doctorStatus.text = "<b>All the medical staff is dead</b>";
            }
            mafiaStatus.gameObject.SetActive(true);
            doctorStatus.gameObject.SetActive(true);
        }
        
        private void EndGame()
        {
            screenChanger.ChangeTo(endGameScreen.name);
        }

        private void SetRolesAssigned()
        {
            _isRolesAssigned = true;
            NetworkCommunicationManager.Instance.OnPlayerRoleAssigned -= SetRolesAssigned;
        }
        
        private void OnDestroy()
        {
            NetworkCommunicationManager.Instance.OnOneMafiaVoted -= OnOneMafiaVoted;
            NetworkCommunicationManager.Instance.OnOneDoctorVoted -= OnOneDoctorVoted;
            NetworkCommunicationManager.Instance.OnOneResidentDayVoted -= OnOneResidentDayVoted;
            GameSessionManager.Instance.OnHostEndGame -= EndGame;
        }
    }
}
