using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class ColorChanger : MonoBehaviour
{
    public GameObject background;
    public Image BackgroundImage;
    void Start()
    {
        foreach (var rect in background.GetComponentsInChildren<RectTransform>())
        {
            LeanTween.color(rect, Color.red, 1.2f);
        }
        
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
