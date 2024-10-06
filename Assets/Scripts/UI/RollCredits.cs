using System.Collections.Generic;
using Backend.Hub.Controllers;
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
        public GameObject namesParent;
        public TextMeshProUGUI subtitle;

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

        private List<string> _currentNames;

        private void OnDisable()
        {
            foreach (var nameObj in nameObjects)
            {
                Destroy(nameObj);
            }
        }

        private void OnEnable()
        {
            _screenRectTransform = screen.GetComponent<RectTransform>();
            _rectTransform = GetComponent<RectTransform>();
            _currentY = -screen.GetComponent<RectTransform>().sizeDelta.y;
            GetComponent<RectTransform>().anchoredPosition = new Vector2(0, _currentY);
            InvokeRepeating(nameof(WaitForLobby), 0f, 0.1f);
        }

        private void WaitForLobby()
        {
            _maxPlayers = LobbyController.Instance.GetMaxPlayers();
            if (_maxPlayers == 0) return;

            // Display town name
            subtitle.text = "FROM " + LobbyController.Instance.GetLobbyName().ToUpper();

            _isLobbyReady = true;
            CancelInvoke(nameof(WaitForLobby));
        }

        private void UpdateCredits()
        {
            // CLEAR NAME OBJECTS
            var namesDisplayedNo = namesParent.transform.childCount;
            for (var i = namesDisplayedNo - 1; i >= 0; i--)
            {
                DestroyImmediate(namesParent.transform.GetChild(i).gameObject);
            }

            // SPAWN NAME OBJECTS FOR ALL NAMES
            if (!_isLobbyReady) return;
            var playerNames = LobbyController.Instance.GetPlayersNamesInLobby();
            foreach (var playerName in playerNames)
            {
                var nameObj = Instantiate(textPrefab, namesParent.transform);
                nameObj.GetComponentInChildren<TextMeshProUGUI>().text = playerName;
            }
        }

        private void FixedUpdate()
        {
            UpdateCredits();
            if (_currentY < _rectTransform.sizeDelta.y - 236)
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