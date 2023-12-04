using System;
using DataStorage;
using Managers;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UIElements;

namespace UI
{
    public class PlayerGameController : MonoBehaviour
    {
        public GameObject playerQuote;
        public TextMeshProUGUI playerQuoteText;
        public GameObject goVoteButton;
        public GameObject information;
        public TextMeshProUGUI informationText;
        public RectTransform informationTextRectTransform;
        public GameObject deadPrompt;
        public RectTransform parentScreenRectTransform;
        public GameObject endGameScreen;
        public GameObject prompt;
        public GameObject comfirmInputButton;
        public TextMeshProUGUI promptText;
        public TMP_InputField input;
        public KeyboardController keyboard;
        private string _time;
        private Animator _playerGameAnimator;
        


        [Header("VARIABLES")] public float animationSpeed;


        private float _currentX;

        private void Start()
        {
            NetworkCommunicationManager.Instance.OnDayBegan += Sunrise;
            NetworkCommunicationManager.Instance.OnEveningBegan += Sunset;
            NetworkCommunicationManager.Instance.OnNightBegan += MoonRise;
            NetworkCommunicationManager.Instance.OnGameEnded += EndGame;

            _playerGameAnimator = GetComponent<Animator>();
            
            OnEnableNight();
        }

        // ON ENABLE FUNCTIONS
        private void OnEnable()
        {
            _time = GameSessionManager.Instance.GetCurrentTimeOfDay();
            if (!PlayerData.IsAlive) return;
            switch (_time)
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
            SetInformationText("NIGHT");
            _playerGameAnimator.Play("night");
            if (goVoteButton.activeSelf)
            {
                SetPlayerQuoteStringNight();
                DisableInput();
            }
            else
            {
                EnableAlibiInput();
            }
            playerQuoteText.text = PlayerPrefs.GetString(PpKeys.KeyPlayerQuote);
        }

        private void OnEnableDay()
        {
            SetInformationText("DAY");
            _playerGameAnimator.Play("day");
            if (goVoteButton.activeSelf)
            {
                SetPlayerQuoteStringDay();
                DisableInput();
            }
            playerQuoteText.text = PlayerPrefs.GetString(PpKeys.KeyPlayerQuote);
        }

        private void OnEnableEvening()
        {
            SetInformationText("EVENING");
        }

        private void EnableAlibiInput()
        {
            promptText.text = "Enter alibi:";
            prompt.SetActive(true);
            input.gameObject.SetActive(true);
        }

        private void DisableInput()
        {
            prompt.SetActive(false);
            input.gameObject.SetActive(false);
        }
        
        
        private void FixedUpdate()
        {
            RollInformation();
        }

        
        private void Sunrise()
        {
            _playerGameAnimator.ResetTrigger("Sunset");
            _playerGameAnimator.SetTrigger("Sunrise");
            var lastKilledName = GameSessionManager.Instance.GetLastKilledName();
    
            if (!PlayerData.IsAlive)
            {
                OnPlayerDead();
            }
            else
            {
                prompt.SetActive(false);
                input.gameObject.SetActive(false);
                
                informationText.text = $"{lastKilledName} was killed last night";
            
                goVoteButton.SetActive(true);
                SetPlayerQuoteStringDay();
                playerQuoteText.text = PlayerPrefs.GetString(PpKeys.KeyPlayerQuote);
            }
        }

        private void Sunset()
        {
            var lastKilledName = GameSessionManager.Instance.GetLastKilledName();
            
            if (!PlayerData.IsAlive)
            {
                if (PlayerData.Name == lastKilledName)
                {
                    EnableLastWords();
                }
                else
                {
                    OnPlayerDead();
                }
            }
            else
            {
                informationText.text = $"{lastKilledName} was executed by the town.";
            }
        }

        private void MoonRise()
        {
            _playerGameAnimator.ResetTrigger("Sunrise");
            _playerGameAnimator.SetTrigger("Sunset");
            if (!PlayerData.IsAlive)
            {
                OnPlayerDead();
            }
            else
            {
                SetPlayerQuoteStringNight();
                goVoteButton.SetActive(true);
            }
            
        }

        private void EndGame()
        {
            ScreenChanger.Instance.ChangeTo(endGameScreen.name);
        }

        private void EnableLastWords()
        {
            promptText.text = "Any last words?";
            input.gameObject.SetActive(true);
            prompt.SetActive(true);
            comfirmInputButton.SetActive(true);
        }

        private void OnPlayerDead()
        {
            playerQuote.SetActive(false);
            information.SetActive(false);
            goVoteButton.SetActive(false);
            DisableInput();
            deadPrompt.SetActive(true);
        }

        public void OnConfirmInputButtonClicked()
        {
            playerQuoteText.text += ".";
            prompt.SetActive(false);
            input.gameObject.SetActive(false);
            keyboard.HideKeyboard();
        }
        
        // HELPER FUNCTIONS
        private void SetPlayerQuoteStringDay()
        {
            var playerQuoteString = $"[{PlayerData.Name}] I vote for _ to be executed";
            playerQuoteText.text = playerQuoteString;
            
            PlayerPrefs.SetString(PpKeys.KeyPlayerQuote, playerQuoteString);
            PlayerPrefs.Save();
        }

        private void SetPlayerQuoteStringNight()
        {
            var playerQuoteString = $"[{PlayerData.Name}] ";
        
            playerQuoteString += PlayerData.Role switch
            {
                // Assign question to information prompt
                Roles.Mafia => "I vote to kill _",
                Roles.Doctor => "I vote to save _",
                Roles.Resident => "I think that _ is sus", // TODO we should display here the 'funny questions' polls I think (?)
                _ => playerQuoteString
            };
            playerQuoteText.text = playerQuoteString;
            PlayerPrefs.SetString(PpKeys.KeyPlayerQuote, playerQuoteString);
            PlayerPrefs.Save();
        }

        public void SetInformationText(string text)
        {
            informationText.text = text;
        }
        
        private void RollInformation()
        {
            if (_currentX < -parentScreenRectTransform.sizeDelta.x - informationText.preferredWidth)
            {
                _currentX = 0;
            }
            else
            {
                _currentX -= 1 * animationSpeed;
            }
            informationTextRectTransform.anchoredPosition = new Vector2(_currentX, 0);
        }

        public void OnInputValueChanged()
        {
            playerQuoteText.text = $"[{PlayerData.Name}] " + input.text;
            comfirmInputButton.SetActive(input.text.Length != 0);
        }

        private void OnDestroy()
        {
            NetworkCommunicationManager.Instance.OnDayBegan -= Sunrise;
            NetworkCommunicationManager.Instance.OnEveningBegan -= Sunset;
            NetworkCommunicationManager.Instance.OnNightBegan -= MoonRise;
            NetworkCommunicationManager.Instance.OnGameEnded -= EndGame;
        }
    }
}
