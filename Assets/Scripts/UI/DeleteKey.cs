using TMPro;
using UnityEngine;

namespace UI
{
    public class DeleteKey : MonoBehaviour
    {
        public TMP_InputField inputField;

        public void OnDeleteKeyPressed()
        {
            if (inputField.text.Length == 0) return;
            inputField.text = inputField.text[..^1];
        }
    }
}
