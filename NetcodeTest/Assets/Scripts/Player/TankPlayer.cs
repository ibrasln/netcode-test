using Cinemachine;
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
        
        [Header("Settings")]
        [SerializeField] private int ownerPriority = 15;

        public NetworkVariable<FixedString32Bytes> PlayerName = new();
        
        public override void OnNetworkSpawn()
        {
            if (IsServer)
            {
                UserData userData = HostSingleton.Instance.GameManager.NetworkServer.GetUserDataByClientId(OwnerClientId);

                PlayerName.Value = userData.Username;
            }
            
            if (IsOwner)
            {
                virtualCamera.Priority = ownerPriority;
            }
        }
    }
}