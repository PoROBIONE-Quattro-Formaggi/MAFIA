using System.Collections;
using System.Collections.Generic;
using TMPro;
using UI;
using UnityEngine;
using UnityEngine.UIElements;

public class EnterNameController : MonoBehaviour
{
    public TextMeshProUGUI enterNamePlaceholder;
    public TMP_InputField enterNameField;
    public GameObject confirmNameButton;
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

    public void TogglePlaceholder(TMP_InputField inputField)
    {
        inputField.caretWidth = inputField.text.Length == 0 ? 0 : 2;
    }

    public void OnNameFieldSelected()
    {
        InvokeRepeating(nameof(ToggleNameFieldPlaceholder), 0f, 0.5f);
        TogglePlaceholder(enterNameField);
        ToggleCaps();
    }

    public void OnNameFieldDeselected()
    {
        CancelInvoke(nameof(ToggleNameFieldPlaceholder));
    }

    private void ToggleCaps()
    {
        // Toggle caps for each word typed
        keyboard.caps = enterNameField.text.Length == 0 ||  enterNameField.text[^1] == ' ';
    }
    
    public void OnPlayerNameValueChanged()
    {
        // TODO validate player name correctly later (ale jak lepiej??? - Wera)

        CancelInvoke(nameof(ToggleNameFieldPlaceholder));
        ToggleCaps();
        TogglePlaceholder(enterNameField);

        // Run animation for deleted text and still selected input
        if (enterNameField.text.Length == 0)
        {
            OnNameFieldSelected();
        }
            
        // Toggle confirm name button if input is correct
        confirmNameButton.SetActive(enterNameField.text != "");
        
        // Enter shortcut key implementation
        if (!enterNameField.text.EndsWith("\n") || enterNameField.text.Length <= 2) return;
        MainMenuUIManager.Instance.SetName(enterNameField.text.Trim());
        ScreenChanger.Instance.ChangeToBrowseLobbiesScreen();
    }
}
