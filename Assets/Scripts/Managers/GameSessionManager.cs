using DataStorage;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Managers
{
    public class GameSessionManager : MonoBehaviour
    {
        public static GameSessionManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<GameSessionManager>();
                }

                return _instance;
            }
        }

        private static GameSessionManager _instance;

        private void Awake()
        {
            if (_instance == null)
            {
                _instance = this;
                transform.SetParent(null);
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void OnEnable()
        {
            SceneManager.sceneLoaded += OnSceneChanged;
        }

        private static void OnSceneChanged(Scene scene, LoadSceneMode loadSceneMode)
        {
            if (SceneManager.GetActiveScene().name != Scenes.GameScene) return;
            var joinCode = PlayerPrefs.GetString(PpKeys.KeyStartGame);
            var isHost = PlayerPrefs.GetInt(PpKeys.KeyIsHost);
            if (joinCode == "0") // Error - no join code was registered
            {
                SceneChanger.ChangeToMainScene();
            }
            else if (isHost == 1)
            {
                RelayManager.Instance.CreateRelay(); // Here NetworkManager automatically connects as a Host I think
                // NetworkCommunicationManager.StartHost();
            }
            else
            {
                RelayManager.JoinRelay(joinCode);
                // NetworkCommunicationManager.StartClient(); TODO chyba to wyłączone IDK
            }
        }

        private void OnDisable()
        {
            // Unsubscribe to prevent memory leaks
            SceneManager.sceneLoaded -= OnSceneChanged;
        }
    }
}