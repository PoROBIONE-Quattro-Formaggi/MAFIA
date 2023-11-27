using System;
using DataStorage;
using Managers;
using TMPro;
using UnityEngine;

namespace UI
{
    public class PlayerGameController : MonoBehaviour
    {
        public GameObject playerQuote;
        public TextMeshProUGUI playerQuoteText;
        public GameObject goVoteButton;
        public GameObject information;
        public TextMeshProUGUI informationText;
        public GameObject deadPrompt;

        [Header("VARIABLES")] public float animationSpeed;


        private float _currentX;
        public RectTransform parentScreenRectTransform;
        private RectTransform _textRectTransform;

        private void Start()
        {
            NetworkCommunicationManager.Instance.OnDayBegan += Sunrise;
        }

        private void OnEnable()
        {
            if (goVoteButton.activeSelf) SetPlayerQuoteStringNight();
            playerQuoteText.text = PlayerPrefs.GetString(PpKeys.KeyPlayerQuote);

            // parentScreenRectTransform = GetComponent<RectTransform>();
            _textRectTransform = informationText.GetComponent<RectTransform>();
            
            SetInformationText("test set information");
        }

        private void FixedUpdate()
        {
            RollInformation();
        }

        private void Sunrise()
        {
            var lastKilledName = GameSessionManager.Instance.GetLastKilledName();
    
            if (!PlayerData.IsAlive)
            {
                playerQuote.SetActive(false);
                information.SetActive(false);
                deadPrompt.SetActive(true);
            }
            else
            {
                informationText.text = $"{lastKilledName} was killed last night";
            
                goVoteButton.SetActive(true);
                SetPlayerQuoteStringDay();
                playerQuoteText.text = PlayerPrefs.GetString(PpKeys.KeyPlayerQuote);
            }
        }

        private void SetPlayerQuoteStringDay()
        {
            var playerQuoteString = $"[{PlayerData.Name}] I vote for _ to be executed";
            
            PlayerPrefs.SetString(PpKeys.KeyPlayerQuote, playerQuoteString);
            PlayerPrefs.Save();
        }

        private void SetPlayerQuoteStringNight()
        {
            var playerQuoteString = $"[{PlayerData.Name}] ";
        
            playerQuoteString += PlayerData.Role switch
            {
                // Assign question to information prompt
                "Mafia" => "I vote for to kill _",
                "Doctor" => "I vote to save _",
                "Resident" => "I think that _ is sus", // TODO we should display here the 'funny questions' polls I think (?)
                _ => playerQuoteString
            };
            PlayerPrefs.SetString(PpKeys.KeyPlayerQuote, playerQuoteString);
            PlayerPrefs.Save();
        }

        public void SetInformationText(string text)
        {
            informationText.text = text;
            _textRectTransform.sizeDelta = new Vector2(informationText.preferredWidth, _textRectTransform.sizeDelta.y);
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
            _textRectTransform.anchoredPosition = new Vector2(_currentX, 0);
        }

        private void OnDestroy()
        {
            NetworkCommunicationManager.Instance.OnDayBegan -= Sunrise;
        }
    }
}
