using TMPro;
using UnityEngine;

namespace UI
{
    public class PlayerLobbyController : MonoBehaviour
    {
        public TextMeshProUGUI informationText;

        private void OnEnable()
        {
            InvokeRepeating(nameof(UpdateWelcomePrompt), 0f, 1f);
        }

        private void UpdateWelcomePrompt()
        {
            SetWelcomePrompt(MainMenuUIManager.Instance.GetName());
        }

        public void SetWelcomePrompt(string playerName)
        {
            informationText.text = $"You are {playerName}, please wait";
        }
    }
}