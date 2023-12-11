using System;
using System.Threading.Tasks;
using DataStorage;
using Managers;
using Third_Party.Toast_UI.Scripts;
using TMPro;
using Unity.Netcode;
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
        public GameObject returnButton;
        public ScreenChanger screenChanger;
        
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
            playerGameAnimator.Play("night");

            if (!PlayerData.IsAlive)
            {
                OnPlayerDead();
                return;
            }
            
            SetInformationText(LoreMessages.GetRandomMessage());
            
            if (goVoteButton.activeSelf)
            {
                var alibi = DefaultAlibis.GetRandomAlibi();
                GameSessionManager.Instance.SetAlibi($"<b>[{PlayerData.Name}]</b> {alibi}");
                
                SetPlayerQuoteStringNight();
                DisableInput();
                confirmInputButton.SetActive(false);
            }
            else
            {
                EnableAlibiInput();
            }
            playerQuoteText.text = PlayerPrefs.GetString(PpKeys.KeyPlayerQuote);
        }

        private void OnEnableDay()
        {
            playerGameAnimator.Play("day");
            
            if (!PlayerData.IsAlive)
            {
                OnPlayerDead();
                return;
            }
            
            SetInformationText(LoreMessages.GetRandomMessage());
            
            if (goVoteButton.activeSelf)
            {
                SetPlayerQuoteStringDay();
                DisableInput();
                confirmInputButton.SetActive(false);
            }
            playerQuoteText.text = PlayerPrefs.GetString(PpKeys.KeyPlayerQuote);
        }

        private void OnEnableEvening()
        {
            playerGameAnimator.Play("day");
            
            if (!PlayerData.IsAlive)
            {
                OnPlayerDead();
                return;
            }
            
            SetInformationText(LoreMessages.GetRandomMessage());
        }

        private void EnableAlibiInput()
        {
            promptText.text = "Click below to edit alibi, or";
            Debug.Log("BEFORE VAR ALIBI =");
            var alibi = GameSessionManager.Instance.GetAlibis()[PlayerData.ClientID];
            //Trim alibi
            var trimIndex = alibi.IndexOf(']') + 1;
            alibi = alibi[trimIndex..].Trim();
            alibi = alibi.Trim('.');
            
            inputPlaceholder.text = alibi;
            Debug.Log("BEFORE SET PLAYER QUOTE STRING");
            SetPlayerQuoteString(alibi);
            SendInputToServer();
            prompt.SetActive(true);
            input.gameObject.SetActive(true);
            confirmInputButton.SetActive(true);
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
            if (confirmInputButton.activeSelf)
            {
                OnConfirmInputButtonClicked();
                SendInputToServer();
            }
            
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
            
            if (confirmInputButton.activeSelf)
            {
                OnConfirmInputButtonClicked();
                SendInputToServer();
            }
            
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
            if (confirmInputButton.activeSelf)
            {
                OnConfirmInputButtonClicked();
                SendInputToServer();
                confirmInputButton.SetActive(false);
            }
            
            // GENERATE ALIBI
            var alibi = DefaultAlibis.GetRandomAlibi();
            GameSessionManager.Instance.SetAlibi($"<b>[{PlayerData.Name}]</b> {alibi}");
            
            
            // ROLL LAST WORDS
            lastWordsText.text = $"{GameSessionManager.Instance.GetLastWords()}\n\n{GameSessionManager.Instance.GetNarratorComment()}";
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
            screenChanger.ChangeTo(endGameScreen.name);
        }
        
        public async void GoToLobbyClicked()
        {
            if (LobbyManager.Instance.IsLobbyHost())
            {
                if (!await LobbyManager.Instance.EndGame())
                {
                    Toast.Show("Cannot end the game. Try again.");
                    return;
                }

                NetworkCommunicationManager.Instance.GoBackToLobbyClientRpc();
            }
            NetworkCommunicationManager.Instance.LeaveRelay();
        }

        private void EnableLastWords()
        {
            promptText.text = "Any last words?";
            inputPlaceholder.text = ". . .";
            input.text = ""; //TODO: generate last words?
            input.gameObject.SetActive(true);
            prompt.SetActive(true);
            confirmInputButton.SetActive(true);
            inputPlaceholder.gameObject.SetActive(true);
        }

        private void OnPlayerDead()
        {
            playerQuote.SetActive(false);
            information.SetActive(false);
            goVoteButton.SetActive(false);
            DisableInput();
            confirmInputButton.SetActive(false);
            deadPrompt.SetActive(true);
            returnButton.SetActive(true);
        }

        public void OnConfirmInputButtonClicked()
        {
            if (inputPlaceholder.text != ". . .")
            {
                // TODO: tu maybe without notify
                input.text = inputPlaceholder.text;
            }
            playerQuoteText.text += ".";
            DisableInput();
        }
        
        public void OnInputValueChanged()
        {
            playerQuoteText.text = $"<b>[{PlayerData.Name}]</b> " + input.text;
            inputPlaceholder.text = ". . .";
            confirmInputButton.SetActive(input.text.Length != 0);
            input.SetTextWithoutNotify(input.text.Trim('\n'));

            if (!input.text.EndsWith('\n')) return;
            OnConfirmInputButtonClicked();
            SendInputToServer();
        }
        
        public void OnInputDeselected()
        {
            if (input.text.Length == 0)
            {
                inputPlaceholder.gameObject.SetActive(true);
            }
        }

        public void SendInputToServer()
        {
            PlayerPrefs.SetString(PpKeys.KeyPlayerQuote, playerQuoteText.text);
            PlayerPrefs.Save();
            Debug.Log($"input send to server: {playerQuoteText.text.Trim('\n')}");
            switch (GameSessionManager.Instance.GetCurrentTimeOfDay())
            {
                case TimeIsAManMadeSocialConstruct.Night:
                    GameSessionManager.Instance.SetAlibi(playerQuoteText.text.Trim('\n'));
                    break;
                case TimeIsAManMadeSocialConstruct.Evening:
                    GameSessionManager.Instance.SetLastWords(playerQuoteText.text.Trim('\n'));
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
