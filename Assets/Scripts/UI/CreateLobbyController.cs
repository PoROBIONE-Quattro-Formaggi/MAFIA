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
        public RectTransform inputDisplay;
        public TextMeshProUGUI townNameInputText;
        public RectTransform townNameInputTextRectTransform;
        public float inputDisplayOffset;
        public float spaceWidth;
        
        [Header("Keyboard")] 
        public KeyboardController keyboard;
        
        [Header("Information")] 
        public TextMeshProUGUI information;
        
        // VARIABLES
        private bool _isPrivate = true;
        private float _inputDisplayMinWidth;
        

        private void Awake()
        {
            _privateLobbyButtonBg = privateLobbyButton.GetComponent<Image>();
            _publicLobbyButtonBg = publicLobbyButton.GetComponent<Image>();
            _privateLobbyButtonText = privateLobbyButton.GetComponentInChildren<TextMeshProUGUI>();
            _publicLobbyButtonText = publicLobbyButton.GetComponentInChildren<TextMeshProUGUI>();

            _inputDisplayMinWidth = inputDisplay.sizeDelta.x;
        }

        private void AdjustCreateDisplay()
        { 
            inputDisplay.sizeDelta = new Vector2(townNameInputText.preferredWidth + inputDisplayOffset, inputDisplay.sizeDelta.y);
            
            // TODO: reset text position (left = 4, but this is fucked up w chuy), spacja nie dodaje do preffered width XDD
        }

        
        // INPUT FIELD ON VALUE CHANGED
        public void OnTownNameInputValueChanged()
        {
            MainMenuUIManager.ToggleCapitalize(keyboard,townNameInputField);
            
            // Adjust create display width to text input width
            if (townNameInputText.preferredWidth > _inputDisplayMinWidth - inputDisplayOffset)
            {
                AdjustCreateDisplay();
            }
            
            if (townNameInputField.text.Length == 0)
            {
                townNameInputField.caretWidth = 0;
                InvokeRepeating(nameof(ToggleTownNamePlaceholder),0.5f,0.5f);
            }
            else
            {
                // show carat and cancel placeholder animation if input is present
                townNameInputField.caretWidth = 2;
                CancelInvoke(nameof(ToggleTownNamePlaceholder));
            }
            OnInputValueChanged();
        }

        public void OnPopulationInputValueChanged()
        {
            if (populationInputField.text.Length == 0)
            {
                populationInputField.caretWidth = 0;
                InvokeRepeating(nameof(TogglePopulationPlaceholder),0.5f,0.5f);
            }
            else
            {
                // show carat and cancel placeholder animation if input is present
                populationInputField.caretWidth = 2;
                CancelInvoke(nameof(TogglePopulationPlaceholder));
            }
            OnInputValueChanged();
        }
        
        
        private void OnInputValueChanged()
        {
            if (townNameInputField.text.Length == 0)
            {
                information.text = "Enter town name";
                hostButton.gameObject.SetActive(false);
            }
            else if (populationInputField.text.Length == 0)
            {
                information.text = "Enter population number";
                hostButton.gameObject.SetActive(false);
            }
            else
            {
                information.text = "Create lobby, and";
                hostButton.gameObject.SetActive(true);
            }
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
    
    public static class RectTransformExtensions
    {
        public static void SetLeft(this RectTransform rt, float left)
        {
            rt.offsetMin = new Vector2(left, rt.offsetMin.y);
        }

        public static void SetRight(this RectTransform rt, float right)
        {
            rt.offsetMax = new Vector2(-right, rt.offsetMax.y);
        }

        public static void SetTop(this RectTransform rt, float top)
        {
            rt.offsetMax = new Vector2(rt.offsetMax.x, -top);
        }

        public static void SetBottom(this RectTransform rt, float bottom)
        {
            rt.offsetMin = new Vector2(rt.offsetMin.x, bottom);
        }
    }
}