using Ink.Runtime;
using System.Linq;
using TMPro;
using TouhouPride.Manager;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace TouhouPride.VN
{
    public class VNManager : MonoBehaviour
    {
        public static VNManager Instance { private set; get; }

        [SerializeField]
        private CinemachineCamera _cam;

        [SerializeField]
        private TextDisplay _display;

        private Story _story;

        [SerializeField]
        private GameObject _container;

        [SerializeField]
        private TMP_Text _nameText;

        private bool _isSkipEnabled;
        private float _skipTimer;
        private float _skipTimerRef = .1f;

        private System.Action _onEnd;

        private void Awake()
        {
            Instance = this;
        }

        public bool IsPlayingStory => _container.activeInHierarchy;

        private void Update()
        {
            if (_isSkipEnabled)
            {
                _skipTimer -= Time.deltaTime;
                if (_skipTimer < 0)
                {
                    _skipTimer = _skipTimerRef;
                    DisplayNextDialogue();
                }
            }
        }

        public void PlayBossStory()
        {
            // TODO: Do that properly
            ShowStory(PlayerManager.Instance.Follower.Info.EndStory, () =>
            {
                PlayerManager.Instance.Boss.AllowDamage();
            });
        }

        private void ResetVN()
        {
            _isSkipEnabled = false;
        }

        public void ShowStory(TextAsset asset, System.Action onEnd = null)
        {
            Debug.Log($"[STORY] Playing {asset.name}");
            _story = new(asset.text);
            _onEnd = onEnd;
            ResetVN();
            DisplayStory(_story.Continue());
        }

        private void DisplayStory(string text)
        {
            _container.SetActive(true);

            foreach (var tag in _story.currentTags)
            {
                var s = tag.Split(' ');
                var content = string.Join(' ', s.Skip(1));
                switch (s[0])
                {
                    case "speaker":
                        _nameText.text = content;
                        _cam.Target.TrackingTarget = content.ToUpperInvariant() switch
                        {
                            "MARISA" => PlayerManager.Instance.Player.transform,
                            "MEIRA" => PlayerManager.Instance.Boss.transform,
                            _ => PlayerManager.Instance.Follower.transform,
                        };
                        break;
                }
            }
            _display.ToDisplay = text;
        }

        public void DisplayNextDialogue()
        {
            if (!_container.activeInHierarchy)
            {
                return;
            }
            if (!_display.IsDisplayDone)
            {
                // We are slowly displaying a text, force the whole display
                _display.ForceDisplay();
            }
            else if (_story.canContinue && // There is text left to write
                !_story.currentChoices.Any()) // We are not currently in a choice
            {
                DisplayStory(_story.Continue());
            }
            else if (!_story.canContinue && !_story.currentChoices.Any())
            {
                _container.SetActive(false);
                _onEnd?.Invoke();
            }
        }

        public void ToggleSkip()
        {
            _isSkipEnabled = !_isSkipEnabled;
        }

        public void OnNextDialogue(InputAction.CallbackContext value)
        {
            if (value.performed && _container.activeInHierarchy && !_isSkipEnabled)
            {
                ResetVN();
                DisplayNextDialogue();
            }
        }

        public void OnSkip(InputAction.CallbackContext value)
        {
            if (value.phase == InputActionPhase.Started)
            {
                _isSkipEnabled = true;
            }
            else if (value.phase == InputActionPhase.Canceled)
            {
                _isSkipEnabled = false;
            }
        }
    }
}