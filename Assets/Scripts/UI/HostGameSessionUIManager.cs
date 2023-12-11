using UnityEngine;

namespace UI
{
    public class HostGameSessionUIManager : MonoBehaviour
    {
        private void OnEnable()
        {
            ScreenChanger.Instance.ChangeToHostGameScreen();
        }
    }
}