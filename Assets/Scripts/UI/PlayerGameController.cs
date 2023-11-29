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
        public RectTransform informationTextRectTransform;
        public GameObject deadPrompt;
        public RectTransform parentScreenRectTransform;
        private string _time;
        


        [Header("VARIABLES")] public float animationSpeed;


        private float _currentX;

        private void Start()
        {
            NetworkCommunicationManager.Instance.OnDayBegan += Sunrise;
            NetworkCommunicationManager.Instance.OnEveningBegan += Sunset;
            NetworkCommunicationManager.Instance.OnNightBegan += MoonRise;
        }

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
            SetInformationText("NIGHT");
            if (goVoteButton.activeSelf) SetPlayerQuoteStringNight();
            playerQuoteText.text = PlayerPrefs.GetString(PpKeys.KeyPlayerQuote);
        }

        private void OnEnableDay()
        {
            SetInformationText("DAY");
            if (goVoteButton.activeSelf) SetPlayerQuoteStringDay();
            playerQuoteText.text = PlayerPrefs.GetString(PpKeys.KeyPlayerQuote);
        }

        private void OnEnableEvening()
        {
            SetInformationText("EVENING");
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

        private void Sunset()
        {
            var lastKilledName = GameSessionManager.Instance.GetLastKilledName();
            
            if (!PlayerData.IsAlive)
            {
                //TODO: get last words etc.
                playerQuote.SetActive(false);
                information.SetActive(false);
                deadPrompt.SetActive(true);
            }
            else
            {
                informationText.text = $"{lastKilledName} was executed by the town.";
            }
        }

        private void MoonRise()
        {
            goVoteButton.SetActive(true);
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
                Roles.Mafia => "I vote for to kill _",
                Roles.Doctor => "I vote to save _",
                Roles.Resident => "I think that _ is sus", // TODO we should display here the 'funny questions' polls I think (?)
                _ => playerQuoteString
            };
            PlayerPrefs.SetString(PpKeys.KeyPlayerQuote, playerQuoteString);
            PlayerPrefs.Save();
        }

        public void SetInformationText(string text)
        {
            informationText.text = text;
            //TODO: double check this resize
            Debug.Log(informationText.preferredWidth);
            informationTextRectTransform.sizeDelta = new Vector2(informationText.preferredWidth, informationTextRectTransform.sizeDelta.y);
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
        }
    }
}
