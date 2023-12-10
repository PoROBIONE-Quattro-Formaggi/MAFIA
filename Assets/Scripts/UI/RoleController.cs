using System;
using Managers;
using Third_Party.Toast_UI.Scripts;
using TMPro;
using UnityEngine;

namespace UI
{
    public class RoleController : MonoBehaviour
    {
        [Header("PROMPTS")] public TextMeshProUGUI prompt;
        public TextMeshProUGUI paragraph1;
        public TextMeshProUGUI paragraph2;
        public GameObject mafiaRole;
        public GameObject citizenRole;
        public GameObject doctorRole;
        public TextMeshProUGUI dots;
        public ButtonOnClickAnimator buttonAnimator;

        [Header("BUTTONS")] public GameObject okButton;

        [Header("SCREENS TO CHANGE TO")] public GameObject night;

        private int _timeCounter;
        private int _counter;


        private void Start()
        {
            InvokeRepeating(nameof(AnimateDots), 0f, 0.5f);
            InvokeRepeating(nameof(EmergencyGoToMainMenuIfApplicable), 0f, 1f);
        }

        private void OnEnable()
        {
            NetworkCommunicationManager.Instance.OnDayBegan += ThrowToGame;
            buttonAnimator.SetButtonText("Ok");
            _timeCounter = 0;
            InvokeRepeating(nameof(ChangeButtonNameIfApplicable), 0f, 1f);
        }
        
        private async void EmergencyGoToMainMenuIfApplicable()
        {
            switch (_counter)
            {
                case >= 10:
                    CancelInvoke(nameof(EmergencyGoToMainMenuIfApplicable));
                    await LobbyManager.Instance.LeaveLobby();
                    SceneChanger.ChangeToMainScene();
                    Toast.Show("Your identity has been stolen. Create a new one.");
                    return;
                case 5:
                    Toast.Show("The Narrator is still searching for your identity...");
                    break;
            }

            _counter += 1;
        }

        private void ThrowToGame()
        {
            ScreenChanger.Instance.ChangeToPlayerGameScreen();
        }
        
        

        private void ChangeButtonNameIfApplicable()
        {
            if (_timeCounter == 10)
            {
                okButton.GetComponent<ButtonOnClickAnimator>().SetButtonText("Please click here");
                CancelInvoke(nameof(ChangeButtonNameIfApplicable));
                return;
            }

            Debug.Log($"time passed: {_timeCounter}");
            _timeCounter += 1;
        }

        private void AnimateDots()
        {
            AnimatePlaceholder(dots);
        }

        public void AnimatePlaceholder(TextMeshProUGUI placeholderText)
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

        private void OnDisable()
        {
            CancelInvoke(nameof(AnimateDots));
            CancelInvoke(nameof(ChangeButtonNameIfApplicable));
            NetworkCommunicationManager.Instance.OnDayBegan -= ThrowToGame;
            // prompt.text = "You are";
            // mafiaRole.SetActive(false);
            // citizenRole.SetActive(false);
            // doctorRole.SetActive(false);
            // okButton.SetActive(false);
            // paragraph1.gameObject.SetActive(false);
            // paragraph2.gameObject.SetActive(false);
        }

        public void DisplayRole(string role)
        {
            CancelInvoke(nameof(EmergencyGoToMainMenuIfApplicable));
            CancelInvoke(nameof(AnimateDots));
            dots.gameObject.SetActive(false);
            // Set text values for prompts
            switch (role)
            {
                case "Mafia":
                    DisplayMafia();
                    break;
                case "Doctor":
                    DisplayDoctor();
                    break;
                case "Resident":
                    DisplayCitizen();
                    break;
            }

            // Show prompts and ok button
            paragraph1.gameObject.SetActive(true);
            paragraph2.gameObject.SetActive(true);
            okButton.SetActive(true);
        }

        private void DisplayMafia()
        {
            prompt.text = "You are in the";
            paragraph1.text = "Choose a target for elimination during the night voting.";
            paragraph2.text = "Select victims wisely to conquer the city without revealing your identity.";
            mafiaRole.SetActive(true);
        }

        private void DisplayDoctor()
        {
            prompt.text = "You are the";
            paragraph1.text = "Eliminate suspected Mafia members during the day voting.";
            paragraph2.text = "During the night voting select one person to protect from being killed by the Mafia.";
            doctorRole.SetActive(true);
        }

        private void DisplayCitizen()
        {
            prompt.text = "You are a";
            paragraph1.text = "Eliminate suspected Mafia members during the day voting.";
            paragraph2.text = "Be vigilant and make smart decisions to avoid being killed by the mafia.";
            citizenRole.SetActive(true);
        }
    }
}