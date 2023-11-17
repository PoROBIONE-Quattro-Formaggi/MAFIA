using System;
using System.Collections.Generic;
using System.Linq;
using DataStorage;
using Third_Party.Toast_UI.Scripts;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Managers
{
    public class GameSessionManager : MonoBehaviour
    {
        public int mafiaNumber = 1;
        public int doctorNumber = 1;

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


        public Dictionary<ulong, string> IDToRole { get; } = new();

        public Dictionary<ulong, string> IDToPlayerName { get; } = new();

        public Dictionary<ulong, bool> IDToIsPlayerAlive { get; } = new();
        public List<ulong> ClientsIDs { get; } = new();
        public event Action OnPlayersAssignedToRoles;

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

        private async void OnSceneChanged(Scene scene, LoadSceneMode loadSceneMode)
        {
            if (SceneManager.GetActiveScene().name != Scenes.GameScene) return;
            var joinCode = PlayerPrefs.GetString(PpKeys.KeyStartGame);
            var isHost = PlayerPrefs.GetInt(PpKeys.KeyIsHost);
            if (joinCode == "0") // Error - no join code was registered
            {
                Toast.Show("Cannot join to the game");
                SceneChanger.ChangeToMainScene();
            }
            else if (isHost == 1)
            {
                Debug.Log("[GameSessionManager] Starting game as a host (creating Relay)");
                NetworkManager.Singleton.OnClientConnectedCallback += TryToAssignPlayersToRoles;
                if (RelayManager.Instance.CreateRelay()) return;
                Toast.Show("Cannot create the game");
                SceneChanger.ChangeToMainScene();
            }
            else
            {
                if (await RelayManager.JoinRelay(joinCode))
                {
                    PlayerPrefs.SetString(PpKeys.KeyStartGame, "0");
                    PlayerPrefs.Save();
                    Debug.Log("[GameSessionManager] Joining to the Relay");
                    return;
                }

                Toast.Show("Cannot join to the game");
                SceneChanger.ChangeToMainScene();
            }
        }

        private void TryToAssignPlayersToRoles(ulong clientId)
        {
            Debug.Log($"New client connected with id: {clientId}");
            AssignPlayersToRoles();
        }

        private void AssignPlayersToRoles()
        {
            var playersIDs = NetworkCommunicationManager.GetAllConnectedPlayersIDs();
            var expectedNumberOfPlayers = PlayerPrefs.GetInt(PpKeys.KeyPlayersNumber);
            if (expectedNumberOfPlayers != playersIDs.Count) return;
            NetworkManager.Singleton.OnClientConnectedCallback -= TryToAssignPlayersToRoles;
            var hostID = NetworkCommunicationManager.Instance.OwnerClientId;
            foreach (var id in playersIDs.Where(id => id != hostID))
            {
                ClientsIDs.Add(id);
            }

            var playersIDsToAssignRoles = ClientsIDs.ToList();
            if (playersIDsToAssignRoles.Count <
                mafiaNumber + doctorNumber + 1) //TODO delete later (debug case when players < 3
            {
                for (var i = 0; i < (mafiaNumber + doctorNumber + 1) - playersIDsToAssignRoles.Count; i++)
                {
                    playersIDsToAssignRoles.Add((ulong)(i + 100));
                }
            }

            Shuffle(playersIDsToAssignRoles);
            var rolesList = GetRolesList(playersIDsToAssignRoles.Count);
            for (var i = 0; i < rolesList.Count; i++)
            {
                IDToRole[playersIDsToAssignRoles[i]] = rolesList[i];
                IDToIsPlayerAlive[(ulong)i] = true;
            }

            Debug.Log("Players assigned with roles like:");
            foreach (var keyValPair in IDToRole)
            {
                Debug.Log($"ClientID: {keyValPair.Key} - {keyValPair.Value}");
            }

            OnPlayersAssignedToRoles?.Invoke();
        }

        private List<string> GetRolesList(int playerNumber)
        {
            var rolesList = new List<string>(playerNumber);
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