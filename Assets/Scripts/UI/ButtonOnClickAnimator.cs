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
        public bool sendsInputToServer;
        public PlayerGameController playerGameController;
        public GameObject screenToChangeTo = null;
        public TextMeshProUGUI animationMaxWidthRender;
        public TextMeshProUGUI text;
        public RectTransform textRectTransform;
        public RectTransform textParent;
        public GameObject thisButton;

        private float _animationMaxWidth;
        private const string MaxWidthKeyFrame = "...";
        private string _text;
        private GameObject _thisButton;

        private void Start()
        {
            _animationMaxWidth = GetAnimationMaxWidth();
            _text = text.text;
            textRectTransform.sizeDelta = new Vector2(_animationMaxWidth, textRectTransform.sizeDelta.y);
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

        public void SetButtonText(string newText)
        {
            text.text = newText;
            _animationMaxWidth = GetAnimationMaxWidth();
            textRectTransform.sizeDelta = new Vector2(_animationMaxWidth, textRectTransform.sizeDelta.y);
        }

        public void OnClickAnimation()
        {
            StartCoroutine(AnimateOnClick());
        }

        private IEnumerator AnimateOnClick()
        {
            if (text.text.EndsWith("...")) yield break;
            text.text += ".";
            yield return new WaitForSeconds(animationTime);
            text.text = text.text[..^1];
            
            if (changesScreen)
            {
                ScreenChanger.Instance.ChangeTo(screenToChangeTo.name);
            } else if (returns)
            {
                ScreenChanger.Instance.ChangeToPreviousScreen();
            } else if (disappears && sendsInputToServer)
            {
                if (thisButton.activeSelf)
                {
                    playerGameController.SendInputToServer();
                }
                thisButton.SetActive(false);
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
