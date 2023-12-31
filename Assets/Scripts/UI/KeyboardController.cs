using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class KeyboardController : MonoBehaviour
    {
        public RectTransform screenRect;
        public TMP_InputField inputField;
        public GameObject numpadKeyboard;
        public GameObject defaultKeyboard;

        [Header("Variables")] public bool caps;
        public float heightOnScreen;
        private RectTransform _keyboardTransform;
        private readonly List<HorizontalLayoutGroup> _rows = new();
        private readonly List<TextMeshProUGUI> _chars = new();

        private void Start()
        {
            _keyboardTransform = GetComponent<RectTransform>();
            ConnectKeys();
            foreach (var row in _keyboardTransform.GetComponentsInChildren<HorizontalLayoutGroup>())
            {
                _rows.Add(row);
            }

            foreach (var keyChar in GetComponentsInChildren<TextMeshProUGUI>())
            {
                _chars.Add(keyChar);
                if (caps)
                {
                    keyChar.text = keyChar.text.ToUpper();
                }
            }
        }


        public void DisplayNumpad()
        {
            defaultKeyboard.SetActive(false);
            numpadKeyboard.SetActive(true);
        }

        public void DisplayDefault()
        {
            numpadKeyboard.SetActive(false);
            defaultKeyboard.SetActive(true);
        }

        public void ConnectKeys()
        {
            // Connect key scripts with input field
            foreach (var keyScript in _keyboardTransform.GetComponentsInChildren<InputChar>())
            {
                keyScript.inputField = inputField;
            }

            try
            {
                // Connect delete with input field
                _keyboardTransform.GetComponentInChildren<DeleteKey>().inputField = inputField;

                // Connect enter with input field
                _keyboardTransform.GetComponentInChildren<EnterKey>().inputField = inputField;
            }
            catch (Exception)
            {
                Debug.Log("some key missing");
            }
        }

        public void ShowKeyboard()
        {
            if (!Application.isMobilePlatform) return;
            _keyboardTransform.anchoredPosition = new Vector2(0, _keyboardTransform.sizeDelta.y + heightOnScreen);
        }

        public void HideKeyboard()
        {
            if (!Application.isMobilePlatform) return;
            _keyboardTransform.anchoredPosition = new Vector2(0, 0);
        }

        public void OnCapsPressed()
        {
            // toggle caps
            caps = !caps;

            if (caps)
            {
                foreach (var keyChar in _chars)
                {
                    keyChar.text = keyChar.text.ToUpper();
                }
            }
            else
            {
                foreach (var keyChar in _chars)
                {
                    keyChar.text = keyChar.text.ToLower();
                }
            }
        }
    }
}