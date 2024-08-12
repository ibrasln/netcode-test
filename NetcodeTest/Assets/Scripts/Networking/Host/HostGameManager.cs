using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace NetcodeTest.Networking.Host
{
    public class HostGameManager
    {
        private Allocation _allocation;
        private string _joinCode;
        private string _lobbyId;
        
        private const int MAX_CONNECTIONS = 20;
        private const string GAME_SCENE_NAME = "Game";
        
        public async Task StartHostAsync()
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
                _joinCode = await Relay.Instance.GetJoinCodeAsync(_allocation.AllocationId);
                Debug.Log(_joinCode);
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
                    IsPrivate = false,
                    Data = new Dictionary<string, DataObject>()
                    {
                        {
                            "JoinCode", new DataObject( visibility: DataObject.VisibilityOptions.Member,
                                value: _joinCode)
                        }
                    }
                };

                Lobby lobby = await Lobbies.Instance.CreateLobbyAsync("My Lobby", MAX_CONNECTIONS, lobbyOptions);

                _lobbyId = lobby.Id;

                HostSingleton.Instance.StartCoroutine(HeartbeatLobby(15));
            }
            catch (LobbyServiceException ex)
            {
                Debug.LogError(ex);
                throw;
            }
            
            NetworkManager.Singleton.StartHost();

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
    }
}