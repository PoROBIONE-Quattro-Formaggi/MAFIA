using TMPro;
using UI;
using UnityEngine;
using UnityEngine.UI;

public class CreateLobbyController : MonoBehaviour
{
    public Button hostButton;
    public TMP_InputField townNameInputField;
    public TMP_InputField populationInputField;
    public KeyboardController keyboard;

    public void OnInputValueChanged()
    {
        hostButton.gameObject.SetActive(townNameInputField.text.Length != 0 && populationInputField.text.Length != 0);
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