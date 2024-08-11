using System;
using Unity.Netcode;
using UnityEngine;

namespace NetcodeTest.Coins
{
    public class CoinCollector : NetworkBehaviour
    {
        public NetworkVariable<int> TotalCoins = new();

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
    }
}