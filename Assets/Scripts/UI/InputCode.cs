using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using TMPro;

public class InputCode : MonoBehaviour
{
    public TMP_InputField input;

    private bool selected;

    public void mono()
    {
        if (!input.text.StartsWith("<mspace=2.75em>"))
        {
            input.text = "<mspace=2.75em>" + input.text;
        }
        input.caretPosition = input.text.Length;
    }

    public void remmono()
    {
        Debug.Log("here queer");
        if (input.text == "<mspace=2.75em>")
        {
            input.text = "";
        }
    }
}
