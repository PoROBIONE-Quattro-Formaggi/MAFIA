using System.Collections;
using System.Collections.Generic;
using TMPro;
using UI;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class InputName : MonoBehaviour
{
    public GameObject forwardButton;
    public TMP_InputField input;
    public GameObject credits;
    public GameObject textPrefab;

    public void DisplayForward()
    {
        forwardButton.SetActive(input.text != "");
    }

    public void handleJoin()
    {
        GameObject playerName = Instantiate(textPrefab, credits.transform);
        foreach (var text in playerName.GetComponentsInChildren<TextMeshProUGUI>())
        {
            if (text.gameObject.name == "Text")
            {
                text.text = input.text;
            }
        }
        ScreenChanger.Instance.ChangeToLobbyPlayerScreen();
    }


}
