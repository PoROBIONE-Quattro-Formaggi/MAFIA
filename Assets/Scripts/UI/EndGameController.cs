using System;
using System.Collections;
using System.Collections.Generic;
using Managers;
using TMPro;
using UnityEngine;

namespace UI
{
    public class EndGameController : MonoBehaviour
    {
        public TextMeshProUGUI informationText;

        private void OnEnable()
        {
            informationText.text = $"The {GameSessionManager.Instance.GetWinnerRole()} wins.";
        }
    }
}

