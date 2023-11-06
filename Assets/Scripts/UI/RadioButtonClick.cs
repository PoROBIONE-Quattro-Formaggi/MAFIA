using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class RadioButtonClick : MonoBehaviour
{
    public Image radioButtonBg;
    public bool blackBackground;
    

    public void ChangeButton(){

        Color black = new(0.114f, 0.114f, 0.106f);
        Color white;
        if (blackBackground) {
            white = new Color(0.863f, 0.863f, 0.855f);
        } else {
            white = new Color(0.918f, 0.918f, 0.91f);
        }
        
        if (radioButtonBg.color == black){
            radioButtonBg.color = white;
        } else if (radioButtonBg.color == white) {
            radioButtonBg.color = black;
        } else if (blackBackground) {
            radioButtonBg.color = white;
        } else {
            radioButtonBg.color = black;
        }
    }
}
