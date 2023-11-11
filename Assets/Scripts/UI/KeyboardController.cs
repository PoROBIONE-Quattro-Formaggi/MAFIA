using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class KeyboardController : MonoBehaviour
{
    public RectTransform screenRect;
    public bool caps;
    
    private RectTransform _keyboardTransform;
    private List<HorizontalLayoutGroup> _rows = new List<HorizontalLayoutGroup>();
    private List<TextMeshProUGUI> _chars = new List<TextMeshProUGUI>();

    

    private void Start()
    {
        _keyboardTransform = GetComponent<RectTransform>();
        foreach (var row in _keyboardTransform.GetComponentsInChildren<HorizontalLayoutGroup>())
        {
            _rows.Add(row);
        }
        
        foreach (var keyChar in GetComponentsInChildren<TextMeshProUGUI>())
        {
            _chars.Add(keyChar);
            if (caps)
            {
                keyChar.text = keyChar.text.ToUpper();
            }
        }
    }

    private void Update()
    {
        for (int i = 0; i < _rows.Count; i++)
        {
            if (i == 4)
            {
                _rows[i].spacing = screenRect.sizeDelta.x / 10;
            }
            else
            {
                _rows[i].spacing = screenRect.sizeDelta.x / 100;
            }
        }
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
    
    public void OnCapsPressed()
    {
        // toggle caps
        caps = !caps;
            
        if (caps)
        {
            foreach (var keyChar in _chars)
            {
                
                keyChar.text = keyChar.text.ToUpper();
            }
        }
        else
        {
            foreach (var keyChar in _chars)
            {
                keyChar.text = keyChar.text.ToLower();
            }
        }
    }
}
