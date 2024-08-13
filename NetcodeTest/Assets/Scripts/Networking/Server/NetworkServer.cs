using System;
using System.Collections.Generic;
using NetcodeTest.Networking.Shared;
using Unity.Netcode;
using UnityEngine;

namespace NetcodeTest.Networking.Server
{
    public class NetworkServer : IDisposable
    {
        public Action<string> OnClientLeft;
        
        private NetworkManager _networkManager;
        
        private Dictionary<ulong, string> _clientIdToAuth = new();
        private Dictionary<string, UserData> _authIdToUserData = new();
        
        public NetworkServer(NetworkManager networkManager)
        {
            _networkManager = networkManager;

            _networkManager.ConnectionApprovalCallback += ApprovalCheck;
            _networkManager.OnServerStarted += OnNetworkReady;
        }

        private void ApprovalCheck(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response)
        {
            string payload = System.Text.Encoding.UTF8.GetString(request.Payload);
            UserData userData = JsonUtility.FromJson<UserData>(payload);

            _clientIdToAuth[request.ClientNetworkId] = userData.UserAuthId;
            _authIdToUserData[userData.UserAuthId] = userData;

            response.Approved = true;
            response.Position = SpawnPoint.GetRandomSpawnPosition();
            response.Rotation = Quaternion.identity;
            response.CreatePlayerObject = true;
        }
        
        private void OnNetworkReady()
        {
            _networkManager.OnClientDisconnectCallback += OnClientDisconnect;
        }

        private void OnClientDisconnect(ulong clientId)
        {
            if (_clientIdToAuth.TryGetValue(clientId, out string authId))
            {
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