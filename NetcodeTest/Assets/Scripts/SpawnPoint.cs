using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace NetcodeTest
{
    public class SpawnPoint : MonoBehaviour
    {
        private static List<SpawnPoint> SpawnPoints = new();

        private void OnEnable()
        {
            SpawnPoints.Add(this);
        }

        private void OnDisable()
        {
            SpawnPoints.Remove(this);
        }

        public static Vector3 GetRandomSpawnPosition()
        {
            if (SpawnPoints.Count == 0) return Vector3.zero;
            
            int index = Random.Range(0, SpawnPoints.Count + 1);
            return SpawnPoints[index].transform.position;
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawSphere(transform.position, 1);
        }
    }
}