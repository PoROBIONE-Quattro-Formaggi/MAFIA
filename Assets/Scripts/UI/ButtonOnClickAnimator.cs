using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace UI
{
    public class ButtonOnClickAnimator : MonoBehaviour, IPointerClickHandler
    {
        public float animationTime;
        public bool changesScreen;
        public bool returns;
        public bool disappears;
        public GameObject screenToChangeTo = null;
        public TextMeshProUGUI animationMaxWidthRender;
        public TextMeshProUGUI text;
        public RectTransform textParent;
        public GameObject thisButton;

        private float _animationMaxWidth;
        private const string MaxWidthKeyFrame = "....";
        private string _text;
        private RectTransform _textRectTransform;
        private GameObject _thisButton;


        private TextMeshProUGUI _buttonText;
        private void Start()
        {
            _buttonText = GetComponentInChildren<TextMeshProUGUI>();
            _animationMaxWidth = GetAnimationMaxWidth();
            _textRectTransform = text.gameObject.GetComponent<RectTransform>();
            _text = text.text;
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
                Debug.Log("screen changer called");
                ScreenChanger.Instance.ChangeTo(screenToChangeTo.name);
            } else if (returns)
            {
                ScreenChanger.Instance.ChangeToPreviousScreen();
            } else if (disappears)
            {
                thisButton.SetActive(false);
            }
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            StartCoroutine(AnimateOnClick());
        }
    }
}
