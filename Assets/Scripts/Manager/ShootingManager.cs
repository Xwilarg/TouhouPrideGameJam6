using System.Collections;
using Projectiles;
using UnityEngine;

namespace TouhouPride.Manager
{
	public class ShootingManager : MonoBehaviour
	{

		private GameObject[] _enemiesInScene;
		
		public static ShootingManager Instance { private set; get; }

        private void Awake()
        {
            Instance = this;
		}

        public IEnumerator homeIn(GameObject bullet)
        {
	        // wait
	        yield return new WaitForSeconds(0.5f);
	        
	        // home in
	        _enemiesInScene = GameObject.FindGameObjectsWithTag("Enemy");

	        
	        if (bullet && _enemiesInScene.Length > 0)
	        {
		        // lets just target first in array for now.
		        bullet.GetComponent<HomingBullet>().StartTargeting(_enemiesInScene[0]);
	        }
        }

		public void Shoot(Vector2 direction, bool targetEnemy, AttackType attack, Vector2 pos)
		{
			direction = direction.normalized;

			switch (attack)
			{
				case AttackType.Straight:
					var prefab = ResourcesManager.Instance.Bullet;

					var go = Instantiate(prefab, pos, Quaternion.identity);

					go.layer = targetEnemy ? LayerMask.NameToLayer("PlayerProjectile") : LayerMask.NameToLayer("EnemyProjectile");

					// TODO: i feel like this should possibly be done by the bullet itself? (feels like coupling as is)
					// Throw the projectile in direction
					go.GetComponent<StandardBullet>().Movement(direction);

					break;
				case AttackType.Wave:
					/*
                    // TODO; actually make it wavey
                    var prefab = ResourcesManager.Instance.Bullet;

                    var go = Instantiate(prefab, pos, Quaternion.identity);
                    go.layer = targetEnemy ? LayerMask.NameToLayer("PlayerProjectile") : LayerMask.NameToLayer("EnemyProjectile");

                    // TODO:
                    // Throw the projectile in direction
                    
                    
                    // Make a projectile script that GetComponent<ACharacter> and call TakeDamage()
                    */
					break;
				case AttackType.Homing:
					// shoot bullet in direction aimed
					
					// TODO: get enemies in immediate vicinity, and then aim the bullet there. 
					
					var homingPrefab = ResourcesManager.Instance.Bullet;
					
					var goHoming = Instantiate(homingPrefab, pos, Quaternion.identity);

					goHoming.layer = targetEnemy ? LayerMask.NameToLayer("PlayerProjectile") : LayerMask.NameToLayer("EnemyProjectile");

					// TODO: i feel like this should possibly be done by the bullet itself? (feels like coupling as is)
					// Throw the projectile in direction
					goHoming.GetComponent<StandardBullet>().Movement(direction);
					
					StartCoroutine(homeIn(goHoming));
					break;
				case AttackType.Laser:
					//TODO; instantiate laser
					
					var laserPrefab = ResourcesManager.Instance.Laser;
					var goLaser = Instantiate(laserPrefab, pos, Quaternion.identity);
					goLaser.layer = targetEnemy ? LayerMask.NameToLayer("PlayerProjectile") : LayerMask.NameToLayer("EnemyProjectile");
					
					goLaser.GetComponent<LaserBullet>().Movement(direction);
					
					goLaser.transform.rotation = Quaternion.Euler(0, 0, (Mathf.Rad2Deg * Mathf.Atan2(direction.y, direction.x)));
					
					//TODO; keep it attached to player object (following position and rotation around player position)
					//goLaser.GetComponent<LaserBullet>().SetAim(direction);
					
					break;
			}
		}
	}
}