using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DeleteKey : MonoBehaviour
{
    public TMP_InputField inputField;

    public void onDeleteKeyPressed()
    {
        if (inputField.text.Length == 0) return;
        inputField.text = inputField.text[..^1];
    }
}
