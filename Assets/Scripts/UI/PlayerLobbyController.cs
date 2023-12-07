using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace UI {

public class PlayerLobbyController : MonoBehaviour
{
    public TextMeshProUGUI informationText;

    private void OnEnable() 
    {
        SetWelcomePrompt(MainMenuUIManager.Instance.GetName());
    }

    public void SetWelcomePrompt(string playerName)
    {
        informationText.text = $"You are {playerName}, please wait";
    }
}
}
