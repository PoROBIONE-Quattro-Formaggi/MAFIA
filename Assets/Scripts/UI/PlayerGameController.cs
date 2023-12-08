using System;
using DataStorage;
using Managers;
using Third_Party.Toast_UI.Scripts;
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
        public GameObject confirmInputButton;
        public TextMeshProUGUI promptText;
        public TMP_InputField input;
        public TextMeshProUGUI inputPlaceholder;
        public KeyboardController keyboard;
        public GameObject lastWords;
        public TextMeshProUGUI lastWordsText;
        public float scrollSpeed;
        public Animator playerGameAnimator;
        public RectTransform lastWordsRectTransform;
        
        private string _time;
        private bool _notYet;
        private bool _rollLastWords;
        
        


        [Header("VARIABLES")] public float animationSpeed;


        private float _currentX;

        private void Start()
        {
            NetworkCommunicationManager.Instance.OnDayBegan += Sunrise;
            NetworkCommunicationManager.Instance.OnEveningBegan += Sunset;
            NetworkCommunicationManager.Instance.OnNightBegan += MoonRise;
            NetworkCommunicationManager.Instance.OnGameEnded += EndGame;

            lastWordsRectTransform = lastWords.GetComponent<RectTransform>();
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
            SetInformationText(LoreMessages.GetRandomMessage());
            SetAlibiInput();
            
            playerGameAnimator.Play("night");
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
            SetInformationText(LoreMessages.GetRandomMessage());
            playerGameAnimator.Play("day");
            if (goVoteButton.activeSelf)
            {
                SetPlayerQuoteStringDay();
                DisableInput();
            }
            playerQuoteText.text = PlayerPrefs.GetString(PpKeys.KeyPlayerQuote);
        }

        private void OnEnableEvening()
        {
            SetInformationText(LoreMessages.GetRandomMessage());
            playerGameAnimator.Play("day");
        }

        private void EnableAlibiInput()
        {
            promptText.text = "Click below to edit alibi, or";
            Debug.Log("BEFORE VAR ALIBI =");
            var alibi = GameSessionManager.Instance.GetAlibis()[PlayerData.ClientID];
            inputPlaceholder.text = alibi;
            Debug.Log("BEFORE SET PLAYER QUOTE STRING");
            SetPlayerQuoteString(alibi);
            prompt.SetActive(true);
            input.gameObject.SetActive(true);
            confirmInputButton.SetActive(true);
        }

        private void SetAlibiInput()
        {
            var alibi = DefaultAlibis.GetRandomAlibi().Trim('.');
            GameSessionManager.Instance.SetAlibi(alibi);
        }

        private void DisableInput()
        {
            prompt.SetActive(false);
            input.gameObject.SetActive(false);
            keyboard.HideKeyboard();
        }
        
        
        private void FixedUpdate()
        {
            RollInformation();
            if (_rollLastWords)
            {
                RollLastWords();
            }
        }

        
        private void Sunrise()
        {
            playerGameAnimator.ResetTrigger("Sunset");
            playerGameAnimator.SetTrigger("Sunrise");
            //OnConfirmInputButtonClicked();
            DisableInput();
            confirmInputButton.SetActive(false);
            
            var lastKilledName = GameSessionManager.Instance.GetLastKilledName();
    
            if (!PlayerData.IsAlive)
            {
                OnPlayerDead();
            }
            else
            {
                informationText.text = MafiaDeaths.GetRandomMafiaDeathMessage(lastKilledName);
            
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
                informationText.text = Executions.GetRandomDeathMessage(lastKilledName);
            }
        }

        private void RollLastWords()
        {
            // ANIMATE LAST WORDS
            if (lastWordsRectTransform.anchoredPosition.y <
                lastWordsRectTransform.sizeDelta.y + parentScreenRectTransform.sizeDelta.y)
            {
                lastWordsRectTransform.anchoredPosition = new Vector2(lastWordsRectTransform.anchoredPosition.x,
                    lastWordsRectTransform.anchoredPosition.y + 1 * scrollSpeed);
            }
            else
            {
                lastWordsRectTransform.anchoredPosition = new Vector2(lastWordsRectTransform.anchoredPosition.x, 0);
                lastWordsText.text = "";
                _notYet = false;
                _rollLastWords = false;
            }
        }

        private void MoonRise()
        {
            // HIDE INPUT
            //OnConfirmInputButtonClicked();
            DisableInput();
            
            // ROLL LAST WORDS
            lastWordsText.text = $"{GameSessionManager.Instance.GetLastWords()}\n{GameSessionManager.Instance.GetNarratorComment()}";
            playerQuote.SetActive(false);
            deadPrompt.SetActive(false);

            string[] subs = lastWordsText.text.Split(']');
            if (subs[1].Trim().Length > 1)
            {
                _notYet = true;
                _rollLastWords = true;
            }
            InvokeRepeating(nameof(WaxingCrescentMoon), 0f, 0.5f);
        }

        private void WaxingCrescentMoon()
        {
            if (_notYet) return;
            CancelInvoke(nameof(WaxingCrescentMoon));
            
            playerGameAnimator.ResetTrigger("Sunrise");
            playerGameAnimator.SetTrigger("Sunset");
            
            if (!PlayerData.IsAlive)
            {
                OnPlayerDead();
            }
            else
            {
                // ALIVE PLAYERS DO THIS
                SetPlayerQuoteStringNight();
                
                playerQuote.SetActive(true);
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
            inputPlaceholder.text = ". . .";
            input.text = ""; //TODO: generate last words?
            input.gameObject.SetActive(true);
            prompt.SetActive(true);
            confirmInputButton.SetActive(true);
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
            if (inputPlaceholder.text != ". . .")
            {
                Debug.Log("input edited");
                input.text = inputPlaceholder.text;
            }
            playerQuoteText.text += ".";
            SendInputToServer();
            DisableInput();
        }
        
        public void OnInputValueChanged()
        {
            playerQuoteText.text = $"<b>[{PlayerData.Name}]</b> " + input.text;
            inputPlaceholder.text = ". . .";
            confirmInputButton.SetActive(input.text.Length != 0);
        }

        private void SendInputToServer()
        {
            PlayerPrefs.SetString(PpKeys.KeyPlayerQuote, playerQuoteText.text);
            PlayerPrefs.Save();
            switch (GameSessionManager.Instance.GetCurrentTimeOfDay())
            {
                case TimeIsAManMadeSocialConstruct.Night:
                    GameSessionManager.Instance.SetAlibi(input.text);
                    break;
                case TimeIsAManMadeSocialConstruct.Evening:
                    GameSessionManager.Instance.SetLastWords(playerQuoteText.text);
                    break;
            }

            input.SetTextWithoutNotify("");
        }
        
        // HELPER FUNCTIONS
        private void SetPlayerQuoteStringDay()
        {
            var playerQuoteString = $"<b>[{PlayerData.Name}]</b> I vote for _ to be executed";
            playerQuoteText.text = playerQuoteString;
            
            PlayerPrefs.SetString(PpKeys.KeyPlayerQuote, playerQuoteString);
            PlayerPrefs.Save();
        }

        private void SetPlayerQuoteStringNight()
        {
            var playerQuoteString = $"<b>[{PlayerData.Name}]</b> ";
        
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

        private void SetPlayerQuoteString(string quote)
        {
            playerQuoteText.text = $"<b>[{PlayerData.Name}]</b> " + quote;
            PlayerPrefs.SetString(PpKeys.KeyPlayerQuote, playerQuoteText.text);
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

        private void OnDestroy()
        {
            NetworkCommunicationManager.Instance.OnDayBegan -= Sunrise;
            NetworkCommunicationManager.Instance.OnEveningBegan -= Sunset;
            NetworkCommunicationManager.Instance.OnNightBegan -= MoonRise;
            NetworkCommunicationManager.Instance.OnGameEnded -= EndGame;
        }
    }
}
