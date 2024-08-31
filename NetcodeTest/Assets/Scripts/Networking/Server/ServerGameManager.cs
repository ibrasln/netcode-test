using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NetcodeTest.Networking.Shared;
using Unity.Netcode;
using Unity.Services.Matchmaker.Models;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace NetcodeTest.Networking.Server
{
    public class ServerGameManager : IDisposable
    {
        public NetworkServer NetworkServer { get; private set; }
        
        private string _serverIp;
        private int _serverPort;
        private int _queryPort;
        private MatchplayBackfiller _backfiller;
        private MultiplayAllocationService _multiplayAllocationService;
        private Dictionary<string, int> _teamIdToTeamIndex = new();
        
        public ServerGameManager(string serverIp, int serverPort, int queryPort, NetworkManager manager, NetworkObject playerPrefab)
        {
            _serverIp = serverIp;
            _serverPort = serverPort;
            _queryPort = queryPort;
            NetworkServer = new(manager, playerPrefab);
            _multiplayAllocationService = new();
        }
        
        public async Task StartGameServerAsync()
        {
            await _multiplayAllocationService.BeginServerCheck();

            try
            {
                MatchmakingResults matchmakerPayload = await GetMatchmakerPayload();

                if (matchmakerPayload is not null)
                {
                    await StartBackfill(matchmakerPayload);

                    NetworkServer.OnUserJoined += UserJoined;
                    NetworkServer.OnUserLeft += UserLeft;
                    
                }
                else
                {
                    Debug.LogWarning("Matchmaker payload timed out!");
                }
            }
            catch (Exception e)
            {
                Debug.LogError(e);
                throw;
            }
            
            if(!NetworkServer.OpenConnection(_serverIp, _serverPort))
            {
                Debug.LogError("Network server didn't start as expected!");
                return;
            }
        }

        private async Task StartBackfill(MatchmakingResults payload)
        {
            _backfiller = new MatchplayBackfiller($"{_serverIp}:{_serverPort}", payload.QueueName, payload.MatchProperties, 20);

            if (_backfiller.NeedsPlayers()) await _backfiller.BeginBackfilling();
        }

        private async Task<MatchmakingResults> GetMatchmakerPayload()
        {
            Task<MatchmakingResults> matchmakerPayloadTask = _multiplayAllocationService.SubscribeAndAwaitMatchmakerAllocation();

            if (await Task.WhenAny(matchmakerPayloadTask, Task.Delay(20000)) == matchmakerPayloadTask)
            {
                return matchmakerPayloadTask.Result;
            }

            return null;
        }

        private void UserJoined(UserData user)
        {
            Team team = _backfiller.GetTeamByUserId(user.UserAuthId);

            if (!_teamIdToTeamIndex.TryGetValue(team.TeamId, out int teamIndex))
            {
                teamIndex = _teamIdToTeamIndex.Count;
                _teamIdToTeamIndex.Add(team.TeamId, teamIndex);   
            }

            user.TeamIndex = teamIndex;
            
            _multiplayAllocationService.AddPlayer();

            if (!_backfiller.NeedsPlayers() && _backfiller.IsBackfilling)
            { 
                _ = _backfiller.StopBackfill();
            }
        }
        
        private void UserLeft(UserData user)
        {
            int playerCount = _backfiller.RemovePlayerFromMatch(user.UserAuthId);
            
            _multiplayAllocationService.RemovePlayer();

            if (playerCount <= 0)
            {
                CloseServer();
                return;
            }

            if (_backfiller.NeedsPlayers() && !_backfiller.IsBackfilling)
            {
                _ = _backfiller.BeginBackfilling();
            }
        }

        private async void CloseServer()
        {
            await _backfiller.StopBackfill();
            Dispose();
            Application.Quit();
        }
        
        public void Dispose()
        {
            NetworkServer.OnUserJoined -= UserJoined;
            NetworkServer.OnUserLeft -= UserLeft;
            
            _backfiller?.Dispose();
            _multiplayAllocationService?.Dispose();
            NetworkServer?.Dispose();
        }
    }
}