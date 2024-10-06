using Managers;
using TMPro;
using UnityEngine;
using System.Runtime.InteropServices;
using Backend.Hub.Controllers;

namespace UI
{
    public class HostLobbyController : MonoBehaviour
    {
        [DllImport("__Internal")]
        private static extern void CopyToClipboard(string str);

        [DllImport("__Internal")]
        // ReSharper disable once UnusedMember.Local
        private static extern void HandlePermission(string str);

        public TextMeshProUGUI information;
        public GameObject playButton;

        private int _maxPlayers;
        

        public async void StartGame()
        {
            await LobbyController.Instance.StartGame();
        }

        public void CopyCode()
        {
            CopyToClipboard(LobbyController.Instance.GetLobbyCode());
            information.text = "CODE COPIED - <mspace=1em>" + LobbyController.Instance.GetLobbyCode();
        }

        private void OnEnable()
        {
            InvokeRepeating(nameof(WaitForLobby), 0f, 0.1f);
            playButton.SetActive(true);
        }

        private void WaitForLobby()
        {
            _maxPlayers = LobbyController.Instance.GetMaxPlayers();
            if (_maxPlayers == 0) return;

            // Display lobby code
            information.text = "TOWN CODE - <mspace=1em>" + LobbyController.Instance.GetLobbyCode();

            CancelInvoke(nameof(WaitForLobby));
        }
    }
}