using System.Collections.Generic;
using System.Linq;
using NetcodeTest.Networking.Client;
using NetcodeTest.Networking.Shared;
using NetcodeTest.Player;
using Unity.Netcode;
using UnityEngine;

namespace NetcodeTest.UI.Leaderboard
{
    public class Leaderboard : NetworkBehaviour
    {
        [SerializeField] private Transform leaderboardEntityHolder;
        [SerializeField] private Transform teamLeaderboardEntityHolder;
        [SerializeField] private GameObject teamLeaderboardBackground;
        [SerializeField] private LeaderboardEntityDisplay leaderboardEntityPrefab;
        [SerializeField] private int entitiesToDisplay = 8;
        [SerializeField] private Color ownerColor;
        [SerializeField] private string[] teamNames;
        [SerializeField] private TeamColorLookup teamColorLookup;
        
        private NetworkList<LeaderboardEntityState> _leaderboardEntities;
        private List<LeaderboardEntityDisplay> _entityDisplays = new();
        private List<LeaderboardEntityDisplay> _teamEntityDisplays = new();

        private void Awake()
        {
            _leaderboardEntities = new();
        }

        public override void OnNetworkSpawn()
        {
            if (IsClient)
            {
                if (ClientSingleton.Instance.GameManager.UserData.UserGamePreferences.GameQueue == GameQueue.Team)
                {
                    teamLeaderboardBackground.SetActive(true);

                    for (int i = 0; i < teamNames.Length; i++)
                    {
                        LeaderboardEntityDisplay teamLeaderboardEntity = Instantiate(leaderboardEntityPrefab, teamLeaderboardEntityHolder);
                        teamLeaderboardEntity.Initialize(i, teamNames[i], 0);

                        Color teamColor = teamColorLookup.GetTeamColor(i);
                        teamLeaderboardEntity.SetColor(teamColor);
                        
                        _teamEntityDisplays.Add(teamLeaderboardEntity);
                    }
                }
                
                _leaderboardEntities.OnListChanged += HandleLeaderboardEntitiesChanged;

                foreach (LeaderboardEntityState entity in _leaderboardEntities)
                {
                    HandleLeaderboardEntitiesChanged(new NetworkListEvent<LeaderboardEntityState>()
                    {
                        Type = NetworkListEvent<LeaderboardEntityState>.EventType.Add,
                        Value = entity
                    });
                }
            }
            
            if (!IsServer) return;
            
            TankPlayer[] players = FindObjectsByType<TankPlayer>(FindObjectsSortMode.None);

            foreach (TankPlayer player in players)
            {
                HandlePlayerSpawned(player);
            }
            
            TankPlayer.OnPlayerSpawned += HandlePlayerSpawned;
            TankPlayer.OnPlayerDespawned += HandlePlayerDespawned;
        }

        public override void OnNetworkDespawn()
        {
            if (IsClient) _leaderboardEntities.OnListChanged += HandleLeaderboardEntitiesChanged;
            
            if (!IsServer) return;
            
            TankPlayer.OnPlayerSpawned -= HandlePlayerSpawned;
            TankPlayer.OnPlayerDespawned -= HandlePlayerDespawned;
        }
        
