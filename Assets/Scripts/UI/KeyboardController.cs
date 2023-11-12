using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UI;

public class KeyboardController : MonoBehaviour
{
    public RectTransform screenRect;
    public TMP_InputField inputField;
    
    [Header("Variables")]
    public bool caps;

    public float heightOnScreen;

    public float spacingRatio;
    
    
    private RectTransform _keyboardTransform;
    private List<HorizontalLayoutGroup> _rows = new List<HorizontalLayoutGroup>();
    private List<TextMeshProUGUI> _chars = new List<TextMeshProUGUI>();

    

    private void Start()
    {
        _keyboardTransform = GetComponent<RectTransform>();
        
        connectKeys();
        
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
                _rows[i].spacing = screenRect.sizeDelta.x / (spacingRatio / 10);
            }
            else
            {
                _rows[i].spacing = screenRect.sizeDelta.x / spacingRatio;
            }
        }
    }

    public void connectKeys()
    {
        // Connect key scripts with input field
        foreach (var keyScript in _keyboardTransform.GetComponentsInChildren<InputChar>())
        {
            keyScript.inputField = inputField;
        }
        
        // Connect delete with input field
        _keyboardTransform.GetComponentInChildren<DeleteKey>().inputField = inputField;

        // Connect enter with input field
        _keyboardTransform.GetComponentInChildren<EnterKey>().inputField = inputField;
    }

    public void ShowKeyboard()
    {
        //if(!Application.isMobilePlatform) return;
        _keyboardTransform.anchoredPosition = new Vector2(0, _keyboardTransform.sizeDelta.y + heightOnScreen);
    }

    public void HideKeyboard()
    {
        //if(!Application.isMobilePlatform) return;
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
