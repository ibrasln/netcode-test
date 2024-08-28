using System;
using UnityEngine;

namespace NetcodeTest.Utils
{
    [RequireComponent(typeof(ParticleSystem))]
    public class ParticleAligner : MonoBehaviour
    {
        private ParticleSystem.MainModule _mainModule;
        
        private void Start()
        {
            _mainModule = GetComponent<ParticleSystem>().main;
        }

        private void Update()
        {
            _mainModule.startRotation = -transform.rotation.eulerAngles.z * Mathf.Deg2Rad;
        }
    }
}