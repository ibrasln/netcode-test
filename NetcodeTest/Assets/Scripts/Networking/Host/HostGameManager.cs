using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using NetcodeTest.Networking.Server;
using NetcodeTest.Networking.Shared;
using NetcodeTest.UI;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using Unity.Services.Authentication;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace NetcodeTest.Networking.Host
{
    public class HostGameManager : IDisposable
    {
        public NetworkServer NetworkServer { get; private set; }
        public string JoinCode { get; private set; }
        
        private Allocation _allocation;
        private string _lobbyId;
        private NetworkObject _playerPrefab;
        
        private const int MAX_CONNECTIONS = 20;
        private const string GAME_SCENE_NAME = "Game";

        public HostGameManager(NetworkObject playerPrefab)
        {
            _playerPrefab = playerPrefab;
        }
        
        public async Task StartHostAsync(bool isPrivate)
        {
            try
            { 
                _allocation = await Relay.Instance.CreateAllocationAsync(MAX_CONNECTIONS);
            }
            catch (Exception ex)
            {
                Debug.LogError(ex);
                return;
            }
            
            try
            { 
                JoinCode = await Relay.Instance.GetJoinCodeAsync(_allocation.AllocationId);
                Debug.Log(JoinCode);
            }
            catch (Exception ex)
            {
                Debug.LogError(ex);
                return;
            }

            UnityTransport transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
            
            RelayServerData relayServerData = new(_allocation, "dtls");
            transport.SetRelayServerData(relayServerData);

            try
            {
                CreateLobbyOptions lobbyOptions = new()
                {
                    IsPrivate = isPrivate,
                    Data = new Dictionary<string, DataObject>()
                    {
                        {
                            "JoinCode", new DataObject( visibility: DataObject.VisibilityOptions.Member,
                                value: JoinCode)
                        }
                    }
                };
                
                string playerName = PlayerPrefs.GetString(NameSelector.PLAYER_NAME_KEY, "Unknown");
                
                Lobby lobby = await Lobbies.Instance.CreateLobbyAsync($"{playerName}'s Lobby", MAX_CONNECTIONS, lobbyOptions);

                _lobbyId = lobby.Id;

                HostSingleton.Instance.StartCoroutine(HeartbeatLobby(15));
            }
            catch (LobbyServiceException ex)
            {
                Debug.LogError(ex);
                throw;
            }

            NetworkServer = new(NetworkManager.Singleton, _playerPrefab);
            
            UserData userData = new UserData()
            {
                Username = PlayerPrefs.GetString(NameSelector.PLAYER_NAME_KEY, "Missing Name"),
                UserAuthId = AuthenticationService.Instance.PlayerId
            };

            string payload = JsonUtility.ToJson(userData);
            byte[] payloadBytes = Encoding.UTF8.GetBytes(payload);

            NetworkManager.Singleton.NetworkConfig.ConnectionData = payloadBytes;
            
            NetworkManager.Singleton.StartHost();

            NetworkServer.OnClientLeft += HandleClientLeft;
            
            NetworkManager.Singleton.SceneManager.LoadScene(GAME_SCENE_NAME, LoadSceneMode.Single);
        }

        /// <summary>
        /// Ping UGS to say "Our lobby is still active."
        /// </summary>
        /// <param name="waitTimeSeconds"></param>
        /// <returns></returns>
        private IEnumerator HeartbeatLobby(float waitTimeSeconds)
        {
            WaitForSecondsRealtime delay = new(waitTimeSeconds);
            while (true)
            {
                Lobbies.Instance.SendHeartbeatPingAsync(_lobbyId);
                yield return delay;
            }
        }

        public void Dispose()
        {
            Shutdown();
        }

        public async void Shutdown()
        {
            if (string.IsNullOrEmpty(_lobbyId)) return;
            
            HostSingleton.Instance.StopCoroutine(nameof(HeartbeatLobby));
            
            try
            {
                await Lobbies.Instance.DeleteLobbyAsync(_lobbyId);
            }
            catch (LobbyServiceException ex)
            {
                Debug.LogError(ex);
            }

            _lobbyId = string.Empty;
            
            NetworkServer.OnClientLeft -= HandleClientLeft;
            
            NetworkServer?.Dispose();
        }

        private async void HandleClientLeft(string authId)
        {
            try
            {
                await LobbyService.Instance.RemovePlayerAsync(_lobbyId, authId);
            }
            catch (LobbyServiceException ex)
            {
                Debug.LogError(ex);
                throw;
            }
        }
        
    }
}
