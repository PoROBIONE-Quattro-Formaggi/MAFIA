using System;
using System.Text;
using Third_Party.Toast_UI.Scripts;
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

        private int _dotIndex = 0;
        private StringBuilder _placeholderString;

        private void Start()
        {
            _placeholderString = new StringBuilder(enterNamePlaceholder.text);
        }


        private void AnimateEnterNamePlaceholder()
        {
            if (_dotIndex == 0)
            {
                _placeholderString[_dotIndex] = ' ';
                _dotIndex += 2;
            } else if (_dotIndex > _placeholderString.Length)
            {
                _placeholderString[_dotIndex - 2] = '.';
                _dotIndex = 0;
            }
            else
            {
                _placeholderString[_dotIndex - 2] = '.';
                _placeholderString[_dotIndex] = ' ';
                _dotIndex += 2;
            }

            enterNamePlaceholder.text = _placeholderString.ToString();
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
            enterNamePlaceholder.text = ". . . . . . . . . . . . . . . . . . . . . . . . . . .";
            enterNameField.text = enterNameField.text.Trim();
        }


        public void OnPlayerNameValueChanged()
        {
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
            if (Validators.CheckIfEndsWithNewline(enterNameField.text))
            {
                OnConfirmNameButtonClicked();
            }
        }

        public void OnConfirmNameButtonClicked()
        {
            if (Validators.CheckIfNameCorrect(enterNameField.text)){
                MainMenuUIManager.Instance.SetName(enterNameField.text.Trim());
                ScreenChanger.Instance.ChangeToBrowseLobbiesScreen();
            }
            else
            {
                Toast.Show("Your name should be at least 2 characters long and no longer than 16");
            }
        }
    }
}