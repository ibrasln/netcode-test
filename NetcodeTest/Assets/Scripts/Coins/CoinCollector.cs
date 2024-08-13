using System;
using NetcodeTest.Combat;
using Unity.Netcode;
using UnityEngine;
using Random = UnityEngine.Random;

namespace NetcodeTest.Coins
{
    public class CoinCollector : NetworkBehaviour
    {
        public NetworkVariable<int> TotalCoins = new();
        
        [Header("References")]
        [SerializeField] private Health health;
        [SerializeField] private BountyCoin coinPrefab;

        [Header("Settings")] 
        [SerializeField] private float coinSpread = 3f;
        [SerializeField] private float bountyPercentage = 50f;
        [SerializeField] private int bountyCoinCount = 10;
        [SerializeField] private int minBountyCoinValue = 5;
        [SerializeField] private LayerMask layerMask;
        
        private Collider2D[] _coinBuffer = new Collider2D[1];
        private float _coinRadius;

        public override void OnNetworkSpawn()
        {
            if (!IsServer) return;
            
            _coinRadius = coinPrefab.GetComponent<CircleCollider2D>().radius;

            health.OnDeath += HandleDeath;
        }

        public override void OnNetworkDespawn()
        {
            if (!IsServer) return;
            
            health.OnDeath -= HandleDeath;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.TryGetComponent(out Coin coin)) return;
            
            int coinValue = coin.Collect();

            if (!IsServer) return;
            
            TotalCoins.Value += coinValue;
        }
        
        public void SpendCoins(int value)
        {
            TotalCoins.Value -= value;
        }

        private void HandleDeath(Health health)
        {
            int bountyValue = (int)(TotalCoins.Value * (bountyPercentage / 100));
            int bountyCoinValue = bountyValue / bountyCoinCount;

            if (bountyCoinValue < minBountyCoinValue) return;

            for (int i = 0; i < bountyCoinCount; i++)
            {
                BountyCoin coinInstance = Instantiate(coinPrefab, GetSpawnPoint(), Quaternion.identity);
                coinInstance.SetValue(bountyCoinValue);
                coinInstance.NetworkObject.Spawn();
            }
        }
     
        private Vector2 GetSpawnPoint()
        {
            while (true)
            {
                Vector2 spawnPoint = (Vector2)transform.position + Random.insideUnitCircle * coinSpread;

                int numColliders = Physics2D.OverlapCircleNonAlloc(spawnPoint, _coinRadius, _coinBuffer, layerMask);

                if (numColliders == 0) return spawnPoint;
            }
        }
    }
}