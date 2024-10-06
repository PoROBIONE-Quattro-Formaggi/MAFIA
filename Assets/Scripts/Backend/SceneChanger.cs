using DataStorage;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Backend
{
    public class SceneChanger : MonoBehaviour
    {
        private static void ChangeSceneTo(string name)
        {
            SceneManager.LoadScene(name);
        }

        public static void ChangeToMainScene()
        {
            ChangeSceneTo(Scenes.HubScene);
        }

        public static void ChangeToGameScene()
        {
            ChangeSceneTo(Scenes.GameScene);
        }
    }
}