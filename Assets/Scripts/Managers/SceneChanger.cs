using DataStorage;
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
    }
}