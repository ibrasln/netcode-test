using System.Collections;
using NetcodeTest.Player;
using Unity.Netcode;
using UnityEngine;

namespace NetcodeTest.Combat
{
    public class RespawnHandler : NetworkBehaviour
    {
        [SerializeField] private NetworkObject playerPrefab;

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
            Destroy(player.gameObject);

            StartCoroutine(RespawnPlayerRoutine(player.OwnerClientId));
        }

        private IEnumerator RespawnPlayerRoutine(ulong ownerClientId)
        {
            yield return null;

            NetworkObject playerInstance = Instantiate(playerPrefab, SpawnPoint.GetRandomSpawnPosition(), Quaternion.identity);

            playerInstance.SpawnAsPlayerObject(ownerClientId);
        }
    }
}