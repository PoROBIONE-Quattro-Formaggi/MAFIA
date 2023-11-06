using DataStorage;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class RadioButtonClick : MonoBehaviour
    {
        public Image radioButtonBg;
        public bool blackBackground;

        public void ChangeButton()
        {
            var white = blackBackground ? Colours.NightWhite : Colours.DayWhite;
            if (radioButtonBg.color == Colours.NightBlack){
                radioButtonBg.color = white;
            } else if (radioButtonBg.color == white) {
                radioButtonBg.color = Colours.NightBlack;
            } else if (blackBackground) {
                radioButtonBg.color = white;
            } else {
                radioButtonBg.color = Colours.NightBlack;
            }
        }
    }
}
