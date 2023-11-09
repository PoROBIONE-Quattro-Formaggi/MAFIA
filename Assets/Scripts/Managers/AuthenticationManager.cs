using Unity.Services.Authentication;
using Unity.Services.Core;
using UnityEngine;

namespace Managers
{
    public class AuthenticationManager : MonoBehaviour
    {
        private async void Start()
        {
            if (UnityServices.State == ServicesInitializationState.Uninitialized)
            {
                await UnityServices.InitializeAsync();
            }

            if (!AuthenticationService.Instance.IsSignedIn)
            {
                await AuthenticationService.Instance.SignInAnonymouslyAsync();
            }
        }
    }
}