using System.Collections;
using NetcodeTest.Player;
using Unity.Netcode;
using UnityEngine;

namespace NetcodeTest.Combat
{
    public class RespawnHandler : NetworkBehaviour
    {
        [SerializeField] private TankPlayer playerPrefab;
        [SerializeField] private float keptCoinPercentage;
        
        public override void OnNetworkSpawn()
        {
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
            if (!IsServer) return;
            
            TankPlayer.OnPlayerSpawned -= HandlePlayerSpawned;
            TankPlayer.OnPlayerDespawned -= HandlePlayerDespawned;
        }
        
        private void HandlePlayerSpawned(TankPlayer player)
        {
            player.Health.OnDeath += (health) => HandlePlayerDeath(player);
        }
        
        private void HandlePlayerDespawned(TankPlayer player)
        {
            player.Health.OnDeath -= (health) => HandlePlayerDeath(player);
        }

        private void HandlePlayerDeath(TankPlayer player)
        {
            int keptCoins = (int)(player.Wallet.TotalCoins.Value * (keptCoinPercentage / 100));
            Destroy(player.gameObject);

            StartCoroutine(RespawnPlayerRoutine(player.OwnerClientId, keptCoins));
        }

        private IEnumerator RespawnPlayerRoutine(ulong ownerClientId, int keptCoins)
        {
            yield return null;

            TankPlayer playerInstance = Instantiate(playerPrefab, SpawnPoint.GetRandomSpawnPosition(), Quaternion.identity);
            
            playerInstance.NetworkObject.SpawnAsPlayerObject(ownerClientId);
            
            playerInstance.Wallet.TotalCoins.Value = keptCoins; 
        }
    }
}