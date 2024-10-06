using Unity.Services.Authentication;
using Unity.Services.Core;
using UnityEngine;

namespace Backend.Hub.Controllers
{
    public class AuthenticationController : MonoBehaviour
    {
        public static async void Initialize()
        {
            // TODO: correctly await for signing in
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