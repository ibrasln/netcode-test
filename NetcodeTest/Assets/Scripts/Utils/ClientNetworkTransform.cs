using System;
using Unity.Netcode.Components;

namespace NetcodeTest.Utils
{
    public class ClientNetworkTransform : NetworkTransform
    {
        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            CanCommitToTransform = IsOwner;
        }

        protected override void Update()
        {
            CanCommitToTransform = IsOwner;
            
            base.Update();
            
            if (!IsHost && NetworkManager != null && NetworkManager.IsConnectedClient && CanCommitToTransform)
            {
                TryCommitTransformToServer(transform, NetworkManager.LocalTime.Time);
            }
        }

        protected override bool OnIsServerAuthoritative()
        {
            return false;
        }
    }
}