using System;
using Input;
using NetcodeTest.Combat;
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
        [SerializeField] private GameObject muzzleFlash;
        [SerializeField] private Collider2D playerCollider;

        [Header("Settings")] 
        [SerializeField] private float projectileSpeed;
        [SerializeField] private float fireRate;
        [SerializeField] private float muzzleFlashDuration;

        private bool _shouldFire;
        private float _previousFireTime;
        private float _muzzleFlashTimer;
        
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
            if (_muzzleFlashTimer > 0)
            {
                _muzzleFlashTimer -= Time.deltaTime;

                if (_muzzleFlashTimer <= 0f)
                {
                    muzzleFlash.SetActive(false);
                }
            }
            
            if (!IsOwner) return;

            if (!_shouldFire) return;

            if (Time.time < 1 / fireRate + _previousFireTime) return;
            
            PrimaryFireServerRpc(projectileSpawnPoint.position, projectileSpawnPoint.up);
            SpawnDummyProjectile(projectileSpawnPoint.position, projectileSpawnPoint.up);
            
            _previousFireTime = Time.time;
        }

        [ServerRpc]
        private void PrimaryFireServerRpc(Vector3 spawnPosition, Vector3 direction)
        {
            GameObject projectile = Instantiate(serverProjectilePrefab, spawnPosition, Quaternion.identity);
            projectile.transform.up = direction;
            
            Physics2D.IgnoreCollision(playerCollider, projectile.GetComponent<Collider2D>());
            
            if (projectile.TryGetComponent(out DealDamageOnContact dealDamageOnContact)) dealDamageOnContact.SetOwner(OwnerClientId);
            
            if (projectile.TryGetComponent(out Rigidbody2D rb))
            {
                rb.velocity = rb.transform.up * projectileSpeed;
            }
            
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
            muzzleFlash.SetActive(true);
            _muzzleFlashTimer = muzzleFlashDuration;
            
            GameObject projectile = Instantiate(clientProjectilePrefab, spawnPosition, Quaternion.identity);
            projectile.transform.up = direction;
            
            Physics2D.IgnoreCollision(playerCollider, projectile.GetComponent<Collider2D>());

            if (projectile.TryGetComponent(out Rigidbody2D rb))
            {
                rb.velocity = rb.transform.up * projectileSpeed;
            }
        }

        
        private void HandlePrimaryFire(bool shouldFire)
        {
            _shouldFire = shouldFire;
        }
    }
}