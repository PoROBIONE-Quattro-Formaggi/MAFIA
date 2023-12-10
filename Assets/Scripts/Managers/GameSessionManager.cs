using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DataStorage;
using Third_Party.Toast_UI.Scripts;
using Unity.Collections;
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
        public string LastKilledName { get; set; } = "";
        public string LastWords { get; set; } = "";
        public string NarratorComment { get; set; } = "";
        public string CurrentNightResidentsQuestion { get; set; } = "";
        public List<string> CurrentNightResidentsAnswerOptions { get; set; } = new();
        public string NightResidentsPollChosenAnswer { get; set; } = "";
        public string CurrentTimeOfDay { get; set; } = TimeIsAManMadeSocialConstruct.Night;
        public string WinnerRole { get; set; } = "";
        public List<ulong> ClientsIDs { get; } = new();
        public event Action OnPlayersAssignedToRoles;
        public event Action OnHostEndGame;

        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // PRIVATE FIELDS ///////////////////////////////////////////////////////////////////////////////////////////////////
        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        private static GameSessionManager _instance;
        private ulong CurrentDayVotedID { get; set; }

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
                if (RelayManager.Instance.CreateRelay()) return;
                Toast.Show("Cannot create the game");
                SceneChanger.ChangeToMainScene();
            }
            else
            {
                if (await RelayManager.JoinRelay(joinCode))
                {
                    // PlayerPrefs.SetString(PpKeys.KeyStartGame, "0");
                    // PlayerPrefs.Save();
                    Debug.Log("[GameSessionManager] Joining to the Relay");
                    return;
                }

                Toast.Show("Cannot join to the game");
                SceneChanger.ChangeToMainScene();
            }
        }

        public async void ReconnectToGame()
        {
            var joinCode = PlayerPrefs.GetString(PpKeys.KeyStartGame);
            var isHost = PlayerPrefs.GetInt(PpKeys.KeyIsHost);
            if (joinCode == "0" || isHost == 1)
            {
                Toast.Show("Cannot join to the game");
                SceneChanger.ChangeToMainScene();
            }
            else
            {
                if (await RelayManager.JoinRelay(joinCode))
                {
                    Debug.Log("[GameSessionManager] Joining to the Relay");
                    NetworkCommunicationManager.Instance.EmergencyEndGameServerRpc();
                    return;
                }

                Toast.Show("Cannot join to the game");
                SceneChanger.ChangeToMainScene();
            }
        }

        public void EmergencyEndGame()
        {
            EndGame("DRAW");
        }

        public void OnNewClientConnected(ulong clientId)
        {
            if (NetworkCommunicationManager.GetOwnClientID() == clientId) return;
            IDToIsPlayerAlive[clientId] = true;
            Debug.Log("CURRENT ALIVE IDS:");
            foreach (var key in IDToIsPlayerAlive.Keys)
            {
                Debug.Log(key);
            }

            var keys = IDToIsPlayerAlive.Keys.ToArray();
            var values = IDToIsPlayerAlive.Values.ToArray();
            Debug.Log($"[GameSessionManager] (OnNewClientConnected) all player 'isAlives':");
            foreach (var keyVal in IDToIsPlayerAlive)
            {
                Debug.Log($"{keyVal.Key} - {keyVal.Value}");
            }

            NetworkCommunicationManager.Instance.SendNewIDToIsPlayerAliveClientRpc(keys, values);
            var expectedNumberOfPlayers = PlayerPrefs.GetInt(PpKeys.KeyPlayersNumber);
            Debug.Log($"expected number of players: {expectedNumberOfPlayers}");
            var currentNumberOfPlayers = IDToIsPlayerAlive.Count;
            Debug.Log($"current number of players: {currentNumberOfPlayers}");
            if (expectedNumberOfPlayers != currentNumberOfPlayers) return;
            AssignPlayersToRoles();
        }

        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // HELPER FUNCTIONS /////////////////////////////////////////////////////////////////////////////////////////////////
        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        private void AssignPlayersToRoles()
        {
            var hostID = NetworkCommunicationManager.Instance.OwnerClientId;
            var playersIDs = NetworkCommunicationManager
                .GetAllConnectedPlayersIDs()
                .Where(id => id != hostID)
                .ToList();
            var expectedNumberOfPlayers = PlayerPrefs.GetInt(PpKeys.KeyPlayersNumber);
            if (expectedNumberOfPlayers != playersIDs.Count) return;
            foreach (var id in playersIDs)
            {
                ClientsIDs.Add(id);
            }

            var playersIDsToAssignRoles = ClientsIDs.ToList();
            Shuffle(playersIDsToAssignRoles);
            var rolesList = GetRolesList(playersIDsToAssignRoles.Count);
            for (var i = 0; i < rolesList.Count; i++)
            {
                IDToRole[playersIDsToAssignRoles[i]] = rolesList[i];
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
                switch (playerNumber)
                {
                    case <= 10:
                        mafiaNumber = 1;
                        doctorNumber = 1;
                        break;
                    case <= 25:
                        mafiaNumber = 2;
                        doctorNumber = 1;
                        break;
                    case <= 50:
                        mafiaNumber = 3;
                        doctorNumber = 2;
                        break;
                    case <= 100:
                        mafiaNumber = 4;
                        doctorNumber = 2;
                        break;
                    default:
                        mafiaNumber = 1;
                        doctorNumber = 1;
                        break;
                }

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

        private ulong GetWhoMafiaVotedID()
        {
            Debug.Log($"count of mafia vote dictionary: {MafiaIDToVotedForID.Count}");
            var occurrences = MafiaIDToVotedForID.Values
                .GroupBy(v => v)
                .ToDictionary(g => g.Key, g => g.Count());

            foreach (var occurrence in occurrences)
            {
                Debug.Log($"Occurrence key: {occurrence.Key}, value: {occurrence.Value}");
            }

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
                if (occurrences.Count == 0)
                {
                    return 0;
                }

                var chosenOption = occurrences.OrderByDescending(x => x.Value).First().Key;
                return chosenOption;
            }
        }

        private void ClearDataFromLastNightVoting()
        {
            MafiaIDToVotedForID.Clear();
            DoctorIDToVotedForID.Clear();
            IDToAlibi.Clear();
            LastKilledName = "";
            ResidentIDToVotedForOption.Clear();
            NightResidentsPollChosenAnswer = "";
            NetworkCommunicationManager.Instance.ClearDataFromLastNightVotingClientRpc();
        }

        private void ClearDataFromLastDayVoting()
        {
            IDToVotedForID.Clear();
            LastKilledName = "";
            LastWords = "";
            CurrentDayVotedID = 0;
            NetworkCommunicationManager.Instance.ClearDataFromLastDayVotingClientRpc();
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
            var questionAnswersPair = ResidentsNightQuestions.GetRandomQuestionWithAnswers();
            CurrentNightResidentsQuestion = questionAnswersPair.Key;
            CurrentNightResidentsAnswerOptions = questionAnswersPair.Value;
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
            WinnerRole = winnerRole;
            OnHostEndGame?.Invoke();
            NetworkCommunicationManager.Instance.EndGameForClientsClientRpc(winnerRole);
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

        public List<ulong> GetAlivePlayersIDs(bool includeYourself = true)
        {
            if (includeYourself)
            {
                return IDToIsPlayerAlive
                    .Where(keyVal => keyVal.Value)
                    .Select(keyVal => keyVal.Key)
                    .ToList();
            }

            return IDToIsPlayerAlive
                .Where(keyVal => keyVal.Value && keyVal.Key != PlayerData.ClientID)
                .Select(keyVal => keyVal.Key)
                .ToList();
        }

        public List<ulong> GetAliveNonMafiaPlayersIDs()
        {
            var aliveIDs = GetAlivePlayersIDs(false);
            return aliveIDs.Where(id => IDToRole[id] != Roles.Mafia).ToList();
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
            Debug.Log($"Sending server RPC with mafia vote for: {votedForID}");
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
            Debug.Log($"alibi sent to server: {alibi}");
            IDToAlibi[PlayerData.ClientID] = alibi;
            NetworkCommunicationManager.Instance.SetAlibiServerRpc(alibi);
        }

        public int GetCurrentAmountOfMafiaThatVoted()
        {
            return MafiaIDToVotedForID.Count;
        }

        public int GetAmountOfAliveMafia()
        {
            Debug.Log("ALL ALIVE PLAYERS IDS BELOW:");
            foreach (var id in GetAlivePlayersIDs())
            {
                Debug.Log(id);
            }

            return GetAlivePlayersIDs().Count(id => IDToRole[id] == Roles.Mafia);
        }

        public int GetCurrentAmountOfDoctorsThatVoted()
        {
            return DoctorIDToVotedForID.Count;
        }

        public int GetAmountOfAliveDoctors()
        {
            Debug.Log("ALL ALIVE PLAYERS IDS BELOW:");
            foreach (var id in GetAlivePlayersIDs())
            {
                Debug.Log(id);
            }

            return GetAlivePlayersIDs().Count(id => IDToRole[id] == Roles.Doctor);
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
            if (!NetworkCommunicationManager.Instance.IsHost)
            {
                Toast.Show("You are not a host - you don't have permission to end night");
                return;
            }

            ClearDataFromLastDayVoting();
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

            // AssignNightResidentsPollChosenAnswer(); TODO TURN ON LATER
            NightResidentsPollChosenAnswer = "[TEST] Chosen poll answer";
            CurrentTimeOfDay = TimeIsAManMadeSocialConstruct.Day;
            NetworkCommunicationManager.Instance.BeginDayForClientsClientRpc();
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
        public void EndDay()
        {
            // - Clear all the voting variables to be ready for next voting - DONE
            // - Calculate who won the voting - DONE
            // - Kill player - DONE
            // - Check if game ended - DONE
            // - Send new night residents questions to clients (RCP) - DONE
            ClearDataFromLastNightVoting();
            var playersChoiceID = GetWhoPlayersDayVotedID();
            CurrentDayVotedID = playersChoiceID;
            KillPlayerWithID(playersChoiceID);
            EndGameIfApplicable();
            SendNewResidentsNightPoll();
            CurrentTimeOfDay = TimeIsAManMadeSocialConstruct.Evening;
            NetworkCommunicationManager.Instance.BeginEveningForClientsClientRpc();
        }

        public ulong GetCurrentDayVotedID()
        {
            return CurrentDayVotedID;
        }

        public int GetCurrentAmountOfResidentsThatDayVoted()
        {
            return IDToVotedForID.Count;
        }

        public int GetAmountOfAliveResidents()
        {
            return GetAlivePlayersIDs(false).Count;
        }

        /// <summary>
        /// Host-only function
        /// </summary>
        public void EndEvening()
        {
            // - Allow this player to put last words - DONE
            // - Show last words - DONE
            CurrentTimeOfDay = TimeIsAManMadeSocialConstruct.Night;
            NetworkCommunicationManager.Instance.BeginNightForClientsClientRpc();
        }


        /// <summary>
        /// Host-only function
        /// </summary>
        /// <param name="comment">Comment to set</param>
        public void SetNarratorComment(string comment)
        {
            NarratorComment = comment;
            NetworkCommunicationManager.Instance.SendNarratorCommentClientRpc(new FixedString64Bytes(comment));
        }

        public string GetNarratorComment()
        {
            return NarratorComment;
        }

        public void SetLastWords(string lastWords)
        {
            Debug.Log($"Sending server rpc with {lastWords}");
            NetworkCommunicationManager.Instance.SetLastWordsServerRpc(lastWords);
        }

        public string GetLastWords()
        {
            return LastWords;
        }

        public string GetCurrentTimeOfDay()
        {
            return CurrentTimeOfDay;
        }

        public string GetWinnerRole()
        {
            return WinnerRole;
        }

        public void ClearAllDataForEndGame()
        {
            IDToRole.Clear();
            IDToPlayerName.Clear();
            IDToIsPlayerAlive.Clear();
            IDToAlibi.Clear();
            MafiaIDToVotedForID.Clear();
            DoctorIDToVotedForID.Clear();
            ResidentIDToVotedForOption.Clear();
            IDToVotedForID.Clear();
            LastKilledName = "";
            LastWords = "";
            NarratorComment = "";
            CurrentNightResidentsQuestion = "";
            CurrentNightResidentsAnswerOptions.Clear();
            NightResidentsPollChosenAnswer = "";
            CurrentTimeOfDay = TimeIsAManMadeSocialConstruct.Night;
            WinnerRole = "";
            ClientsIDs.Clear();
            CurrentDayVotedID = 0;
            PlayerData.ClientID = ulong.MaxValue;
            PlayerData.Name = "";
            PlayerData.Role = "";
            PlayerData.IsAlive = false;
        }
    }
}