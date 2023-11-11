using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeyboardController : MonoBehaviour
{
    private RectTransform _keyboardTransform;

    private void Start()
    {
        _keyboardTransform = GetComponent<RectTransform>();
    }

    public void ShowKeyboard()
    {
        if(!Application.isMobilePlatform) return;
        _keyboardTransform.anchoredPosition = new Vector2(0, _keyboardTransform.sizeDelta.y);
    }

    public void HideKeyboard()
    {
        if(!Application.isMobilePlatform) return;
        _keyboardTransform.anchoredPosition = new Vector2(0, 0);
    }
}
