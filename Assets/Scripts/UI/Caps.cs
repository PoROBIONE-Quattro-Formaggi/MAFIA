using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Unity.VisualScripting;

public class Caps : MonoBehaviour
{
    public GameObject keyboard;

    private List<TextMeshProUGUI> _chars = new List<TextMeshProUGUI>();

    private bool _caps;
    
    
    void Start()
    {
        foreach (var keyChar in keyboard.GetComponentsInChildren<TextMeshProUGUI>())
        {
            _chars.Add(keyChar);
        }
        
    }

    public void OnCapsPressed()
    {
        // toggle caps
        _caps = !_caps;
            
        if (_caps)
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
