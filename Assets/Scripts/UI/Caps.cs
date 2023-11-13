using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace UI
{
    public class Caps : MonoBehaviour
    {
        public GameObject keyboard;

        private readonly List<TextMeshProUGUI> _chars = new();

        public bool caps;


        private void Start()
        {
            foreach (var keyChar in keyboard.GetComponentsInChildren<TextMeshProUGUI>())
            {
                _chars.Add(keyChar);
            }
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