using System;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Core;
using UnityEngine;

namespace NetcodeTest.Networking.Client
{
    public static class AuthenticationWrapper
    {
        public static AuthState AuthState { get; private set; } = AuthState.NotAuthenticated;

        public static async Task<AuthState> Authenticate(int maxRetries = 5)
        {
            if (AuthState == AuthState.Authenticated) return AuthState;

            if (AuthState == AuthState.Authenticating)
            {
                Debug.LogWarning("Already authenticating..");
                await Authenticating();
                return AuthState;
            }
            
            await SignInAnonymouslyAsync(maxRetries);

            return AuthState;
        }

        private static async Task<AuthState> Authenticating()
        {
            while (AuthState is AuthState.Authenticating or AuthState.NotAuthenticated)
            {
                await Task.Delay(200);
            }

            return AuthState;
        }
        
        private static async Task SignInAnonymouslyAsync(int maxRetries = 5)
        {
            AuthState = AuthState.Authenticating;
            
            int retries = 0;
            while (AuthState == AuthState.Authenticating && retries < maxRetries)
            {
                try
                {
                    await AuthenticationService.Instance.SignInAnonymouslyAsync();

                    if (AuthenticationService.Instance.IsSignedIn && AuthenticationService.Instance.IsAuthorized)
                    {
                        AuthState = AuthState.Authenticated;
                        Debug.Log($"Authenticated successfully after {retries} retries!");
                        break;
                    }
                }
                catch (AuthenticationException ex)
                {
                    Debug.LogError(ex);
                    AuthState = AuthState.Error;
                }
                catch (RequestFailedException ex)
                {
                    Debug.LogError(ex);
                    AuthState = AuthState.Error;
                }
                
                retries++;
                await Task.Delay(1000); // 1 second
            }

            if (AuthState != AuthState.Authenticated)
            {
                Debug.LogWarning($"Player was not signed in successfully after {retries} retries!");
                AuthState = AuthState.TimeOut;
            }
        }
    }

    public enum AuthState
    {
        NotAuthenticated,
        Authenticating,
        Authenticated,
        Error,
        TimeOut
    }
}