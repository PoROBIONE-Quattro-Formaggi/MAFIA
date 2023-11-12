using TMPro;
using UnityEngine;

namespace UI
{
    public class EnterKey : MonoBehaviour
    {
        public KeyboardController keyboard;
        public TMP_InputField inputField;

        public void OnEnterPressed()
        {
            keyboard.HideKeyboard();
        }
    }
}
