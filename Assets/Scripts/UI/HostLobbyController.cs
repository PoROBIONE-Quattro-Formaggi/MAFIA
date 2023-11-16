using Managers;
using TMPro;
using UnityEngine;

namespace UI
{
    public class HostLobbyController : MonoBehaviour
    {
        public TextMeshProUGUI information;

        public void CopyCode()
        {
            LobbyManager.Instance.GetLobbyCode().CopyToClipboard();
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