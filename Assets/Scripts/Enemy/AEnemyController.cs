using System.Collections;
using UnityEngine;

namespace TouhouPride.Enemy
{
    public abstract class AEnemyController : ACharacter
    {
        private bool _isActive;
        protected bool IsActive
        {
            set
            {
                _isActive = value;
                if (value)
                {
                    StartCoroutine(AttackTimerCoroutine());
                }
            }
            get => _isActive;
        }

        private Rigidbody2D _rb;

        protected abstract Vector2 Move();
        protected abstract Vector2? DoesAttack();

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
        }

        private void FixedUpdate()
        {
            if (IsActive)
            {
                _rb.velocity = Move();
            }
            else
            {
                _rb.velocity = Vector2.zero;
            }
        }

        private IEnumerator AttackTimerCoroutine()
        {
            while (true)
            {
                yield return new WaitForSeconds(2f);
                var attackDir = DoesAttack();
                if (attackDir.HasValue)
                {
                    Shoot(attackDir.Value, false);
                }
            }
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.CompareTag("Player"))
            {
                IsActive = true;
            }
        }
    }
}