using System;
using UnityEngine;

namespace NetcodeTest.Utils
{
    public class Lifetime : MonoBehaviour
    {
        [SerializeField] private float lifetime = 1f;
        private void Start()
        {
            Destroy(gameObject, lifetime);
        }
    }
}
