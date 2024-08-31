using System;
using Cinemachine;
using NetcodeTest.Coins;
using NetcodeTest.Combat;
using NetcodeTest.Networking.Host;
using NetcodeTest.Networking.Server;
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

        [SerializeField] private SpriteRenderer minimapIcon;
        [SerializeField] private Texture2D crosshair;
        [field: SerializeField] public Health Health { get; private set; }
        [field: SerializeField] public CoinCollector Wallet { get; private set; }
        
        [Header("Settings")]
        [SerializeField] private int ownerPriority = 15;
        [SerializeField] private Color minimapIconColor;
        
        public NetworkVariable<FixedString32Bytes> PlayerName = new();
        public NetworkVariable<int> TeamIndex = new();

        public static event Action<TankPlayer> OnPlayerSpawned;
        public static event Action<TankPlayer> OnPlayerDespawned;
        
        public override void OnNetworkSpawn()
        {
            if (IsServer)
            {
                UserData userData = null;
                
                if (IsHost)
                {
                    userData = HostSingleton.Instance.GameManager.NetworkServer.GetUserDataByClientId(OwnerClientId);
                }
                else
                {
                    userData = ServerSingleton.Instance.GameManager.NetworkServer.GetUserDataByClientId(OwnerClientId);
                }

                PlayerName.Value = userData.Username;
                TeamIndex.Value = userData.TeamIndex;
                
                OnPlayerSpawned?.Invoke(this);
            }
            
            if (IsOwner)
            {
                virtualCamera.Priority = ownerPriority;

                minimapIcon.color = minimapIconColor;
                
                Cursor.SetCursor(crosshair, new(crosshair.width / 2, crosshair.height / 2), CursorMode.Auto);
            }
        }

        public override void OnNetworkDespawn()
        {
            if (IsServer) OnPlayerDespawned?.Invoke(this);
        }
    }
}