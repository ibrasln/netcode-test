using System;
using Unity.Netcode;
using UnityEngine.SceneManagement;

namespace NetcodeTest.Networking.Client
{
    public class NetworkClient : IDisposable
    {
        private NetworkManager _networkManager;

        private const string MENU_SCENE_NAME = "Menu";
        
        public NetworkClient(NetworkManager networkManager)
        {
            _networkManager = networkManager;
            
            _networkManager.OnClientDisconnectCallback += OnClientDisconnect;
        }

        private void OnClientDisconnect(ulong clientId)
        {
            if (clientId != 0 && clientId != _networkManager.LocalClientId) { return; }
            
            Disconnect();
        }

        public void Disconnect()
        {
            if (SceneManager.GetActiveScene().name != MENU_SCENE_NAME) SceneManager.LoadScene(MENU_SCENE_NAME);
            
            if (_networkManager.IsConnectedClient) _networkManager.Shutdown();
        }
        
        public void Dispose()
        {
            if (_networkManager is not null) _networkManager.OnClientDisconnectCallback -= OnClientDisconnect;
        }
    }
}