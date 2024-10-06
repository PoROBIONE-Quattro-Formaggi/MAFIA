using DataStorage;
using UnityEngine;

namespace UI
{
    public class ScreenChanger : MonoBehaviour
    {
        // TODO: implement state transitions between screens (await server response when necessary etc.)
        
        // TODO: implement sharedCurrentActiveScreen variable -> change screen based on this (only screen changer can change this) 
        
        public static ScreenChanger Instance { get; private set; }
        public GameObject screensParent;
        private string _lastScreenName;

        public string GetLastScreenName()
        {
            return _lastScreenName;
        }

        private void Awake()
        {
            Instance = this;
        }

        private void DisableAll()
        {
            foreach (Transform screenTransform in screensParent.transform)
            {
                if (!screenTransform.gameObject.activeSelf) continue;
                _lastScreenName = screenTransform.gameObject.name;
                screenTransform.gameObject.SetActive(false);
            }
        }

        public void ChangeTo(string screenName)
        {
            DisableAll();
            foreach (Transform screenTransform in screensParent.transform)
            {
                if (screenTransform.gameObject.name != screenName) continue;

                screenTransform.gameObject.SetActive(true);
            }
        }

        public void ChangeToEndGameScreen()
        {
            ChangeTo(Screens.EndGameScreen);
        }

        public void ChangeToPlayerRoleScreen()
        {
            ChangeTo(Screens.PlayerRoleScreen);
        }

        public void ChangeToBrowseLobbiesScreen()
        {
            ChangeTo(Screens.BrowseLobbies);
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

        public void ChangeToLobbyHostScreen()
        {
            ChangeTo(Screens.LobbyHostScreen);
        }

        public void ChangeToLobbyPlayerScreen()
        {
            ChangeTo(Screens.LobbyPlayerScreen);
        }

        public void ChangeToPlayerGameScreen()
        {
            ChangeTo(Screens.PlayerGameScreen);
        }

        public void ChangeToHostGameScreen()
        {
            ChangeTo(Screens.HostGameScreen);
        }

        public void ChangeToPlayerVoteScreen()
        {
            ChangeTo(Screens.PlayerVoteScreen);
        }

        public void ChangeToInstructionScreen()
        {
            ChangeTo(Screens.InstructionScreen);
        }

        public void ChangeToErrorScreen()
        {
            ChangeTo(Screens.ErrorScreen);
        }

        public void ChangeToPreviousScreen()
        {
            if (_lastScreenName == "")
            {
                ChangeTo(Screens.MainScreen);
            }

            ChangeTo(_lastScreenName);
        }
    }
}