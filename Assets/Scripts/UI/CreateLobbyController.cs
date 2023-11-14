using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class CreateLobbyController : MonoBehaviour
    {
        public Button hostButton;
        public TMP_InputField townNameInputField;
        public TMP_InputField populationInputField;
        public KeyboardController keyboard;
        public TextMeshProUGUI information;

        public void OnInputValueChanged()
        {
            if (townNameInputField.text.Length != 0 && populationInputField.text.Length == 0)
            {
                information.text = "Enter population number, and";
                hostButton.gameObject.SetActive(false);
            } else if (townNameInputField.text.Length == 0)
            {
                information.text = "Enter town name, and";
                hostButton.gameObject.SetActive(false);
            }
            else if (townNameInputField.text.Length != 0 && populationInputField.text.Length !=0)
            {
                information.text = "Create lobby, and";
                hostButton.gameObject.SetActive(true);
            }
        }

        public void OnTownNameInputSelected()
        {
            keyboard.inputField = townNameInputField;
            keyboard.ConnectKeys();
        }

        public void OnPopulationInputSelected()
        {
            keyboard.inputField = populationInputField;
            keyboard.ConnectKeys();
        }
    }
}