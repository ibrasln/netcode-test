using System;
using System.Text;
using System.Threading.Tasks;
using NetcodeTest.Networking.Shared;
using NetcodeTest.UI;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using Unity.Services.Core;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace NetcodeTest.Networking.Client
{
    public class ClientGameManager
    {
        private JoinAllocation _allocation;
        
        private const string MENU_SCENE_NAME = "Menu";
        private const string GAME_SCENE_NAME = "Game";
        
        public async Task<bool> InitAsync()
        {
            await UnityServices.InitializeAsync();

            AuthState authState = await AuthenticationWrapper.Authenticate();

            return authState == AuthState.Authenticated;
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

            UserData userData = new UserData()
            {
                Username = PlayerPrefs.GetString(NameSelector.PLAYER_NAME_KEY, "Missing Name")
            };

            string payload = JsonUtility.ToJson(userData);
            byte[] payloadBytes = Encoding.UTF8.GetBytes(payload);

            NetworkManager.Singleton.NetworkConfig.ConnectionData = payloadBytes;
            
            NetworkManager.Singleton.StartClient();

            NetworkManager.Singleton.SceneManager.LoadScene(GAME_SCENE_NAME, LoadSceneMode.Single);
        }
    }
}