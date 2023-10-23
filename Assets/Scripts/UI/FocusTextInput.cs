using TMPro;
using UnityEngine;

namespace UI
{
    public class FocusTextInput : MonoBehaviour
    {
        [SerializeField] private TMP_InputField nameInput;
        private void Start()
        {
            nameInput.Select();
        }
    }
}
