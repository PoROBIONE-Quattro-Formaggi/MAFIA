using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace UI {

public class PlayerLobbyController : MonoBehaviour
{
    public TextMeshProUGUI informationText;
    public RectTransform screenRect;
    public RectTransform informationRect;

    private void OnEnable() {
        informationText.text = $"You are {MainMenuUIManager.Instance.GetName()}, please wait";
        ResizePrompt();
    }

    private void Update() {
        ResizePrompt();
        
    }

    private void ResizePrompt()
    {
        informationRect.sizeDelta = informationText.preferredWidth < screenRect.sizeDelta.x ? new Vector2(informationText.preferredWidth, informationRect.sizeDelta.y) : new Vector2(screenRect.sizeDelta.x, informationRect.sizeDelta.y);
    }
    
}
}
