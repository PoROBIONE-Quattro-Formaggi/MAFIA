using TMPro;
using UnityEngine;

namespace UI
{
    public class EnterNameController : MonoBehaviour
    {
        [Header("Input")] 
        public TextMeshProUGUI enterNamePlaceholder;
        public TMP_InputField enterNameField;

        [Header("Confirm button")] public GameObject confirmNameButton;

        [Header("Keyboard")] public KeyboardController keyboard;


        private void AnimateEnterNamePlaceholder()
        {
            MainMenuUIManager.Instance.AnimatePlaceholder(enterNamePlaceholder);
        }
        

        public void OnNameFieldSelected()
        {
            InvokeRepeating(nameof(AnimateEnterNamePlaceholder), 0f, 0.5f);
            MainMenuUIManager.ToggleCarat(enterNameField);
            MainMenuUIManager.ToggleCapitalize(keyboard, enterNameField);
            keyboard.ShowKeyboard();
        }

        public void OnNameFieldDeselected()
        {
            CancelInvoke(nameof(AnimateEnterNamePlaceholder));
            enterNamePlaceholder.text = ". . .";
            enterNameField.text = enterNameField.text.Trim();
        }


        public void OnPlayerNameValueChanged()
        {
            // TODO validate player name correctly later (ale jak lepiej??? - Wera)

            CancelInvoke(nameof(AnimateEnterNamePlaceholder));
            MainMenuUIManager.ToggleCapitalize(keyboard, enterNameField);
            MainMenuUIManager.ToggleCarat(enterNameField);

            // Play placeholder animation if no text input present
            if (enterNameField.text.Length == 0)
            {
                InvokeRepeating(nameof(AnimateEnterNamePlaceholder), 0f, 0.5f);
            }

            // Toggle confirm name button if input is correct
            confirmNameButton.SetActive(enterNameField.text != "");

            // Enter shortcut key implementation
            if (!enterNameField.text.EndsWith("\n") || enterNameField.text.Length <= 2) return;
            OnConfirmNameButtonClicked();
        }

        public void OnConfirmNameButtonClicked()
        {
            MainMenuUIManager.Instance.SetName(enterNameField.text.Trim());
            ScreenChanger.Instance.ChangeToBrowseLobbiesScreen();
        }
    }
}