using UnityEngine;

namespace UI
{
    public class RollCredits : MonoBehaviour
    {
        public GameObject screen;
        public float scrollSpeed;

        private float _finY;
        private float _currentY;
        private RectTransform _rectTransform;
        private RectTransform _screenRectTransform;
        private RectTransform _rectTransform2;

        private void Start()
        {
            _screenRectTransform = screen.GetComponent<RectTransform>();
            _rectTransform = GetComponent<RectTransform>();
            _currentY = -screen.GetComponent<RectTransform>().sizeDelta.y;

            GetComponent<RectTransform>().anchoredPosition = new Vector2(0, _currentY);
        }

        private void Update()
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

        // FOR LATER USE
        // GameObject playerName = Instantiate(textPrefab, credits.transform);
        //     foreach (var text in playerName.GetComponentsInChildren<TextMeshProUGUI>())
        // {
        //     if (text.gameObject.name == "Text")
        //     {
        //         text.text = input.text;
        //     }
        // }
        // ScreenChanger.Instance.ChangeToLobbyPlayerScreen();
    }
}