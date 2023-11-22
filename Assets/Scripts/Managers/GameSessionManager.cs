using System;
using System.Collections.Generic;
using System.Linq;
using DataStorage;
using Third_Party.Toast_UI.Scripts;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;


namespace Managers
{
    public class GameSessionManager : MonoBehaviour
    {
        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // PUBLIC FIELDS ////////////////////////////////////////////////////////////////////////////////////////////////////
        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

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
        public Dictionary<ulong, string> IDToAlibi { get; } = new();
        public Dictionary<ulong, ulong> MafiaIDToVotedForID { get; } = new();
        public Dictionary<ulong, ulong> DoctorIDToVotedForID { get; } = new();
        public Dictionary<ulong, int> ResidentIDToVotedForOption { get; } = new();
        public Dictionary<ulong, ulong> IDToVotedForID { get; } = new();
        public ulong CurrentDayVotedID { get; set; }
        public string LastKilledName { get; set; } = "";
        public string LastWords { get; set; } = "";
        public string NarratorComment { get; set; } = "";
        public string CurrentNightResidentsQuestion { get; set; } = "";
        public List<string> CurrentNightResidentsAnswerOptions { get; set; } = new();
        public string NightResidentsPollChosenAnswer { get; set; } = "";
        public event Action OnPlayersAssignedToRoles;

        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // PRIVATE FIELDS ///////////////////////////////////////////////////////////////////////////////////////////////////
        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        private static GameSessionManager _instance;
        private List<ulong> ClientsIDs { get; } = new();

        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // MONO BEHAVIOUR FUNCTIONS /////////////////////////////////////////////////////////////////////////////////////////
        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

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

        private void OnDisable()
        {
            // Unsubscribe to prevent memory leaks
            SceneManager.sceneLoaded -= OnSceneChanged;
        }

        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // ON EVENT FUNCTIONS ///////////////////////////////////////////////////////////////////////////////////////////////
        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

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
                NetworkManager.Singleton.OnClientConnectedCallback += OnNewClientConnected;
                // IDToRole.CollectionChanged += OnIDToRoleChanged;
                // IDToPlayerName.CollectionChanged += OnIDToPlayerNameChanged;
                // IDToIsPlayerAlive.CollectionChanged += OnIDToIsPlayerAliveChanged;
                // IDToAlibi.CollectionChanged += OnIDToIsAlibiChanged;
                // MafiaIDToVotedForID.CollectionChanged += OnMafiaIDToVoteForIDChanged;
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

        // private void OnIDToRoleChanged(object sender, NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs)
        // {
        //     var keys = IDToRole.Keys.ToArray();
        //     var values = IDToRole.Values.Select(value => new FixedString32Bytes(value)).ToArray();
        //     NetworkCommunicationManager.Instance.SendNewIDToRoleClientRpc(keys, values);
        // }
        //
        // private void OnIDToPlayerNameChanged(object sender,
        //     NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs)
        // {
        //     var keys = IDToPlayerName.Keys.ToArray();
        //     var values = IDToRole.Values.Select(value => new FixedString32Bytes(value)).ToArray();
        //     NetworkCommunicationManager.Instance.SendNewIDToPlayerNameClientRpc(keys, values);
        // }
        //
        // private void OnIDToIsPlayerAliveChanged(object sender,
        //     NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs)
        // {
        //     var keys = IDToIsPlayerAlive.Keys.ToArray();
        //     var values = IDToIsPlayerAlive.Values.ToArray();
        //     NetworkCommunicationManager.Instance.SendNewIDToIsPlayerAliveClientRpc(keys, values);
        // }
        //
        // private void OnIDToIsAlibiChanged(object sender,
        //     NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs)
        // {
        //     var keys = IDToAlibi.Keys.ToArray();
        //     var values = IDToAlibi.Values.Select(value => new FixedString128Bytes(value)).ToArray();
        //     NetworkCommunicationManager.Instance.SendNewIDToAlibiClientRpc(keys, values);
        // }
        //
        // private void OnMafiaIDToVoteForIDChanged(object sender,
        //     NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs)
        // {
        //     var keys = MafiaIDToVotedForID.Keys.ToArray();
        //     var values = MafiaIDToVotedForID.Values.ToArray();
        //     var clientRpcParams = new ClientRpcParams
        //     {
        //         Send = new ClientRpcSendParams
        //         {
        //             TargetClientIds = keys.ToList()
        //         }
        //     };
        //     NetworkCommunicationManager.Instance.SendNewMafiaIDToVotedForIDClientRpc(keys, values, clientRpcParams);
        // }

