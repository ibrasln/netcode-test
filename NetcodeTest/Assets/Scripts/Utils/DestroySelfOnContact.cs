using NetcodeTest.Combat;
using NetcodeTest.Player;
using UnityEngine;

namespace NetcodeTest.Utils
{
    public class DestroySelfOnContact : MonoBehaviour
    {
        [SerializeField] private Projectile projectile;
        
        private void OnTriggerEnter2D(Collider2D other)
        {
            if (projectile.TeamIndex != -1)
            {
                if (other.attachedRigidbody != null)
                {
                    if (other.attachedRigidbody.TryGetComponent(out TankPlayer player))
                    {
                        if (player.TeamIndex.Value == projectile.TeamIndex) return;
                    }
                }
            }
            
            Destroy(gameObject);
        }
    }
}