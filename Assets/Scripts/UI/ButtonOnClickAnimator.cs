using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UI;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ButtonOnClickAnimator : MonoBehaviour, IPointerClickHandler
{
    public float animationTime;
    public bool changesScreen;
    public GameObject screenToChangeTo = null;
    public TextMeshProUGUI animationMaxWidthRender;
    public TextMeshProUGUI text;
    public RectTransform textParent;

    private float _animationMaxWidth;
    private const string MaxWidthKeyFrame = "....";
    private string _text;
    private RectTransform _textRectTransform;


    private TextMeshProUGUI _buttonText;
    private void Start()
    {
        _buttonText = GetComponentInChildren<TextMeshProUGUI>();
        _animationMaxWidth = GetAnimationMaxWidth();
        _textRectTransform = text.gameObject.GetComponent<RectTransform>();
        _text = text.text;
        Debug.Log(_animationMaxWidth);
        _textRectTransform.sizeDelta = new Vector2(_animationMaxWidth, _textRectTransform.sizeDelta.y);
    }

    private void OnDisable()
    {
        text.text = _text;
    }

    private float GetAnimationMaxWidth()
    {
        animationMaxWidthRender.text = text.text + MaxWidthKeyFrame;
        return animationMaxWidthRender.preferredWidth;
    }

    public void OnClickAnimation()
    {
        StartCoroutine(AnimateOnClick());
    }

    private IEnumerator AnimateOnClick()
    {
        _buttonText.text += ".";
        yield return new WaitForSeconds(animationTime);
        _buttonText.text = _buttonText.text[..^1];
        if (changesScreen)
        {
            ScreenChanger.Instance.ChangeTo(screenToChangeTo.name);
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        StartCoroutine(AnimateOnClick());
    }
}
