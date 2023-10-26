using Unity.Services.Authentication;
using Unity.Services.Core;
using UnityEngine;

namespace Managers
{
    public class AuthenticationManager : MonoBehaviour
    {
        private async void Start()
        {
            await UnityServices.InitializeAsync();
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
            // AuthenticationService.Instance.SignedIn += OnPlayerSignIn;
        }

        // private static void OnPlayerSignIn()
        // {
        // }
    }
}