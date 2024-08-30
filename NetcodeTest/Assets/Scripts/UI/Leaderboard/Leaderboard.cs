using System.Collections.Generic;
using System.Linq;
using NetcodeTest.Player;
using Unity.Netcode;
using UnityEngine;

namespace NetcodeTest.UI.Leaderboard
{
    public class Leaderboard : NetworkBehaviour
    {
        [SerializeField] private Transform leaderboardEntityHolder;
        [SerializeField] private LeaderboardEntityDisplay leaderboardEntityPrefab;
        [SerializeField] private int entitiesToDisplay = 8;
        
        private NetworkList<LeaderboardEntityState> _leaderboardEntities;
        private List<LeaderboardEntityDisplay> _entityDisplays = new();

        private void Awake()
        {
            _leaderboardEntities = new();
        }

        public override void OnNetworkSpawn()
        {
            if (IsClient)
            {
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
                        LeaderboardEntityDisplay entityDisplay = Instantiate(leaderboardEntityPrefab, leaderboardEntityHolder);
                        entityDisplay.Initialize(changeEvent.Value.ClientId, 
                            changeEvent.Value.PlayerName, 
                            changeEvent.Value.Coins);
                        
                        _entityDisplays.Add(entityDisplay);
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
        }

        private void HandlePlayerSpawned(TankPlayer player)
        {
            _leaderboardEntities.Add(new LeaderboardEntityState()
            {
                ClientId = player.OwnerClientId,
                PlayerName =  player.PlayerName.Value,
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
                    Coins = newCoins
                };
                
                return;
            }
        }
    }
}