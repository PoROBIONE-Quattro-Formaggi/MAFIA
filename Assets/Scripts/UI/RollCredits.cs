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
            //Application.targetFrameRate = 120;   // DEBUG: for testing different frame rates
            
            _screenRectTransform = screen.GetComponent<RectTransform>();
            _rectTransform = GetComponent<RectTransform>();
            _currentY = -screen.GetComponent<RectTransform>().sizeDelta.y;
            GetComponent<RectTransform>().anchoredPosition = new Vector2(0, _currentY);
            InvokeRepeating(nameof(WaitForLobby), 0f, 0.1f);
            
            Debug.Log("Started Roll credits");
        }

        private void WaitForLobby()
        {
            _maxPlayers = LobbyManager.Instance.GetMaxPlayers();
            if (_maxPlayers == 0) return;
            Debug.Log("Lobby Found");
            
            
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

            subtitle.text = "FROM" + LobbyManager.Instance.GetLobbyName().ToUpper();

            _isLobbyReady = true;
            CancelInvoke(nameof(WaitForLobby));
        }

        private void UpdateCredits()
        {
            if (!_isLobbyReady) return;
            Debug.Log("Updating credits");
            var playerNames = LobbyManager.Instance.GetPlayersNamesInLobby();
            for (var i = 0; i < _maxPlayers; i++)
            {
                nameObjects[i].text = i < playerNames.Count ? playerNames[i] : "";
            }
        }

        private void FixedUpdate()
        {
            UpdateCredits();
            Debug.Log("Animating credits");
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