using TMPro;
using UnityEngine;

namespace UI
{
    public class InputChar : MonoBehaviour
    {
        public TextMeshProUGUI charText;
        public TMP_InputField inputField;

        public void OnKeyPressed()
        {
            if (inputField.text.Length == inputField.characterLimit) return;
            Debug.Log($"key pressed: {charText.text}");
            inputField.text += charText.text;
        }
    }
}