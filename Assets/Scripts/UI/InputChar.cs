using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class InputChar : MonoBehaviour
{
    public TextMeshProUGUI charText;

    public TMP_InputField inputField;

    public void OnKeyPressed()
    {
        inputField.text += charText.text;
    }
}
