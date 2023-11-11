using TMPro;
using UnityEngine;

namespace UI
{
    public class EnterKey : MonoBehaviour
    {
        public GameObject keyboard;
        public TMP_InputField inputField;

        public void OnEnterPressed()
        {
            inputField.text = inputField.text + "\n";
            keyboard.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 0);
        }
    }
}
