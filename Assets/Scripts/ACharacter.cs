using TouhouPride.Manager;
using TouhouPride.SO;
using UnityEngine;

namespace TouhouPride
{
    public abstract class ACharacter : MonoBehaviour
    {
        [SerializeField]
        private PlayerInfo _info;
        public PlayerInfo Info => _info;

        protected void Shoot(Vector2 direction, bool targetEnemy)
        {
            switch (_info.AttackType)
            {
                case AttackType.Straight:
                case AttackType.Wave:
                    var prefab = ResourcesManager.Instance.Bullet;

                    var go = Instantiate(prefab, transform.position, Quaternion.identity);
                    go.layer = targetEnemy ? LayerMask.NameToLayer("PlayerProjectile") : LayerMask.NameToLayer("EnemyProjectile");

                    // TODO:
                    // Throw the projectile in direction
                    // Make a projectile script that GetComponent<ACharacter> and call TakeDamage()
                    break;
            }
        }

        public void TakeDamage(int amount)
        {
            Destroy(gameObject); // TODO
        }
    }
}