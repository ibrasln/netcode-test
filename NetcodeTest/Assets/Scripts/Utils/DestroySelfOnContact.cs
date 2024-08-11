using UnityEngine;

namespace NetcodeTest.Utils
{
    public class DestroySelfOnContact : MonoBehaviour
    {
        private void OnTriggerEnter2D(Collider2D other)
        {
            Destroy(gameObject);
        }
    }
}