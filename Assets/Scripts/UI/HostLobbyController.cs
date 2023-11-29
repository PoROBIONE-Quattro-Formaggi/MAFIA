using Managers;
using TMPro;
using UnityEngine;
using System.Runtime.InteropServices;

namespace UI
{
    public class HostLobbyController : MonoBehaviour
    {
        [DllImport("__Internal")]
        private static extern void CopyToClipboard(string str);
        
        [DllImport("__Internal")]
        private static extern void HandlePermission(string str);

        public TextMeshProUGUI information;

        private int _maxPlayers;

        public void CopyCode()
        {
            CopyToClipboard(LobbyManager.Instance.GetLobbyCode());
            information.text = "CODE COPIED - <mspace=1em>" + LobbyManager.Instance.GetLobbyCode();
        }

        private void OnEnable()
        {
            InvokeRepeating(nameof(WaitForLobby), 0f, 0.1f);
        }

        private void WaitForLobby()
        {
            _maxPlayers = LobbyManager.Instance.GetMaxPlayers();
            if (_maxPlayers == 0) return;

            // Display lobby code
            information.text = "LOBBY CODE - <mspace=1em>" + LobbyManager.Instance.GetLobbyCode();

            CancelInvoke(nameof(WaitForLobby));
        }
    }
}