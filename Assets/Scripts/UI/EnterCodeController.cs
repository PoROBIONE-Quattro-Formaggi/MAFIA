using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

namespace UI
{
    public class EnterCodeController : MonoBehaviour
    {
        public TMP_InputField codeInputField;
        public TextMeshProUGUI codeDisplay;
        public TextMeshProUGUI information;

        public void PasteCode()
        {
            TextEditor textEditor = new TextEditor();
            textEditor.Paste();
            codeInputField.text = textEditor.text;
        }
    
        public void OnLobbyCodeValueChanged()
        {
            if (codeInputField.text.Length != 6)
            {
                information.text = "enter lobby code";
                return;
            }
            try
            {
                // TODO this dont work lol
                MainMenuUIManager.Instance.HandleJoinLobbyClicked(codeInputField.text);
            }
            catch(Exception)
            {
                information.text = "lobby not found";
            }
            
        }
        
        public void DisplayCode()
        {
            codeDisplay.text = "<mspace=2.95em>" + codeInputField.text;
        }

        public void ClearCodeInput()
        {
            codeInputField.text = "";
        }
    }
}

