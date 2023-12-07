using DataStorage;
using Managers;
using UnityEngine;
using TMPro;

namespace UI
{
    public class ClientGameSessionUIManager : MonoBehaviour
    {
        public GameObject infoBar;
        public GameObject alibiPrompt;
        public GameObject alibiInput;
        public GameObject goVoteButton;
        public GameObject okButton;
        public TextMeshProUGUI rolePromptText;
        public GameObject rolePrompt;

        public RoleController roleController;
        
        private void OnEnable()
        {
            Debug.Log($"On enable client called. IsPlayerRoleAssigned: {NetworkCommunicationManager.Instance.IsPlayerRoleAssigned}");
            if (!NetworkCommunicationManager.Instance.IsPlayerRoleAssigned)
            {
                ScreenChanger.Instance.ChangeToPlayerRoleScreen();
                NetworkCommunicationManager.Instance.OnPlayerRoleAssigned += SetPlayerRoleToPrompt;
            }
            else
            {
                ScreenChanger.Instance.ChangeToPlayerGameScreen();
            }
        }

        private void SetPlayerRoleToPrompt()
        {
            roleController.DisplayRole(PlayerData.Role);
            NetworkCommunicationManager.Instance.OnPlayerRoleAssigned -= SetPlayerRoleToPrompt;
        }
    }
}