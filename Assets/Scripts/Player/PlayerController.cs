using System.Collections;
using TouhouPride.Love;
using TouhouPride.Manager;
using TouhouPride.Utils;
using TouhouPride.VN;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

namespace TouhouPride.Player
{
    public class PlayerController : ACharacter
    {
        public static PlayerController Instance { private set; get; }

        [SerializeField] CinemachineCamera _cam;
        private Camera _mainCam;

        private Rigidbody2D _rb;
        private Vector2 _mov;
        protected Follower _follower;
        protected Animator _anim;

        private Vector2 _lastDir = Vector2.up;

        private bool _canShoot = true;

        private bool _isDashing;
        private bool _canDash = true;

        private bool _isCurrentlyFiring;

        private bool _isStrafing;

        protected override void Awake()
        {
            Instance = this;
            base.Awake();

            _rb = GetComponent<Rigidbody2D>();
            _follower = GetComponent<Follower>();
            _anim = GetComponent<Animator>();

            _anim.runtimeAnimatorController = _info.CharacterAnimator;

            _mainCam = Camera.main;
        }
        
        protected override void Start()
        {
            base.Start();
            StartInternal();
        }

        protected virtual void StartInternal()
        {
            PlayerManager.Instance.Register(this);
            GetComponent<Follower>().SetInfo(false);
        }

        // I didn't want to make _lastDir public, but I still wanted the follower class to be able to refer to it. 
        public Vector2 GetLastDirection()
        {
            return _lastDir;
        }

        private void FixedUpdate()
        {
            if (VNManager.Instance.IsPlayingStory)
            {
                _rb.velocity = Vector2.zero;
            }
            else
            if (_isDashing)
            {
                _rb.velocity = _lastDir * 3f * Info.Speed;
            }
            else
            {
                _rb.velocity = _mov * Info.Speed;
            }

            _anim.SetFloat("X", OneOne(_lastDir.x));
            _anim.SetFloat("Y", OneOne(_lastDir.y));
            _anim.enabled = getVelocityMagnitude();
        }

        public bool getVelocityMagnitude()
        {
            return _rb.velocity.magnitude != 0f;
        }

        private float OneOne(float x)
        {
            if (x < 0f) return -1f;
            if (x > 0f) return 1f;
            return 0f;
        }

        private void OnDisable()
        {
            _rb.velocity = Vector2.zero;
            _mov = Vector2.zero;
            _isCurrentlyFiring = false;
            _isDashing = false;
        }

        public void TargetMe()
        {
            _cam.Target.TrackingTarget = transform;
        }

        public IEnumerator RapidFireCoroutine()
        {
            while (_isDashing || VNManager.Instance.IsPlayingStory) yield return new WaitForEndOfFrame(); // Can't shoot while dashing
            if (_isCurrentlyFiring && _canShoot)
            {
                Shoot(_lastDir, true);
                _canShoot = false;
                yield return new WaitForSeconds(Info.ReloadTime);
                _canShoot = true;

                if (_isCurrentlyFiring)
                {
                    yield return RapidFireCoroutine();
                }
            }
        }

        private IEnumerator DashExecute()
        {
            _isDashing = true;
            _canDash = false;

            yield return new WaitForSeconds(.5f);

            _isDashing = false;

            yield return new WaitForSeconds(1f);

            _canDash = true;
        }

        protected override void TakeDamage()
        {
            base.TakeDamage();

            if (PlayerManager.Instance.Follower.gameObject.GetInstanceID() == gameObject.GetInstanceID())
            {
                _follower.Switch();
            }
            PlayerManager.Instance.Follower.gameObject.SetActive(false);
        }

        public void OnMove(InputAction.CallbackContext value)
        {
            _mov = value.ReadValue<Vector2>();
            
            if (_mov.magnitude != 0f && !_isDashing && !_isStrafing)
            {
                _lastDir = _mov;
            }
        }

        public void OnSwitchCharacter(InputAction.CallbackContext value)
        {
            if (value.started && !VNManager.Instance.IsPlayingStory)
            {
                _follower.Switch();
            }
        }

        // expects that the Strafe Input Event is an Axis; 
        // allows us to more easily read it as a button that'd be 'Pressed' and 'Released'.
        public void OnStrafe(InputAction.CallbackContext value)
        {
            if (value.started && !VNManager.Instance.IsPlayingStory)
            {
                print("is strafing");
                _isStrafing = true;
            }
            
            else if (value.canceled)
            {
                print("not strafing.");
                _isStrafing = false;
                // need to update _lastDir. 
                _lastDir = _mov;
            }
        }
        
        public void OnDash(InputAction.CallbackContext value)
        {
            if (value.started && _canDash)
            {
                StartCoroutine(DashExecute());
            }
        }

        public void OnShoot(InputAction.CallbackContext value)
        {
            // if button pressed, start fire
            if (value.started)
            {
                _isCurrentlyFiring = true;
                if (_canShoot)
                {
                    StartCoroutine(RapidFireCoroutine());
                }
            }

            // if button release, cease fire.
            else if (value.canceled)
            {
                _isCurrentlyFiring = false;
            }
        }

        public void OnBomb(InputAction.CallbackContext value)
        {
            // pass in partner once we keep track of that.
            if (value.started && LoveMeter.Instance.CanBomb(PlayerManager.Instance.Follower.Info.Name) && !VNManager.Instance.IsPlayingStory)
            {
                LoveMeter.Instance.UsePower(PlayerManager.Instance.Follower.Info.Name);
                
                var bounds = _mainCam.CalculateBounds();

                for (int i = EnemyManager.Instance.Enemies.Count - 1; i >= 0; i--)
                {
                    var e = EnemyManager.Instance.Enemies[i];
                    if (bounds.Contains((Vector2)e.transform.position))
                    {
                        e.TakeDamage(25);
                    }
                }
            }
        }
    }
}
