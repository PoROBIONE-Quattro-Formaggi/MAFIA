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
            ScreenChanger.Instance.ChangeToPlayerRoleScreen();
            NetworkCommunicationManager.Instance.OnPlayerRoleAssigned += SetPlayerRoleToPrompt;
        }

        private void SetPlayerRoleToPrompt()
        {
            roleController.DisplayRole(PlayerData.Role);
        }

        // BUTTON ONCLICK FUNCTIONS
        public void OnOkButtonClicked()
        {
            DisableRoleInformation();
            EnableNight();
        }

        private void DisableRoleInformation()
        {
            okButton.SetActive(false);
            rolePrompt.SetActive(false);
        }
        

        private void EnableNight()
        {
            // TODO: set information for information prompt + actually animate prompt
            infoBar.SetActive(true);
            goVoteButton.SetActive(true);
        }
        
    }
}