using System;
using Cinemachine;
using NetcodeTest.Coins;
using NetcodeTest.Combat;
using NetcodeTest.Networking.Host;
using NetcodeTest.Networking.Shared;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

namespace NetcodeTest.Player
{
    public class TankPlayer : NetworkBehaviour
    {
        [Header("References")]
        [SerializeField] private CinemachineVirtualCamera virtualCamera;
        [field: SerializeField] public Health Health { get; private set; }
        [field: SerializeField] public CoinCollector Wallet { get; private set; }
        
        [Header("Settings")]
        [SerializeField] private int ownerPriority = 15;

        public NetworkVariable<FixedString32Bytes> PlayerName = new();

        public static event Action<TankPlayer> OnPlayerSpawned;
        public static event Action<TankPlayer> OnPlayerDespawned;
        
        public override void OnNetworkSpawn()
        {
            if (IsServer)
            {
                UserData userData = HostSingleton.Instance.GameManager.NetworkServer.GetUserDataByClientId(OwnerClientId);

                PlayerName.Value = userData.Username;
                
                OnPlayerSpawned?.Invoke(this);
            }
            
            if (IsOwner)
            {
                virtualCamera.Priority = ownerPriority;
            }
        }

        public override void OnNetworkDespawn()
        {
            if (IsServer) OnPlayerDespawned?.Invoke(this);
        }
    }
}