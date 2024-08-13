using System;
using System.Collections.Generic;
using NetcodeTest.Player;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

namespace NetcodeTest.Combat
{
    public class HealingZone : NetworkBehaviour
    {
        [Header("References")]
        [SerializeField] private Image healPowerBar;

        [Header("Settings")] 
        [SerializeField] private int maxHealPower = 30;
        [SerializeField] private float healCooldown = 60f;
        [SerializeField] private float healTickRate = 1f;
        [SerializeField] private int coinsPerTick = 10;
        [SerializeField] private int healthPerTick = 10;

        private float _remainingCooldown;
        private float _tickTimer;
        private List<TankPlayer> _playersInZone = new();
        private NetworkVariable<int> _healPower = new();

        public override void OnNetworkSpawn()
        {
            if (IsClient)
            {
                _healPower.OnValueChanged += HandleHealPowerChanged;
                HandleHealPowerChanged(0, _healPower.Value);
            }

            if (IsServer)
            {
                _healPower.Value = maxHealPower;
            }
        }

        public override void OnNetworkDespawn()
        {
            if (IsClient)
            {
                _healPower.OnValueChanged -= HandleHealPowerChanged;
            }
        }

        private void Update()
        {
            if (!IsServer) return;

            if (_remainingCooldown > 0)
            {
                _remainingCooldown -= Time.deltaTime;

                if (_remainingCooldown <= 0)
                {
                    _healPower.Value = maxHealPower;
                }
                else return;
            }

            _tickTimer += Time.deltaTime;

            if (_tickTimer >= 1 / healTickRate)
            {
                foreach (TankPlayer player in _playersInZone)
                {
                    if (_healPower.Value == 0) break;
                    
                    if (player.Health.CurrentHealth.Value == player.Health.MaxHealth) continue;
                    
                    if (player.Wallet.TotalCoins.Value < coinsPerTick) continue;

                    player.Wallet.SpendCoins(coinsPerTick);
                    player.Health.RestoreHealth(healthPerTick);

                    _healPower.Value -= 1;

                    if (_healPower.Value == 0) _remainingCooldown = healCooldown;
                }

                _tickTimer = _tickTimer % (1 / healTickRate);
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!IsServer) return;

            if (!other.attachedRigidbody.TryGetComponent(out TankPlayer player)) return;
            
            _playersInZone.Add(player);
            
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (!IsServer) return;
            
            if (!other.attachedRigidbody.TryGetComponent(out TankPlayer player)) return;
            
            _playersInZone.Remove(player);
        }

        private void HandleHealPowerChanged(int oldHealPower, int newHealPower)
        {
            healPowerBar.fillAmount = (float)newHealPower / maxHealPower;
        }
    }
}