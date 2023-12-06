using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace UI
{

    public class LoadingrController : MonoBehaviour
    {
        public TextMeshProUGUI dots;

        private void AnimateDots()
        {
            MainMenuUIManager.Instance.AnimatePlaceholder(dots);
        }

        public void Start()
        {
            InvokeRepeating(nameof(AnimateDots), 0f, 0.5f);
        }
    }
}
