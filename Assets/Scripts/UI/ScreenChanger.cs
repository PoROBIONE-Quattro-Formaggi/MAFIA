using DataStorage;
using Managers;
using UnityEngine;

namespace UI
{
    public class ScreenChanger : MonoBehaviour
    {
        public GameObject screensParent;

        private void DisableAll()
        {
            foreach (Transform screenTransform in screensParent.transform)
            {
                screenTransform.gameObject.SetActive(false);
            }
        }

        private void ChangeTo(string screenName)
        {
            DisableAll();
            foreach (Transform screenTransform in screensParent.transform)
            {
                if (screenTransform.gameObject.name != screenName) continue;
                screenTransform.gameObject.SetActive(true);
            }
        }

        public void ChangeToJoinLobbyScreen()
        {
            LobbyManager.Instance.AssignLobbiesToButtons();
            ChangeTo(Screens.JoinLobby);
        }
        
        public void ChangeToMainScreen()
        {
            ChangeTo(Screens.MainScreen);
        }

        public void ChangeToCreateLobbyScreen()
        {
            ChangeTo(Screens.CreateLobbyScreen);
        }

        public void ChangeToEnterCodeScreen()
        {
            ChangeTo(Screens.EnterCodeScreen);
        }

        public void ChangeToSetNameScreen()
        {
            ChangeTo(Screens.SetNameScreen);
        }
    }
}
