using System.Collections;
using System.Collections.Generic;
using TMPro;
using UI;
using UnityEngine;
using UnityEngine.UIElements;

public class EnterNameController : MonoBehaviour
{
    [Header("Input")]
    public TextMeshProUGUI enterNamePlaceholder;
    public TMP_InputField enterNameField;
    
    [Header("Confirm button")]
    public GameObject confirmNameButton;
    
    [Header("Keyboard")]
    public KeyboardController keyboard;

    public void ToggleNameFieldPlaceholder()
    {
        enterNamePlaceholder.text = enterNamePlaceholder.text.Length switch
        {
            0 => ".",
            1 => ". .",
            3 => ". . .",
            5 => "",
            _ => enterNamePlaceholder.text
        };
    }

    public void OnNameFieldSelected()
    {
        InvokeRepeating(nameof(ToggleNameFieldPlaceholder), 0f, 0.5f);
        MainMenuUIManager.Instance.ToggleCarat(enterNameField);
        MainMenuUIManager.ToggleCapitalize(keyboard, enterNameField);
    }

    public void OnNameFieldDeselected()
    {
        CancelInvoke(nameof(ToggleNameFieldPlaceholder));
        enterNamePlaceholder.text = ". . ."; 
        enterNameField.text = enterNameField.text.Trim();
    }
    
    
    public void OnPlayerNameValueChanged()
    {
        // TODO validate player name correctly later (ale jak lepiej??? - Wera)

        CancelInvoke(nameof(ToggleNameFieldPlaceholder));
        MainMenuUIManager.ToggleCapitalize(keyboard, enterNameField);
        MainMenuUIManager.Instance.ToggleCarat(enterNameField);
        
        // Play placeholder animation if no text input present
        if (enterNameField.text.Length == 0)
        {
            InvokeRepeating(nameof(ToggleNameFieldPlaceholder), 0f, 0.5f);
        }
            
        // Toggle confirm name button if input is correct
        confirmNameButton.SetActive(enterNameField.text != "");
        
        // Enter shortcut key implementation
        if (!enterNameField.text.EndsWith("\n") || enterNameField.text.Length <= 2) return;
        OnConfirmNameButtonClicked();
    }

    public void OnConfirmNameButtonClicked()
    {
        MainMenuUIManager.Instance.SetName(enterNameField.text.Trim());
        ScreenChanger.Instance.ChangeToBrowseLobbiesScreen();
    }
}
