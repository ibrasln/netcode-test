using System;
using System.Text;
using System.Threading.Tasks;
using NetcodeTest.Networking.Shared;
using NetcodeTest.UI;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace NetcodeTest.Networking.Client
{
    public class ClientGameManager : IDisposable
    {
        private JoinAllocation _allocation;
        private NetworkClient _networkClient;
        private MatchplayMatchmaker _matchmaker;

        private UserData _userData;
        
        private const string MENU_SCENE_NAME = "Menu";
        private const string GAME_SCENE_NAME = "Game";
        
        public async Task<bool> InitAsync()
        {
            await UnityServices.InitializeAsync();

            _networkClient = new(NetworkManager.Singleton);
            _matchmaker = new();
            
            AuthState authState = await AuthenticationWrapper.Authenticate();

            if (authState == AuthState.Authenticated)
            {
                _userData = new UserData()
                {
                    Username = PlayerPrefs.GetString(NameSelector.PLAYER_NAME_KEY, "Missing Name"),
                    UserAuthId = AuthenticationService.Instance.PlayerId
                };
                
                return true;
            }

            return false;
        }

        public void GoToMenu()
        {
            SceneManager.LoadScene(MENU_SCENE_NAME);
        }

        public async Task StartClientAsync(string joinCode)
        {
            try
            { 
                _allocation = await Relay.Instance.JoinAllocationAsync(joinCode);
            }
            catch (Exception ex)
            {
                Debug.LogError(ex);
                return;
            }

            UnityTransport transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
            RelayServerData relayServerData = new(_allocation, "dtls");
            transport.SetRelayServerData(relayServerData);

            string payload = JsonUtility.ToJson(_userData);
            byte[] payloadBytes = Encoding.UTF8.GetBytes(payload);

            NetworkManager.Singleton.NetworkConfig.ConnectionData = payloadBytes;
            
            NetworkManager.Singleton.StartClient();
        }

        private async Task<MatchmakerPollingResult> GetMatchAsync()
        {
            MatchmakingResult matchmakingResult = await _matchmaker.Matchmake(_userData);

            if (matchmakingResult.result == MatchmakerPollingResult.Success)
            {
                // Connect to server
            }

            return matchmakingResult.result;
        }
        
        public void Disconnect()
        {
            _networkClient.Disconnect();
        }
        
        public void Dispose()
        {
            _networkClient?.Dispose();
        }
    }
}