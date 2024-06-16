using System.Collections.Generic;
using TouhouPride.Manager;
using TouhouPride.Map;
using TouhouPride.SO;
using UnityEngine;

namespace TouhouPride
{
    public abstract class ACharacter : MonoBehaviour
    {
        [SerializeField]
        protected PlayerInfo _info;
        public virtual PlayerInfo Info { get => _info; }

        public RectTransform HealthBar { set; private get; }

        protected Animator _animator;

        private int _health;

        private List<IRequirement<ACharacter>> _requirements = new();
        public void AddRequirement(IRequirement<ACharacter> req)
        {
            _requirements.Add(req);
        }

        protected virtual void TakeDamage()
        { }

        protected virtual void Awake()
        {
            _animator = GetComponent<Animator>();
        }

        protected virtual void Start()
        {
            _health = Info.MaxHealth;
        }

        protected void Shoot(Vector2 direction, bool targetEnemy)
        {
            ShootingManager.Instance.Shoot(direction, targetEnemy, _info.AttackType, transform.position);
        }

        protected bool _canTakeDamage = true;

        public void TakeDamage(int amount)
        {
            if (!_canTakeDamage)
            {
                return;
            }
            _health -= amount;

            if (_health <= 0)
            {
                foreach (var r in _requirements)
                {
                    r.Unlock(this);
                }
                if (HealthBar != null)
                {
                    HealthBar.localScale = Vector3.zero;
                }
                Destroy(gameObject);
            }
            else
            {
                if (HealthBar != null)
                {
                    HealthBar.localScale = new(_health / (float)Info.MaxHealth, 1f, 1f);
                }
                TakeDamage();
            }
        }
    }
}