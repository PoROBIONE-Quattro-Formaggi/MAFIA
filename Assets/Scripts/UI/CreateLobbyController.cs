using System;
using DataStorage;
using Managers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class CreateLobbyController : MonoBehaviour
    {
        [Header("Buttons")] 
        public Button hostButton;
        public Button privateLobbyButton;
        public Button publicLobbyButton;
        private Image _privateLobbyButtonBg;
        private Image _publicLobbyButtonBg;
        private TextMeshProUGUI _privateLobbyButtonText;
        private TextMeshProUGUI _publicLobbyButtonText;
        
        [Header("Input")] 
        public TMP_InputField townNameInputField;
        public TextMeshProUGUI townNameInputPlaceholder;
        public TMP_InputField populationInputField;
        public TextMeshProUGUI populationInputPlaceholder;
        
        [Header("Keyboard")] 
        public KeyboardController keyboard;
        
        [Header("Information")] 
        public TextMeshProUGUI information;
        
        // VARIABLES
        private bool _isPrivate = true;

        private void Awake()
        {
            _privateLobbyButtonBg = privateLobbyButton.GetComponent<Image>();
            _publicLobbyButtonBg = publicLobbyButton.GetComponent<Image>();
            _privateLobbyButtonText = privateLobbyButton.GetComponentInChildren<TextMeshProUGUI>();
            _publicLobbyButtonText = publicLobbyButton.GetComponentInChildren<TextMeshProUGUI>();
        }

        
        // INPUT FIELD ON VALUE CHANGED
        public void OnInputValueChanged()
        {
            MainMenuUIManager.ToggleCapitalize(keyboard,townNameInputField);
            if (townNameInputField.text.Length == 0)
            {
                information.text = "Enter town name, and";
                hostButton.gameObject.SetActive(false);
                townNameInputField.caretWidth = 0;
                InvokeRepeating(nameof(ToggleTownNamePlaceholder),0.5f,0.5f);
            }
            else if (populationInputField.text.Length == 0)
            {
                information.text = "Enter population number, and";
                hostButton.gameObject.SetActive(false);
                populationInputField.caretWidth = 0;
                InvokeRepeating(nameof(TogglePopulationPlaceholder),0.5f,0.5f);
            }
            else
            {
                information.text = "Create lobby, and";
                hostButton.gameObject.SetActive(true);
            }
            
            // show carat and cancel placeholder animation if input is present 
            if (populationInputField.text.Length < 1 && townNameInputField.text.Length < 1) return;
            townNameInputField.caretWidth = 2;
            populationInputField.caretWidth = 2;
            CancelInvoke(nameof(ToggleTownNamePlaceholder));
            CancelInvoke(nameof(TogglePopulationPlaceholder));
        }

        
        // INPUT FIELD ON SELECTED FUNCTIONS
        public void OnTownNameInputSelected()
        {
            MainMenuUIManager.ToggleCapitalize(keyboard,townNameInputField);
            InvokeRepeating(nameof(ToggleTownNamePlaceholder),0.5f,0.5f);
            
            if (townNameInputField.text.Length == 0)
            {
                townNameInputField.caretWidth = 0;
            }
            
            keyboard.inputField = townNameInputField;
            keyboard.ConnectKeys();
        }
        
        public void OnPopulationInputSelected()
        {
            InvokeRepeating(nameof(TogglePopulationPlaceholder),0.5f,0.5f);
            
            if (populationInputField.text.Length == 0)
            {
                populationInputField.caretWidth = 0;
            }
            
            keyboard.inputField = populationInputField;
            keyboard.ConnectKeys();
        }
        
        
        // INPUT FIELD ON DESELECTED FUNCTIONS
        public void OnTownNameInputDeselected()
        {
            CancelInvoke(nameof(ToggleTownNamePlaceholder));
            townNameInputPlaceholder.text = ".";
        }

        public void OnPopulationInputDeselected()
        {
            CancelInvoke(nameof(TogglePopulationPlaceholder));
            populationInputPlaceholder.text = ".";
        }
        
        
        // BUTTON ON CLICK FUNCTIONS
        public void OnPublicClicked()
        {
            _isPrivate = false;
            _privateLobbyButtonBg.color = Colours.NightWhite;
            _privateLobbyButtonText.color = Colours.NightBlack;
            _publicLobbyButtonBg.color = Colours.NightBlack;
            _publicLobbyButtonText.color = Colours.NightWhite;
        }

        public void OnPrivateClicked()
        {
            _isPrivate = true;
            _privateLobbyButtonBg.color = Colours.NightBlack;
            _privateLobbyButtonText.color = Colours.NightWhite;
            _publicLobbyButtonBg.color = Colours.NightWhite;
            _publicLobbyButtonText.color = Colours.NightBlack;
        }
        
        public void OnCreateLobbyClicked()
        {
            var maxPlayersInt = 5;
            try
            {
                maxPlayersInt = int.Parse(populationInputField.text);
            }
            catch (Exception)
            {
                Debug.Log("Can't convert to int");
            }

            LobbyManager.Instance.CreateLobbyAsync("Narrator", townNameInputField.text.Trim(), maxPlayersInt, _isPrivate, "");
            ScreenChanger.Instance.ChangeToLobbyHostScreen();
        }
        
        
        // PLACEHOLDER ANIMATION FUNCTIONS
        private void ToggleTownNamePlaceholder()
        {
            townNameInputPlaceholder.text = townNameInputPlaceholder.text == "" ? "." : "";
        }
        
        private void TogglePopulationPlaceholder()
        {
            populationInputPlaceholder.text = populationInputPlaceholder.text == "" ? "." : "";
        }
    }
}