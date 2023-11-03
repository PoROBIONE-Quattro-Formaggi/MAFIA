using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using TMPro;

public class InputCode : MonoBehaviour
{
    public TMP_InputField input;

    public void mono()
    {
        Debug.Log(!input.text.StartsWith("<mspace=2.75em>"));
        Debug.Log(input.text);
        if (!input.text.StartsWith("<mspace=2.75em>"))
        {
            input.text = "<mspace=2.75em>" + input.text;
            input.caretPosition = input.text.Length;
        }
    }
}
