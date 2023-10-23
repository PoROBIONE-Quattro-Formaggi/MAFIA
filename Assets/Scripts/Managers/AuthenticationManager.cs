using TMPro;
using Unity.Services.Authentication;
using Unity.Services.Core;
using UnityEngine;

namespace Managers
{
    public class AuthenticationManager : MonoBehaviour
    {
        public TextMeshProUGUI debug;

        private async void Start()
        {
            await UnityServices.InitializeAsync();
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
            AuthenticationService.Instance.SignedIn += OnPlayerSignIn;
        }

        private void OnPlayerSignIn()
        {
            debug.text += "\nSigned in as " + AuthenticationService.Instance.PlayerId;
        }
    }
}