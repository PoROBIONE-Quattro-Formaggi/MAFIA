using System;
using System.Runtime.InteropServices;
using UnityEngine;
using TMPro;

namespace UI
{
    public class EnterCodeController : MonoBehaviour
    {
        [DllImport("__Internal")]
        private static extern string ClearClipboard();
        
        [DllImport("__Internal")]
        private static extern void ShowClipboard();
        
        [DllImport("__Internal")]
        private static extern void HideClipboard();
        
        [Header("Input")] public TMP_InputField codeInputField;
        public TextMeshProUGUI codeInputPlaceholder;
        public TextMeshProUGUI codeDisplay;

        [Header("Information")] public TextMeshProUGUI information;

        private bool _blockAnimateCodeInputInvoke;
        private bool _blockAutomaticJoin;

        private void OnEnable()
        {
            ShowClipboard();
        }

        private void OnDisable()
        {
            HideClipboard();
        }

        public void OnCodeInputDeselected()
        {
            CancelInvoke(nameof(TogglePlaceholder));
            _blockAnimateCodeInputInvoke = false;
            codeInputPlaceholder.text = "<mspace=2.52em>......";
            HideClipboard();
        }

        public void OnCodeInputSelected()
        {
            if (codeInputField.text.Length == 0 && !_blockAnimateCodeInputInvoke)
            {
                InvokeRepeating(nameof(TogglePlaceholder), 0f, 0.5f);
                _blockAnimateCodeInputInvoke = true;
            }
            else
            {
                CancelInvoke(nameof(TogglePlaceholder));
                _blockAnimateCodeInputInvoke = false;
            }
            ShowClipboard();
        }

        public void SelectCodeInput()
        {
            codeInputField.Select();
            Debug.Log("carat reset");
            codeInputField.caretPosition = codeInputField.text.Length;
        }

        public void OnPaste(string pasteString)
        {
            Debug.Log($"Paste: {pasteString}");
            ClearClipboard();
            if (pasteString.Length != 6) return;
            SelectCodeInput();
            Debug.Log($"attempted");
            codeInputField.text = pasteString;
        }

        public void OnCodeInputValueChanged()
        {
            OnCodeInputSelected();
            codeInputField.text = codeInputField.text.ToUpper();
            Debug.Log(codeInputField.text);
            TryToJoin();
            DisplayCode();
        }

        // HELPER FUNCTIONS
        private void TryToJoin()
        {
            if (codeInputField.text.Length != 6)
            {
                information.text = "enter lobby code";
                return;
            }

            try
            {
                information.text = "Attempting to join lobby";
                MainMenuUIManager.Instance.SetCode(codeInputField.text.Trim());
                MainMenuUIManager.Instance.HandleJoinLobbyClicked("id");
                codeInputField.text = "";
            }
            catch (Exception)
            {
                information.text = "lobby not found";
            }
        }

        private void DisplayCode()
        {
            codeDisplay.text = "<mspace=2.52em>" + codeInputField.text;
        }

        public void ClearCodeInput()
        {
            codeInputField.text = "";
        }

        private void TogglePlaceholder()
        {
            codeInputPlaceholder.text = codeInputPlaceholder.text switch
            {
                "<mspace=2.52em>......" => codeInputPlaceholder.text = "<mspace=2.52em> .....",
                "<mspace=2.52em> ....." => codeInputPlaceholder.text = "<mspace=2.52em>. ....",
                "<mspace=2.52em>. ...." => codeInputPlaceholder.text = "<mspace=2.52em>.. ...",
                "<mspace=2.52em>.. ..." => codeInputPlaceholder.text = "<mspace=2.52em>... ..",
                "<mspace=2.52em>... .." => codeInputPlaceholder.text = "<mspace=2.52em>.... .",
                "<mspace=2.52em>.... ." => codeInputPlaceholder.text = "<mspace=2.52em>..... ",
                "<mspace=2.52em>..... " => codeInputPlaceholder.text = "<mspace=2.52em>......",
                _ => codeInputPlaceholder.text
            };
        }

        public void PasteCode()
        {
            var textEditor = new TextEditor();
            textEditor.Paste();
            codeInputField.text = textEditor.text;
        }
    }
}