        private void OnNewClientConnected(ulong clientId)
        {
            Debug.Log($"New client connected with id: {clientId}");
            AssignPlayersToRoles();
        }

        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // HELPER FUNCTIONS /////////////////////////////////////////////////////////////////////////////////////////////////
        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        private void AssignPlayersToRoles()
        {
            var playersIDs = NetworkCommunicationManager.GetAllConnectedPlayersIDs();
            var expectedNumberOfPlayers = PlayerPrefs.GetInt(PpKeys.KeyPlayersNumber);
            if (expectedNumberOfPlayers != playersIDs.Count) return;
            NetworkManager.Singleton.OnClientConnectedCallback -= OnNewClientConnected;
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
            return;

            List<string> GetRolesList(int playerNumber)
            {
                var tempRolesList = new List<string>(playerNumber);
                foreach (var field in typeof(Roles).GetFields())
                {
                    var roleName = field.GetValue(typeof(Roles));
                    switch (roleName)
                    {
                        case Roles.Mafia:
                            for (var i = 0; i < mafiaNumber; i++)
                            {
                                tempRolesList.Add(Roles.Mafia);
                            }

                            break;
                        case Roles.Doctor:
                            for (var i = 0; i < doctorNumber; i++)
                            {
                                tempRolesList.Add(Roles.Doctor);
                            }

                            break;
                        case Roles.Resident:
                            for (var i = 0; i < playerNumber - doctorNumber - mafiaNumber; i++)
                            {
                                tempRolesList.Add(Roles.Resident);
                            }

                            break;
                    }
                }

                return tempRolesList;
            }

            static void Shuffle(IList<ulong> listToShuffle)
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
        }

        private List<ulong> GetAlivePlayersIDs()
        {
            return IDToIsPlayerAlive
                .Where(keyVal => keyVal.Value)
                .Select(keyVal => keyVal.Key)
                .ToList();
        }

        private ulong GetWhoMafiaVotedID()
        {
            var occurrences = MafiaIDToVotedForID.Values
                .GroupBy(v => v)
                .ToDictionary(g => g.Key, g => g.Count());
            var mafiaChoice = occurrences.OrderByDescending(x => x.Value).First().Key;
            return mafiaChoice;
        }

        private void AssignNightResidentsPollChosenAnswer()
        {
            var optionResidentsVotedFor = GetOptionResidentsVotedFor();
            NightResidentsPollChosenAnswer = CurrentNightResidentsAnswerOptions[optionResidentsVotedFor];
            NetworkCommunicationManager.Instance.SendNightResidentsPollChosenAnswerClientRpc(
                NightResidentsPollChosenAnswer);
            return;

            int GetOptionResidentsVotedFor()
            {
                var occurrences = ResidentIDToVotedForOption.Values
                    .GroupBy(v => v)
                    .ToDictionary(g => g.Key, g => g.Count());
                var chosenOption = occurrences.OrderByDescending(x => x.Value).First().Key;
                return chosenOption;
            }
        }

        private void ClearDataFromLastVoting()
        {
            MafiaIDToVotedForID.Clear();
            DoctorIDToVotedForID.Clear();
            IDToVotedForID.Clear();
            IDToAlibi.Clear();
            LastKilledName = "";
            LastWords = "";
            ResidentIDToVotedForOption.Clear();
            NightResidentsPollChosenAnswer = "";
            NetworkCommunicationManager.Instance.ClearDataFromLastVotingClientRpc();
        }

        private void KillPlayerWithID(ulong id)
        {
            LastKilledName = IDToPlayerName[id];
            IDToIsPlayerAlive[id] = false;
            var keys = IDToIsPlayerAlive.Keys.ToArray();
            var values = IDToIsPlayerAlive.Values.ToArray();
            NetworkCommunicationManager.Instance.SendNewIDToIsPlayerAliveClientRpc(keys, values);
            NetworkCommunicationManager.Instance.SendLastKilledNameClientRpc(LastKilledName);
        }

        private ulong GetWhoPlayersDayVotedID()
        {
            var occurrences = IDToVotedForID.Values
                .GroupBy(v => v)
                .ToDictionary(g => g.Key, g => g.Count());
            var playersChoice = occurrences.OrderByDescending(x => x.Value).First().Key;
            return playersChoice;
        }

        private void SendNewResidentsNightPoll()
        {
            CurrentNightResidentsQuestion = "NEW QUESTION"; //TODO
            CurrentNightResidentsAnswerOptions = new List<string>(); //TODO
            NetworkCommunicationManager.Instance.SendNightResidentsQuestionClientRpc(CurrentNightResidentsQuestion);
            var options = CurrentNightResidentsAnswerOptions
                .Select(value => new FixedString64Bytes(value))
                .ToArray();
            NetworkCommunicationManager.Instance.SendNightResidentsOptionsClientRpc(options);
        }

