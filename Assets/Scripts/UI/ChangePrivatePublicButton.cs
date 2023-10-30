using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ChangePrivatePublicButton : MonoBehaviour
{
    private Color black = new(0.114f, 0.114f, 0.106f, 1);
    private Color white = new(0.863f, 0.863f, 0.855f, 1);

    public Button privateButton;
    public Button publicButton;
    

    

    public void changeButtons(){
        Image privateButtonBg = privateButton.GetComponent<Image>();
        Image publicButtonBg = publicButton.GetComponent<Image>();
        TextMeshProUGUI privateButtonText = privateButton.GetComponentInChildren<TextMeshProUGUI>();
        TextMeshProUGUI publicButtonText = publicButton.GetComponentInChildren<TextMeshProUGUI>();
        
        if (privateButtonBg.color == black && publicButtonBg.color == white) {
            privateButtonBg.color = white;
            privateButtonText.color = black;
            publicButtonBg.color = black;
            publicButtonText.color = white;
        } else {
            privateButtonBg.color = black;
            privateButtonText.color = white;
            publicButtonBg.color = white;
            publicButtonText.color = black;
        }
    }
    
}
