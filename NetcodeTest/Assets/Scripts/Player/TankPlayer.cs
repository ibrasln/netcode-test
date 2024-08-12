using Cinemachine;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Serialization;

namespace NetcodeTest.Player
{
    public class TankPlayer : NetworkBehaviour
    {
        [Header("References")]
        [SerializeField] private CinemachineVirtualCamera virtualCamera;
        
        [Header("Settings")]
        [SerializeField] private int ownerPriority = 15;
        
        public override void OnNetworkSpawn()
        {
            if (IsOwner)
            {
                virtualCamera.Priority = ownerPriority;
            }
        }
    }
}