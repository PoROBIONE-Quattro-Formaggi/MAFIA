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
        
        //parameters
        public float scrollSpeed;
        public int _debug_population;
        public List<TextMeshProUGUI> nameObjects;
        public List<string> _debug_playerNames;

        private float _finY;
        private float _currentY;
        private RectTransform _rectTransform;
        private RectTransform _screenRectTransform;
        private RectTransform _rectTransform2;
        private List<string> _currentNames; 

        private void Start()
        {
            for (int i = 0; i < _debug_population; i++)
            {
                var nameObj = Instantiate(textPrefab, credits.transform);
                foreach (var text in nameObj.GetComponentsInChildren<TextMeshProUGUI>())
                {
                    if (text.gameObject.name != "Text") continue;
                    nameObjects.Add(text);
                }
            }
            _screenRectTransform = screen.GetComponent<RectTransform>();
            _rectTransform = GetComponent<RectTransform>();
            _currentY = -screen.GetComponent<RectTransform>().sizeDelta.y;
            
            UpdateCredits();
            
            GetComponent<RectTransform>().anchoredPosition = new Vector2(0, _currentY);
        }

        private void UpdateCredits()
        {
            List<string> playerNames = new List<string>();
            try
            {
                playerNames = LobbyManager.Instance.GetPlayersNamesInLobby();
            }
            catch (Exception e)
            {
                Debug.Log(e);
            }
            Debug.Log(playerNames.Count);
            
            for (int i = 0; i < _debug_population; i++)
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