using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NetcodeTest.Networking.Shared;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;

namespace NetcodeTest.Networking.Server
{
    public class NetworkServer : IDisposable
    {
        public Action<UserData> OnUserJoined;
        public Action<UserData> OnUserLeft;
        public Action<string> OnClientLeft;
        
        private NetworkManager _networkManager;
        private NetworkObject _playerPrefab;
        
        private Dictionary<ulong, string> _clientIdToAuth = new();
        private Dictionary<string, UserData> _authIdToUserData = new();
        
        public NetworkServer(NetworkManager networkManager, NetworkObject playerPrefab)
        {
            _networkManager = networkManager;
            _playerPrefab = playerPrefab;
            
            _networkManager.ConnectionApprovalCallback += ApprovalCheck;
            _networkManager.OnServerStarted += OnNetworkReady;
        }

        public bool OpenConnection(string ip, int port)
        {
            UnityTransport transport = _networkManager.gameObject.GetComponent<UnityTransport>();
            transport.SetConnectionData(ip, (ushort)port);

            return _networkManager.StartServer();
        }
        
        private void ApprovalCheck(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response)
        {
            string payload = System.Text.Encoding.UTF8.GetString(request.Payload);
            UserData userData = JsonUtility.FromJson<UserData>(payload);

            _clientIdToAuth[request.ClientNetworkId] = userData.UserAuthId;
            _authIdToUserData[userData.UserAuthId] = userData;

            OnUserJoined?.Invoke(userData);

            _ = SpawnPlayerDelay(request.ClientNetworkId);
            
            response.Approved = true;
            response.CreatePlayerObject = false;
        }

        private async Task SpawnPlayerDelay(ulong clientId)
        {
            await Task.Delay(1000);

            NetworkObject playerInstance = GameObject.Instantiate(_playerPrefab, SpawnPoint.GetRandomSpawnPosition(), Quaternion.identity);
            
            playerInstance.SpawnAsPlayerObject(clientId);
        }
        
        private void OnNetworkReady()
        {
            _networkManager.OnClientDisconnectCallback += OnClientDisconnect;
        }

        private void OnClientDisconnect(ulong clientId)
        {
            if (_clientIdToAuth.TryGetValue(clientId, out string authId))
            {
                OnUserLeft?.Invoke(_authIdToUserData[authId]);
                
                _clientIdToAuth.Remove(clientId);
                _authIdToUserData.Remove(authId);
                
                OnClientLeft?.Invoke(authId);
            }
        }

        public UserData GetUserDataByClientId(ulong clientId)
        {
            if (_clientIdToAuth.TryGetValue(clientId, out string authId))
            {
                if (_authIdToUserData.TryGetValue(authId, out UserData userData)) return userData;
            }

            return null;
        }
        
        public void Dispose()
        {
            if (_networkManager is null) return;
            
            _networkManager.ConnectionApprovalCallback -= ApprovalCheck;
            _networkManager.OnServerStarted -= OnNetworkReady;
            _networkManager.OnClientDisconnectCallback -= OnClientDisconnect;
                
            if (_networkManager.IsListening) _networkManager.Shutdown();
        }

        
    }
}