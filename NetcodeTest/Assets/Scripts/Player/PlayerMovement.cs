using System;
using Input;
using Unity.Netcode;
using UnityEngine;

namespace NetcodeTest.Player
{
    public class PlayerMovement : NetworkBehaviour
    {
        [Header("References")]
        [SerializeField] private InputReader inputReader;
        [SerializeField] private Transform bodyTransform;
        [SerializeField] private Rigidbody2D rb;

        [Header("Settings")]
        [SerializeField] private float movementSpeed = 4;
        [SerializeField] private float turningRate;

        private Vector2 _previousMovementInput;
        
        public override void OnNetworkSpawn()
        {
            if (!IsOwner) return;

            inputReader.MoveEvent += HandleMove;
        }

        public override void OnNetworkDespawn()
        {
            if (!IsOwner) return;
            
            inputReader.MoveEvent -= HandleMove;
        }

        private void Update()
        {
            if (!IsOwner) return;

            float zRotation = _previousMovementInput.x * -turningRate * Time.deltaTime;
            bodyTransform.Rotate(0f, 0f, zRotation);
        }

        private void FixedUpdate()
        {
            if (!IsOwner) return;

            rb.velocity = _previousMovementInput.y * movementSpeed * (Vector2)bodyTransform.up ;
        }

        private void HandleMove(Vector2 movementInput)
        {
            _previousMovementInput = movementInput;
        }
    }
}