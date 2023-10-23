using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FocusTextInput : MonoBehaviour
{
    [SerializeField] private TMP_InputField nameInput;
    // Start is called before the first frame update
    void Start()
    {
        nameInput.Select();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
