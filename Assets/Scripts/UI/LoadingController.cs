using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace UI
{

    public class LoadingController : MonoBehaviour
    {
        public TextMeshProUGUI dots;

        private void AnimateDots()
        {
            AnimatePlaceholder(dots);
        }

        public void Start()
        {
            InvokeRepeating(nameof(AnimateDots), 0f, 0.5f);
        }
        
        public void AnimatePlaceholder(TextMeshProUGUI placeholderText)
        {
            placeholderText.text = placeholderText.text.Length switch
            {
                0 => ".",
                1 => ". .",
                3 => ". . .",
                5 => "",
                _ => "."
            };
        }

        public void OnDisable()
        {
            CancelInvoke(nameof(AnimateDots));
        }
    }
}
