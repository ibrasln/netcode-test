using System;
using Input;
using Unity.Netcode;
using UnityEngine;

namespace NetcodeTest.Player
{
    public class ProjectileLauncher : NetworkBehaviour
    {
        [Header("References")]
        [SerializeField] private InputReader inputReader;
        [SerializeField] private Transform projectileSpawnPoint;
        [SerializeField] private GameObject serverProjectilePrefab;
        [SerializeField] private GameObject clientProjectilePrefab;

        [Header("Settings")] 
        [SerializeField] private float projectileSpeed;

        private bool _shouldFire;
        
        public override void OnNetworkSpawn()
        {
            if (!IsOwner) return;

            inputReader.PrimaryFireEvent += HandlePrimaryFire;
        }

        public override void OnNetworkDespawn()
        {
            if (!IsOwner) return;
            
            inputReader.PrimaryFireEvent -= HandlePrimaryFire;
        }

        private void Update()
        {
            if (!IsOwner) return;

            if (!_shouldFire) return;
            
            PrimaryFireServerRpc(projectileSpawnPoint.position, projectileSpawnPoint.up);
            SpawnDummyProjectile(projectileSpawnPoint.position, projectileSpawnPoint.up);
        }

        [ServerRpc]
        private void PrimaryFireServerRpc(Vector3 spawnPosition, Vector3 direction)
        {
            GameObject projectile = Instantiate(serverProjectilePrefab, spawnPosition, Quaternion.identity);
            projectile.transform.up = direction;
            
            SpawnDummyProjectileClientRpc(spawnPosition, direction);
        }

        [ClientRpc]
        private void SpawnDummyProjectileClientRpc(Vector3 spawnPosition, Vector3 direction)
        {
            if (IsOwner) return;
            
            SpawnDummyProjectile(spawnPosition, direction);
        }
        
        private void SpawnDummyProjectile(Vector3 spawnPosition, Vector3 direction)
        {
            GameObject projectile = Instantiate(clientProjectilePrefab, spawnPosition, Quaternion.identity);
            projectile.transform.up = direction;
        }

        
        private void HandlePrimaryFire(bool shouldFire)
        {
            _shouldFire = shouldFire;
        }
    }
}