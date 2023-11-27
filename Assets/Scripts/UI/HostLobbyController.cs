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

        public void CopyCode()
        {
            CopyToClipboard(LobbyManager.Instance.GetLobbyCode());
            information.text = "CODE COPIED - <mspace=1em>" + LobbyManager.Instance.GetLobbyCode();
        }

    }



    public static class ClipboardExtension
    {
        /// <summary>
        /// Puts the string into the Clipboard.
        /// </summary>
        public static void CopyToClipboard(this string str)
        {
            GUIUtility.systemCopyBuffer = str;
        }
    }
}