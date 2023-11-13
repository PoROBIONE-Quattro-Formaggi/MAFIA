using System.Collections.Generic;
using System.Linq;
using DataStorage;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Managers
{
    public class GameSessionManager : MonoBehaviour
    {
        public int mafiaNumber = 1;
        public int doctorNumber = 1;
        public int residentNumber = 2;

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

        public Dictionary<ulong, string> IdxRole { get; } = new();
        public List<ulong> ClientsIds { get; private set; } = new();

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

        private void OnSceneChanged(Scene scene, LoadSceneMode loadSceneMode)
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
                InvokeRepeating(nameof(TryToAssignPlayersToRoles), 0f, 0.1f);
            }
            else
            {
                RelayManager.JoinRelay(joinCode);
            }
        }

        private void TryToAssignPlayersToRoles()
        {
            if (!NetworkCommunicationManager.Instance.IsNetworkSpawned) return;
            var hostID = NetworkCommunicationManager.Instance.OwnerClientId;
            Debug.Log($"Host ID: {hostID}");
            var playersIDs = NetworkCommunicationManager.GetAllConnectedPlayersIDs();
            Debug.Log($"Players number: {playersIDs.Count}");
            foreach (var id in playersIDs.Where(id => id == hostID))
            {
                playersIDs.Remove(id);
            }

            ClientsIds = playersIDs;
            Shuffle(playersIDs);
            var rolesList = GetRolesList(playersIDs.Count);

            for (var i = 0; i < rolesList.Count; i++)
            {
                IdxRole[playersIDs[i]] = rolesList[i];
            }

            IdxRole[hostID] = Roles.Narrator;
            Debug.Log("Assigned players");
            CancelInvoke(nameof(TryToAssignPlayersToRoles));
        }

        private List<string> GetRolesList(int playerNumber)
        {
            List<string> rolesList = new();
            foreach (var field in typeof(Roles).GetFields())
            {
                var roleName = field.GetValue(typeof(Roles));
                switch (roleName)
                {
                    case Roles.Mafia:
                        for (var i = 0; i < mafiaNumber; i++)
                        {
                            rolesList.Add(Roles.Mafia);
                        }

                        break;
                    case Roles.Doctor:
                        for (var i = 0; i < doctorNumber; i++)
                        {
                            rolesList.Add(Roles.Doctor);
                        }

                        break;
                    case Roles.Resident:
                        for (var i = 0; i < playerNumber - doctorNumber - mafiaNumber; i++)
                        {
                            rolesList.Add(Roles.Resident);
                        }

                        break;
                }
            }

            return rolesList;
        }

        private static void Shuffle(IList<ulong> listToShuffle)
        {
            var rng = new System.Random();
            var n = listToShuffle.Count;
            while (n > 1)
            {
                n--;
                var k = rng.Next(n + 1);
                (listToShuffle[k], listToShuffle[n]) = (listToShuffle[n], listToShuffle[k]);
            }
        }

        private void OnDisable()
        {
            // Unsubscribe to prevent memory leaks
            SceneManager.sceneLoaded -= OnSceneChanged;
        }
    }
}