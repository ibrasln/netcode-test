using UnityEngine;

namespace NetcodeTest.Utils
{
    public class SpawnOnDestroy : MonoBehaviour
    {
        [SerializeField] private GameObject prefab;

        private void OnDestroy()
        {
            Instantiate(prefab, transform.position, Quaternion.identity);
        }
    }
}