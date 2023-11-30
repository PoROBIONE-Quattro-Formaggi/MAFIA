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
        Debug.Log("Rezised");
        
    }

    private void ResizePrompt()
    {
         Debug.Log($"{informationText.preferredWidth} + {screenRect.sizeDelta.x}");
        if (informationText.preferredWidth < screenRect.sizeDelta.x)
        {
            informationRect.sizeDelta = new Vector2(informationText.preferredWidth, informationRect.sizeDelta.y);
        }
        else
        {
            Debug.Log($"SECOND CASE{informationText.preferredWidth} + {screenRect.sizeDelta.x}");
            informationRect.sizeDelta = new Vector2(screenRect.sizeDelta.x, informationRect.sizeDelta.y);
        }
    }
    
}
}
