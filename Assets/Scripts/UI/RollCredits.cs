using System.Collections.Generic;
using Managers;
using TMPro;
using UnityEngine;

namespace UI
{
    public class RollCredits : MonoBehaviour
    {
        public GameObject screen;
        public float scrollSpeed;
        public GameObject textPrefab;
        public GameObject credits;

        private float _finY;
        private float _currentY;
        private RectTransform _rectTransform;
        private RectTransform _screenRectTransform;
        private RectTransform _rectTransform2;
        private List<string> _currentNames; 

        private void Start()
        {
            _screenRectTransform = screen.GetComponent<RectTransform>();
            _rectTransform = GetComponent<RectTransform>();
            _currentY = -screen.GetComponent<RectTransform>().sizeDelta.y;
            
            UpdateCredits();
            
            GetComponent<RectTransform>().anchoredPosition = new Vector2(0, _currentY);
        }

        private void UpdateCredits()
        {
            var playerNames = LobbyManager.Instance.GetPlayersNamesInLobby();
            foreach (var playerName in playerNames)
            {
                if (_currentNames.Contains(playerName)) break;
                var nameObj = Instantiate(textPrefab, credits.transform);
                foreach (var text in nameObj.GetComponentsInChildren<TextMeshProUGUI>())
                {
                    if (text.gameObject.name != "Text") continue;
                    text.text = playerName;
                    _currentNames.Add(playerName);
                }
            }
        }

        private void Update()
        {
            UpdateCredits();
            if (_currentY < _rectTransform.sizeDelta.y)
            {
                _currentY += 1 * scrollSpeed;
            }
            else
            {
                _currentY = -_screenRectTransform.sizeDelta.y;
            }

            _rectTransform.anchoredPosition = new Vector2(0, _currentY);
        }
    }
}