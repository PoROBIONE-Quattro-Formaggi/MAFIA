using DataStorage;
using UI;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Managers
{
    public class SceneChanger : MonoBehaviour
    {
        private static void ChangeSceneTo(string name)
        {
            SceneManager.LoadScene(name);
        }

        public static void ChangeToMainScene()
        {
            ChangeSceneTo(Scenes.MainScene);
        }

        public static void ChangeToGameScene()
        {
            ChangeSceneTo(Scenes.GameScene);
        }

        public static void ChangeToMainSceneToLobbyPlayerScreen()
        {
            ChangeToMainScene();
            ScreenChanger.Instance.ChangeToLobbyPlayerScreen();
        }

        public static void ChangeToMainSceneToLobbyHostScreen()
        {
            Debug.Log("Changing to main scene");
            ChangeToMainScene();
            Debug.Log("Changing to lobby host screen");
            ScreenChanger.Instance.ChangeToLobbyHostScreen();
        }
    }
}