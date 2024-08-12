using System.Collections.Generic;
using NetcodeTest.Networking.Shared;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace NetcodeTest.Networking.Client
{
    public class NetworkClient
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

            if (SceneManager.GetActiveScene().name != MENU_SCENE_NAME) SceneManager.LoadScene(MENU_SCENE_NAME);
            
            if (_networkManager.IsConnectedClient) _networkManager.Shutdown();
        }
    }
}