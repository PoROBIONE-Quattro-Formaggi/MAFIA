using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CreateLobbyController : MonoBehaviour
{
    public Button hostButton;
    public TMP_InputField townNameInputField;
    public TMP_InputField populationInputField;
    public KeyboardController keyboard;

    public void onInputValueChanged()
    {
        hostButton.gameObject.SetActive(townNameInputField.text.Length != 0 && populationInputField.text.Length != 0);
    }

    public void onTownNameInputSelected()
    {
        keyboard.inputField = townNameInputField;
        keyboard.connectKeys();
    }
    
    public void onPopulationInputSelected()
    {
        keyboard.inputField = populationInputField;
        keyboard.connectKeys();
    }

}
