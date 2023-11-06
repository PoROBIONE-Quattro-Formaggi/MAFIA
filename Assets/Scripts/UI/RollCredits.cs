using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RollCredits : MonoBehaviour
{
    public GameObject screen;
    public float scrollSpeed;

    private float _finY;
    private float _currentY;
    private Vector2 _localPosition;
    private RectTransform _rectTransform;
    private RectTransform _screenRectTransform;
    private RectTransform _rectTransform2;

    // Start is called before the first frame update
    void Start()
    {
        _screenRectTransform = screen.GetComponent<RectTransform>();
        _rectTransform = GetComponent<RectTransform>();
        _localPosition = GetComponent<RectTransform>().anchoredPosition;
        _currentY = -screen.GetComponent<RectTransform>().sizeDelta.y;

        GetComponent<RectTransform>().anchoredPosition = new Vector2(0, _currentY);
        
    }

    // Update is called once per frame
    void Update()
    {
        if (_currentY < _rectTransform.sizeDelta.y)
        {
            _currentY += 1 * scrollSpeed;
            
        }
        else
        {
            _currentY = -_screenRectTransform.sizeDelta.y;
        }
        
        _rectTransform.anchoredPosition = new Vector2(0, _currentY);
        
        
    }
}
