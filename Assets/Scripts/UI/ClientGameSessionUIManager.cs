using DataStorage;
using Managers;
using UnityEngine;

namespace UI
{
    public class ClientGameSessionUIManager : MonoBehaviour
    {
        public RoleController roleController;
        public ScreenChanger screenChanger;

        private void OnEnable()
        {
            if (!NetworkCommunicationManager.Instance.IsPlayerRoleAssigned)
            {
                screenChanger.ChangeToPlayerRoleScreen();
                NetworkCommunicationManager.Instance.OnPlayerRoleAssigned += SetPlayerRoleToPrompt;
            }
            else
            {
                screenChanger.ChangeToPlayerGameScreen();
            }
        }

        private void SetPlayerRoleToPrompt()
        {
            roleController.DisplayRole(PlayerData.Role);
            NetworkCommunicationManager.Instance.OnPlayerRoleAssigned -= SetPlayerRoleToPrompt;
        }
    }
}