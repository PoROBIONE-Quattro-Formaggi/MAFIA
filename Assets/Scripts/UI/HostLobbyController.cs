using System.Collections;
using System.Collections.Generic;
using Managers;
using UnityEngine;
using TMPro;
using UI;

public class HostLobbyController : MonoBehaviour
{
    public TextMeshProUGUI information;

    public void copyCode()
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
