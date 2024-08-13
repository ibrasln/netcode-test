using System;
using System.Threading.Tasks;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace NetcodeTest.Networking.Server
{
    public class ServerGameManager : IDisposable
    {
        private NetworkServer _networkServer;
        
        private string _serverIp;
        private int _serverPort;
        private int _queryPort;

        private MultiplayAllocationService _multiplayAllocationService;
        
        private const string GAME_SCENE_NAME = "Game";
        
        public ServerGameManager(string serverIp, int serverPort, int queryPort, NetworkManager manager)
        {
            _serverIp = serverIp;
            _serverPort = serverPort;
            _queryPort = queryPort;
            _networkServer = new(manager);
            _multiplayAllocationService = new();
        }
        
        public async Task StartGameServerAsync()
        {
            await _multiplayAllocationService.BeginServerCheck();

            if(!_networkServer.OpenConnection(_serverIp, _serverPort))
            {
                Debug.LogError("Network server didn't start as expected!");
                return;
            }
            
            NetworkManager.Singleton.SceneManager.LoadScene(GAME_SCENE_NAME, LoadSceneMode.Single);
        }
        
        public void Dispose()
        {
            _multiplayAllocationService?.Dispose();
            _networkServer?.Dispose();
        }
    }
}