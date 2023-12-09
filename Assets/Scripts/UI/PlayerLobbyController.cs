using System;
using System.Collections;
using System.Collections.Generic;
using Managers;
using TMPro;
using UnityEngine;

namespace UI
{
    public class PlayerLobbyController : MonoBehaviour
    {
        public TextMeshProUGUI informationText;

        private void OnEnable()
        {
            LobbyManager.Instance.OnHostMigrated += HandleHostMigration;
            InvokeRepeating(nameof(UpdateWelcomePrompt),0f,1f);
        }

        private void UpdateWelcomePrompt()
        {
            SetWelcomePrompt(MainMenuUIManager.Instance.GetName());
        }

        private void HandleHostMigration()
        {
            ScreenChanger.Instance.ChangeToLobbyHostScreen();
        }

        public void SetWelcomePrompt(string playerName)
        {
            informationText.text = $"You are {playerName}, please wait";
        }

        private void OnDisable()
        {
            LobbyManager.Instance.OnHostMigrated -= HandleHostMigration;
        }
    }
}