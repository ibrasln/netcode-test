using Input;
using NetcodeTest.Coins;
using NetcodeTest.Combat;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.EventSystems;

namespace NetcodeTest.Player
{
    public class ProjectileLauncher : NetworkBehaviour
    {
        [Header("References")] 
        [SerializeField] private TankPlayer player;
        [SerializeField] private InputReader inputReader;
        [SerializeField] private Transform projectileSpawnPoint;
        [SerializeField] private GameObject serverProjectilePrefab;
        [SerializeField] private GameObject clientProjectilePrefab;
        [SerializeField] private GameObject muzzleFlash;
        [SerializeField] private Collider2D playerCollider;
        [SerializeField] private CoinCollector coinCollector;
        [SerializeField] private int costToFire;

        [Header("Settings")] 
        [SerializeField] private float projectileSpeed;
        [SerializeField] private float fireRate;
        [SerializeField] private float muzzleFlashDuration;

        private bool _isPointerOverUI;
        private float _timer;
        private bool _shouldFire;
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

            _isPointerOverUI = EventSystem.current.IsPointerOverGameObject();
            
            if (_timer > 0) _timer -= Time.deltaTime;
            
            if (!_shouldFire) return;

            if (_timer > 0) return;

            if (coinCollector.TotalCoins.Value < costToFire) return;
            
            PrimaryFireServerRpc(projectileSpawnPoint.position, projectileSpawnPoint.up);
            SpawnDummyProjectile(projectileSpawnPoint.position, projectileSpawnPoint.up, player.TeamIndex.Value);
            
            _timer = 1 / fireRate;
        }

        [ServerRpc]
        private void PrimaryFireServerRpc(Vector3 spawnPosition, Vector3 direction)
        {
            if (coinCollector.TotalCoins.Value < costToFire) return;
            
            coinCollector.SpendCoins(costToFire);
            
            GameObject projectileInstance = Instantiate(serverProjectilePrefab, spawnPosition, Quaternion.identity);
            projectileInstance.transform.up = direction;
            
            Physics2D.IgnoreCollision(playerCollider, projectileInstance.GetComponent<Collider2D>());
            
            if (projectileInstance.TryGetComponent(out Projectile projectile)) projectile.Initialize(player.TeamIndex.Value);
            
            if (projectile.TryGetComponent(out Rigidbody2D rb)) rb.velocity = rb.transform.up * projectileSpeed;
            
            SpawnDummyProjectileClientRpc(spawnPosition, direction, player.TeamIndex.Value);
        }

        [ClientRpc]
        private void SpawnDummyProjectileClientRpc(Vector3 spawnPosition, Vector3 direction, int teamIndex)
        {
            if (IsOwner) return;
            
            SpawnDummyProjectile(spawnPosition, direction, teamIndex);
        }
        
        private void SpawnDummyProjectile(Vector3 spawnPosition, Vector3 direction, int teamIndex)
        {
            muzzleFlash.SetActive(true);
            _muzzleFlashTimer = muzzleFlashDuration;
            
            GameObject projectileInstance = Instantiate(clientProjectilePrefab, spawnPosition, Quaternion.identity);
            projectileInstance.transform.up = direction;
            
            Physics2D.IgnoreCollision(playerCollider, projectileInstance.GetComponent<Collider2D>());

            if (projectileInstance.TryGetComponent(out Projectile projectile)) projectile.Initialize(teamIndex);
            
            if (projectile.TryGetComponent(out Rigidbody2D rb)) rb.velocity = rb.transform.up * projectileSpeed;
        }

        
        private void HandlePrimaryFire(bool shouldFire)
        {
            if (shouldFire)
            {
                if (_isPointerOverUI) return;
            }
            _shouldFire = shouldFire;
        }
    }
}