using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace UI {

public class PlayerLobbyController : MonoBehaviour
{
    public TextMeshProUGUI informationText;

    private void OnEnable() {
        informationText.text = $"You are {MainMenuUIManager.Instance.GetName()}, please wait";
    }
    
}
}