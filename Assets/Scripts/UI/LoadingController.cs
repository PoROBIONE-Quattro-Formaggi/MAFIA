using Managers;
using Third_Party.Toast_UI.Scripts;
using UnityEngine;
using TMPro;

namespace UI
{
    public class LoadingController : MonoBehaviour
    {
        public TextMeshProUGUI dots;
        private int _counter;

        private void AnimateDots()
        {
            AnimatePlaceholder(dots);
        }

        public void Start()
        {
            InvokeRepeating(nameof(AnimateDots), 0f, 0.5f);
            InvokeRepeating(nameof(EmergencyGoToMainMenuIfApplicable), 0f, 1f);
        }

        private async void EmergencyGoToMainMenuIfApplicable()
        {
            switch (_counter)
            {
                case >= 20:
                    CancelInvoke(nameof(EmergencyGoToMainMenuIfApplicable));
                    NetworkCommunicationManager.Instance.LeaveRelay();
                    LobbyManager.Instance.IsGameEnded = true;
                    LobbyManager.Instance.IsCurrentlyInGame = false;
                    await LobbyManager.Instance.LeaveLobby();
                    SceneChanger.ChangeToMainScene();
                    Toast.Show("This road leads to nowhere. Going back.");
                    return;
                case 10:
                    Toast.Show("Took a wrong turn, re-calculating route.");
                    break;
            }

            _counter += 1;
        }

        private void AnimatePlaceholder(TextMeshProUGUI placeholderText)
        {
            placeholderText.text = placeholderText.text.Length switch
            {
                0 => ".",
                1 => ". .",
                3 => ". . .",
                5 => "",
                _ => "."
            };
        }

        public void OnDisable()
        {
            CancelInvoke(nameof(AnimateDots));
            CancelInvoke(nameof(EmergencyGoToMainMenuIfApplicable));
        }
    }
}