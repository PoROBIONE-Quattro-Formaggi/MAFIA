using System;
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
        public TextMeshProUGUI subtitle;
        public TextMeshProUGUI lobbyCodeText;

        //parameters
        public float scrollSpeed;
        public List<TextMeshProUGUI> nameTexts;
        public List<GameObject> nameObjects;


        private bool _isLobbyReady;
        private int _maxPlayers;
        private float _finY;
        private float _currentY;
        private RectTransform _rectTransform;
        private RectTransform _screenRectTransform;
        private RectTransform _rectTransform2;

        private List<string> _currentNames;

        private void OnDisable()
        {
            foreach (var nameObj in nameObjects)
            {
                Destroy(nameObj);
            }
            nameTexts.Clear();
            nameObjects.Clear();
        }

        private void OnEnable()
        {
            //Application.targetFrameRate = 120;   // DEBUG: for testing different frame rates
            
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
            
            // Display town name
            subtitle.text = "FROM " + LobbyManager.Instance.GetLobbyName().ToUpper();
            
            // Display lobby code
            lobbyCodeText.text = "LOBBY CODE - <mspace=2em>" + LobbyManager.Instance.GetLobbyCode();
            
            // Spawn text objects for all possible players 
            for (var i = 0; i < _maxPlayers; i++)
            {
                var nameObj = Instantiate(textPrefab, credits.transform);
                nameObjects.Add(nameObj);
                foreach (var text in nameObj.GetComponentsInChildren<TextMeshProUGUI>())
                {
                    if (text.gameObject.name != "Text") continue;
                    nameTexts.Add(text);
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
                nameTexts[i].text = i < playerNames.Count ? playerNames[i] : "";
            }
        }

        private void FixedUpdate()
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