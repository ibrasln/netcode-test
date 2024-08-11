using System;
using Input;
using Unity.Netcode;
using UnityEngine;

namespace NetcodeTest.Player
{
    public class PlayerAiming : NetworkBehaviour
    {
        [SerializeField] private Transform turretTransform;
        [SerializeField] private InputReader inputReader;

        private Camera _mainCam;
        
        private void Start()
        {
            _mainCam = Camera.main;
        }

        private void LateUpdate()
        {
            if (!IsOwner) return;

            Vector2 aimScreenPosition = inputReader.AimPosition;
            Vector2 aimWorldPosition = _mainCam.ScreenToWorldPoint(aimScreenPosition);

            turretTransform.up = new Vector2(
                aimWorldPosition.x - turretTransform.position.x,
                aimWorldPosition.y - turretTransform.position.y);
        }
    }
}