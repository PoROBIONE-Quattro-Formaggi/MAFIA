using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class CreateLobbyController : MonoBehaviour
    {
        public Button hostButton;
        public TMP_InputField townNameInputField;
        public TextMeshProUGUI townNameInputPlaceholder;
        public TMP_InputField populationInputField;
        public TextMeshProUGUI populationInputPlaceholder;
        public KeyboardController keyboard;
        public TextMeshProUGUI information;

        public void OnInputValueChanged()
        {
            if (townNameInputField.text.Length == 0)
            {
                information.text = "Enter town name, and";
                hostButton.gameObject.SetActive(false);
                townNameInputField.caretWidth = 0;
            }
            else if (populationInputField.text.Length == 0)
            {
                information.text = "Enter population number, and";
                hostButton.gameObject.SetActive(false);
                populationInputField.caretWidth = 0;
            }
            else
            {
                information.text = "Create lobby, and";
                hostButton.gameObject.SetActive(true);
            }
            
            if (populationInputField.text.Length > 1 || townNameInputField.text.Length > 1)
            {
                townNameInputField.caretWidth = 2;
                populationInputField.caretWidth = 2;
            }
            CancelInvoke(nameof(ToggleTownNamePlaceholder));
            CancelInvoke(nameof(TogglePopulationPlaceholder));
        }

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

        public void ToggleTownNamePlaceholder()
        {
            townNameInputPlaceholder.text = townNameInputPlaceholder.text == "" ? "." : "";
        }
        
        public void TogglePopulationPlaceholder()
        {
            populationInputPlaceholder.text = populationInputPlaceholder.text == "" ? "." : "";
        }

        public void OnTownNameInputSelected()
        {
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
    }
}