        private void HandleLeaderboardEntitiesChanged(NetworkListEvent<LeaderboardEntityState> changeEvent)
        {
            if (!gameObject.scene.isLoaded) return;
            
            switch (changeEvent.Type)
            {
                case NetworkListEvent<LeaderboardEntityState>.EventType.Add:
                    if (_entityDisplays.All(x => x.ClientId != changeEvent.Value.ClientId))
                    {
                        LeaderboardEntityDisplay leaderboardEntity = Instantiate(leaderboardEntityPrefab, leaderboardEntityHolder);
                        leaderboardEntity.Initialize(changeEvent.Value.ClientId, 
                            changeEvent.Value.PlayerName, 
                            changeEvent.Value.Coins);

                        if (NetworkManager.Singleton.LocalClientId == changeEvent.Value.ClientId) leaderboardEntity.SetColor(ownerColor);
                            
                        _entityDisplays.Add(leaderboardEntity);
                    }
                    break;
                
                case NetworkListEvent<LeaderboardEntityState>.EventType.Remove:
                    LeaderboardEntityDisplay displayToRemove = _entityDisplays.FirstOrDefault(x => x.ClientId == changeEvent.Value.ClientId);
                    if (displayToRemove is not null)
                    {
                        displayToRemove.transform.SetParent(null);
                        Destroy(displayToRemove.gameObject);
                        _entityDisplays.Remove(displayToRemove);
                    }
                    break;
                
                case NetworkListEvent<LeaderboardEntityState>.EventType.Value:
                    LeaderboardEntityDisplay displayToUpdate = _entityDisplays.FirstOrDefault(x => x.ClientId == changeEvent.Value.ClientId);
                    if (displayToUpdate is not null)
                    {
                        displayToUpdate.UpdateCoins(changeEvent.Value.Coins);
                    }
                    break;
            }
                
            _entityDisplays.Sort((x, y) => y.Coins.CompareTo(x.Coins));
            
            for (int i = 0; i < _entityDisplays.Count; i++)
            {
                _entityDisplays[i].transform.SetSiblingIndex(i);
                _entityDisplays[i].UpdateText();

                bool shouldShow = i <= entitiesToDisplay - 1;
                _entityDisplays[i].gameObject.SetActive(shouldShow);
            }

            LeaderboardEntityDisplay myDisplay = _entityDisplays.FirstOrDefault(x => x.ClientId == NetworkManager.Singleton.LocalClientId);

            if (myDisplay is not null)
            {
                if (myDisplay.transform.GetSiblingIndex() >= entitiesToDisplay)
                {
                    leaderboardEntityHolder.GetChild(entitiesToDisplay - 1).gameObject.SetActive(false);
                    myDisplay.gameObject.SetActive(true);
                }
            }
            
            if (!teamLeaderboardBackground.activeSelf) return;
            
            LeaderboardEntityDisplay teamDisplay = _teamEntityDisplays.FirstOrDefault(x => x.TeamIndex == changeEvent.Value.TeamIndex);

            if (teamDisplay is not null)
            {
                if (changeEvent.Type == NetworkListEvent<LeaderboardEntityState>.EventType.Remove)
                {
                    teamDisplay.UpdateCoins(teamDisplay.Coins - changeEvent.Value.Coins);
                }
                else
                {
                    teamDisplay.UpdateCoins(teamDisplay.Coins + (changeEvent.Value.Coins - changeEvent.PreviousValue.Coins));
                }
                
                _teamEntityDisplays.Sort((x, y) => y.Coins.CompareTo(x.Coins));

                for (int i = 0; i < _teamEntityDisplays.Count; i++)
                {
                    _teamEntityDisplays[i].transform.SetSiblingIndex(i);
                    _teamEntityDisplays[i].UpdateText();
                }
            }
        }

        private void HandlePlayerSpawned(TankPlayer player)
        {
            _leaderboardEntities.Add(new LeaderboardEntityState()
            {
                ClientId = player.OwnerClientId,
                PlayerName =  player.PlayerName.Value,
                TeamIndex = player.TeamIndex.Value,
                Coins = 0
            });

            player.Wallet.TotalCoins.OnValueChanged +=
                (oldCoins, newCoins) => HandleCoinsChanged(player.OwnerClientId, newCoins);
        }
        
        private void HandlePlayerDespawned(TankPlayer player)
        {
            if(IsServer && player.OwnerClientId == OwnerClientId) { return; }
            
            foreach (LeaderboardEntityState entity in _leaderboardEntities)
            {
                if (entity.ClientId != player.OwnerClientId) continue;

                _leaderboardEntities.Remove(entity);
                break;
            }

            player.Wallet.TotalCoins.OnValueChanged -=
                (oldCoins, newCoins) => HandleCoinsChanged(player.OwnerClientId, newCoins);
        }

        private void HandleCoinsChanged(ulong clientId, int newCoins)
        {
            for (int i = 0; i < _leaderboardEntities.Count; i++)
            {
                if (_leaderboardEntities[i].ClientId != clientId) continue;

                _leaderboardEntities[i] = new LeaderboardEntityState()
                {
                    ClientId = _leaderboardEntities[i].ClientId,
                    PlayerName = _leaderboardEntities[i].PlayerName,
                    TeamIndex = _leaderboardEntities[i].TeamIndex,
                    Coins = newCoins
                };
                
                return;
            }
        }
    }
}