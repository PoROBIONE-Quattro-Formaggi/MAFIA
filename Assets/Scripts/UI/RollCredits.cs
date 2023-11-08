using System.Collections.Generic;
using Managers;
using TMPro;
using UnityEngine;

namespace UI
{
    public class RollCredits : MonoBehaviour
    {
        public GameObject screen;
        public GameObject textPrefab;
        public GameObject credits;

        //parameters
        public float scrollSpeed;
        public List<TextMeshProUGUI> nameObjects;


        private bool _isLobbyReady;
        private int _maxPlayers;
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
            GetComponent<RectTransform>().anchoredPosition = new Vector2(0, _currentY);
            InvokeRepeating(nameof(WaitForLobby), 0f, 0.1f);
        }

        private void WaitForLobby()
        {
            _maxPlayers = LobbyManager.Instance.GetMaxPlayers();
            if (_maxPlayers == 0) return;
            // Spawn text objects for all possible players 
            for (var i = 0; i < _maxPlayers; i++)
            {
                var nameObj = Instantiate(textPrefab, credits.transform);
                foreach (var text in nameObj.GetComponentsInChildren<TextMeshProUGUI>())
                {
                    if (text.gameObject.name != "Text") continue;
                    nameObjects.Add(text);
                }
            }

            _isLobbyReady = true;
            CancelInvoke(nameof(WaitForLobby));
        }

        private void UpdateCredits()
        {
            if (!_isLobbyReady) return;
            var playerNames = LobbyManager.Instance.GetPlayersNamesInLobby();
            for (var i = 0; i < _maxPlayers; i++)
            {
                nameObjects[i].text = i < playerNames.Count ? playerNames[i] : "";
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