        private void EndGameIfApplicable()
        {
            var aliveIDs = GetAlivePlayersIDs();
            var aliveNonMafia = aliveIDs.Where(id => IDToRole[id] != Roles.Mafia).ToList();
            var aliveMafia = aliveIDs.Where(id => IDToRole[id] == Roles.Mafia).ToList();
            if (aliveNonMafia.Count == 0)
            {
                EndGame(Roles.Mafia);
            }
            else if (aliveMafia.Count == 0)
            {
                EndGame(Roles.Resident);
            }
        }

        private void EndGame(string winnerRole)
        {
            Debug.Log($"THE END\n{winnerRole} wins");
            //TODO implement functionality
        }

        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // PUBLIC FUNCTIONS /////////////////////////////////////////////////////////////////////////////////////////////////
        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        public List<string> GetAlivePlayersNames(bool includeYourself = true)
        {
            var aliveIDs = includeYourself
                ? GetAlivePlayersIDs()
                : GetAlivePlayersIDs().Where(val => val != PlayerData.ClientID).ToList();

            var alivePlayerNames = IDToPlayerName
                .Where(keyVal => aliveIDs.Contains(keyVal.Key))
                .Select(keyVal => keyVal.Value)
                .ToList();
            return alivePlayerNames;
        }

        public string GetCurrentNightResidentsQuestion()
        {
            return CurrentNightResidentsQuestion;
        }

        public List<string> GetCurrentNightResidentsAnswers()
        {
            return CurrentNightResidentsAnswerOptions;
        }

        public void MafiaVoteFor(ulong votedForID)
        {
            NetworkCommunicationManager.Instance.MafiaVoteForServerRpc(votedForID);
        }

        public List<ulong> GetIDsMafiaVotedFor()
        {
            return MafiaIDToVotedForID.Values.ToList();
        }

        public void DoctorVoteFor(ulong votedForID)
        {
            NetworkCommunicationManager.Instance.DoctorVoteForServerRpc(votedForID);
        }

        public void ResidentVoteFor(int votedForOption)
        {
            NetworkCommunicationManager.Instance.ResidentVoteForServerRpc(votedForOption);
        }

        public void SetAlibi(string alibi)
        {
            NetworkCommunicationManager.Instance.SetAlibiServerRpc(alibi);
        }

        /// <summary>
        /// Host-only function
        /// </summary>
        public void EndNight()
        {
            // 1.0 Clear all the voting variables to be ready for next voting - DONE
            // 1. Calculate who to kill if not protected - DONE
            // 2. Check if game ended - DONE
            // 3. Update status of the killed player (send him an RPC and update host dictionary) - DONE
            // 4. Update 'lastDeath' (show to all who is dead) - DONE
            // 5. Show the winner in the night residents' poll - DONE
            // 5.1 Show players' alibis - DONE
            ClearDataFromLastVoting();
            var mafiaChoice = GetWhoMafiaVotedID();
            if (!DoctorIDToVotedForID.Values.Contains(mafiaChoice))
            {
                KillPlayerWithID(mafiaChoice);
                EndGameIfApplicable();
            }
            else
            {
                LastKilledName = "Nobody";
                NetworkCommunicationManager.Instance.SendLastKilledNameClientRpc(LastKilledName);
            }

            AssignNightResidentsPollChosenAnswer();
        }

        public string GetLastKilledName()
        {
            return LastKilledName;
        }

        public string GetNightResidentsPollChosenAnswer()
        {
            return NightResidentsPollChosenAnswer;
        }

        public Dictionary<ulong, string> GetAlibis()
        {
            return IDToAlibi;
        }

        public void DayVoteFor(ulong votedForID)
        {
            NetworkCommunicationManager.Instance.DayVoteForServerRpc(votedForID);
        }

        /// <summary>
        /// Host-only function
        /// </summary>
        public ulong GetIDWhoWasVotedDay()
        {
            // - Clear all the voting variables to be ready for next voting - DONE
            // - Calculate who won the voting - DONE
            // - Allow this player to put last words - TODO
            // - Kill player - DONE
            // - Check if game ended - DONE
            // - Show last words - TODO
            // - Send new night residents questions to clients (RCP) - DONE
            ClearDataFromLastVoting();
            var playersChoiceID = GetWhoPlayersDayVotedID();
            CurrentDayVotedID = playersChoiceID;
            KillPlayerWithID(playersChoiceID);
            EndGameIfApplicable();
            SendNewResidentsNightPoll();
            return playersChoiceID;
        }


        /// <summary>
        /// Host-only function
        /// </summary>
        public void SetNarratorComment(string comment)
        {
            NarratorComment = comment;
        }

        public void SetLastWords(string lastWords)
        {
            NetworkCommunicationManager.Instance.SetLastWordsServerRpc(lastWords);
        }

        public string GetLastWords()
        {
            return LastWords;
        }
    }
}