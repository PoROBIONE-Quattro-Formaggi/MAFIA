using DataStorage;
using Managers;
using UnityEngine;

namespace UI
{
    public class InstructionController : MonoBehaviour
    {
        private void OnEnable()
        {
            if (ScreenChanger.Instance.GetLastScreenName() == Screens.PlayerRoleScreen)
            {
                NetworkCommunicationManager.Instance.OnDayBegan += ThrowToGame;
            }
        }

        private void OnDisable()
        {
            NetworkCommunicationManager.Instance.OnDayBegan -= ThrowToGame;
        }

        private void ThrowToGame()
        {
            ScreenChanger.Instance.ChangeToPlayerGameScreen();
        }
    }
}