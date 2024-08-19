using System;
using _Game.Scripts.Interaction;
using _Game.Scripts.UI;
using GeneralUtils;
using TMPro;
using DG.Tweening;
using UnityEngine;

namespace _Game.Scripts {
    public class PlayerController : MonoBehaviour {
        [SerializeField] private Rigidbody _rb;
        [SerializeField] private Collider _collider;
        [SerializeField] private Transform _cameraTarget;
        [SerializeField] private CollisionTracker _groundCollisionTracker;
        [SerializeField] private Interactor _interactor;
        [SerializeField] private ClimbingComponent _climbingComponent;
        [SerializeField] private GameObject _pickaxeHolder;

        [Header("Look Settings")]
        [SerializeField] private float _horizontalRotationSpeed;
        [SerializeField] private float _verticalRotationSpeed;
        [SerializeField] private float _webGLScale;

        [Header("Move Settings")]
        [SerializeField] private float _movementSpeed;
        [SerializeField] private float _maxSlope;

        [Header("Jump Settings")]
        [SerializeField] private float _jumpForce;
        [SerializeField] private float _climbingJumpForce;
        [SerializeField] private float _coyoteTime;
        [SerializeField] private float _gravity;
        [SerializeField] private float _fallGravityMultiplier;
        [SerializeField] [Range(0f, 1f)] private float _jumpManeuverability;
        [SerializeField] [Range(0f, 1f)] private float _slideManeuverability;

        [Header("Climb Settings")]
        [SerializeField] private float _climbingSpeed;

        private CameraController _camera;
        private TextMeshProUGUI _debugText;
        private ProgressBar _staminaProgressBar;
        private Transform _originalParent;
        private bool _canClimb;

        private readonly UpdatedValue<State> _state = new UpdatedValue<State>(State.None);
        private Vector2 _moveInput;
        private bool _jumpInput;
        private bool _climbInput;

        private Contact _slidingContact;
        private Vector3 _velocity;
        private float _remainingCoyoteTime;

        private bool _debugFreeze;

        public void Init(CameraController camera, TextMeshProUGUI debugText, ProgressBar staminaProgressBar, bool canClimb) {
            _camera = camera;
            _camera.SetTarget(_cameraTarget);
            _debugText = debugText;
            _staminaProgressBar = staminaProgressBar;
            _originalParent = transform.parent;

            var ignoredColliders = GetComponentsInChildren<Collider>();
            _interactor.Init(_camera.CameraTransform, ignoredColliders);
            _climbingComponent.Init(() => _maxSlope, ignoredColliders);
            _staminaProgressBar.Load(0f, _climbingComponent.MaxStamina);

            SetCanClimb(canClimb);
        }

        public void SetCanClimb(bool canClimb) {
            _canClimb = canClimb;
            _pickaxeHolder.SetActive(canClimb);
            _climbingComponent.enabled = canClimb;
        }

        public void ReloadInTheSameLevel(PlayerController previous) {
            _interactor.ReloadInTheSameLevel(previous._interactor);
        }

        public void ToggleNoClip() {
            if (App.DevBuild) {
                SetState(_state.Value == State.NoClip ? State.Falling : State.NoClip);
            }
        }

        private void Start() {
            _groundCollisionTracker.Ignore(_collider);
            SetState(State.Grounded);
        }

        private void Update() {
            UpdateInputs();

            _staminaProgressBar.Value = _climbingComponent.Stamina;
        }

        private void FixedUpdate() {
            var deltaTime = Time.fixedDeltaTime;

            var state = GetState(_state.Value, deltaTime);
            SetState(state);

            UpdateMovement(deltaTime);
        }

        private void UpdateInputs() {
            if (Input.GetButtonDown("DebugFreeze") && App.DevBuild) {
                _debugFreeze = !_debugFreeze;
            }

            if (_debugFreeze) {
                return;
            }

            var horizontalInput = Input.GetAxisRaw("Horizontal");
            var verticalInput = Input.GetAxisRaw("Vertical");
            _moveInput = new Vector2(horizontalInput, verticalInput);


            var lookScale =
#if UNITY_WEBGL && !UNITY_EDITOR
                _webGLScale;
#else
                1f;
#endif

            var horizontalRotationInput = Input.GetAxis("Mouse X");
            var horizontalRotation = horizontalRotationInput * _horizontalRotationSpeed * lookScale;


            var verticalRotationInput = -Input.GetAxis("Mouse Y");
            var deltaVerticalRotation = verticalRotationInput * _verticalRotationSpeed * lookScale;

            Rotate(horizontalRotation, deltaVerticalRotation);

            _jumpInput = _jumpInput || Input.GetButtonDown("Jump");
            _climbInput = _canClimb && _climbInput || Input.GetButtonDown("Climb");

            if (_state.Value == State.NoClip) {
                UpdateNoClipMovement(Time.deltaTime);
            }

            // _debugText.text = $"{transform.rotation.eulerAngles}\n{transform.localRotation.eulerAngles}" +
            //                   $"\n{_cameraTarget.rotation.eulerAngles}\n{_cameraTarget.localRotation.eulerAngles}" +
            //                   $"\n\n{horizontalRotation} {deltaVerticalRotation}";
        }

        private void UpdateNoClipMovement(float deltaTime) {
            var movementRotation =
                Quaternion.Euler(transform.rotation.eulerAngles.With(x: _cameraTarget.localRotation.eulerAngles.x));

            var movementSpeed = new Vector3(_moveInput.x, 0, _moveInput.y).normalized * _movementSpeed;
            var movementVelocity = movementRotation * movementSpeed;
            transform.Translate(movementVelocity * deltaTime * 10, Space.World);
        }

        private void UpdateMovement(float deltaTime) {
            if (_state.Value == State.NoClip) {
                return;
            }

            var climbInput = _climbInput;
            _climbInput = false;
            if (climbInput && CanFall() && TryStartClimbing()) {
                SetState(State.Climbing);
                return;
            }

            var jumpInput = _jumpInput;
            _jumpInput = false;
            if (_state.Value == State.Climbing) {
                if (jumpInput) {
                    Jump();
                } else if (climbInput || _climbingComponent.CantClimb) {
                    SetState(State.Falling);
                } else {
                    HandleClimbing(deltaTime);
                }

                return;
            }

            TryLatchOnMovingPlatforms();

            var currentDirection = Quaternion.FromToRotation(Vector3.forward, transform.forward);
            var movementSpeed = new Vector3(_moveInput.x, 0, _moveInput.y).normalized * _movementSpeed;
            var movementSpeedRotated = currentDirection * movementSpeed;

            var velocityAdjusted = CanAdjustForSlope()
                ? AdjustVelocityForSlopes(movementSpeedRotated)
                : movementSpeedRotated;

            var maneuverability = InTheAir()
                ? _jumpManeuverability
                : CanSlide()
                    ? _slideManeuverability
                    : 1f;
            var newVelocity = Vector3.Lerp(_velocity, velocityAdjusted, maneuverability);
            if (!CanAdjustForSlope() || CanSlide()) {
                newVelocity.y = _velocity.y;
            }

            _velocity = newVelocity;
            Move(newVelocity);

            if (CanFall()) {
                var gravity = _state.Value == State.Falling ? _fallGravityMultiplier * _gravity : _gravity;
                _velocity.y -= gravity * deltaTime;
            } else {
                _velocity.y = 0;
            }

            if (jumpInput) {
                Jump();
            }
        }

        private bool InTheAir() => _state.Value == State.Jumping || _state.Value == State.Falling;
        private bool CanFall() => InTheAir() || CanSlide();
        private bool CanSlide() => _state.Value == State.Sliding;
        private bool CanAdjustForSlope() => _state.Value == State.Grounded || CanSlide();

        private Vector3 AdjustVelocityForSlopes(Vector3 velocity) {
            Contact lowest = null;
            foreach (var collision in _groundCollisionTracker.Collisions) {
                var contact = _groundCollisionTracker.GetContact(collision);
                if ((lowest == null || contact.Point.y < lowest.Point.y) && collision.attachedRigidbody == null) {
                    lowest = contact;
                }
            }

            if (lowest != null && Vector3.Angle(lowest.Normal, Vector3.up) <= _maxSlope) {
                var moveRotation = Quaternion.FromToRotation(Vector3.up, lowest.Normal);
                velocity = moveRotation * velocity;
            }

            return velocity;
        }

        private void TryLatchOnMovingPlatforms() {
            foreach (var collision in _groundCollisionTracker.Collisions) {
                if (collision.gameObject.CompareTag("Moving")) {
                    _rb.transform.SetParent(collision.transform, true);
                    return;
                }
            }

            _rb.transform.SetParent(_originalParent, true);
        }

        private void Move(Vector3 speed) {
            _rb.velocity = speed;

            if (_state.Value == State.Grounded) {
                string sound = IsMetalGround() ? "walk_metal" : "walk_default";
                if (speed.magnitude > 0f) {
                    SoundController.Instance.PlaySound(sound, 0.1f, 1f, false);
                } else {
                    SoundController.Instance.StopSound(sound, 0.1f);
                }
            } else if (_state.Value == State.Sliding) {
                if (speed.y < -5f) {
                    SoundController.Instance.PlaySound("slide", 0.1f, 0.5f, false);
                } else {
                    SoundController.Instance.StopSound("slide", 0.1f);
                }
            }
        }

        private void Rotate(float horizontalRotation, float verticalRotation) {
            float horizontalCameraRotation;
            if (_state.Value != State.Climbing) {
                transform.Rotate(Vector3.up, horizontalRotation);
                horizontalCameraRotation = 0f;
            } else {
                horizontalCameraRotation = horizontalRotation;
            }

            var currentVerticalRotation = _cameraTarget.localRotation.eulerAngles.x;
            var adjustedVerticalRotation =
                currentVerticalRotation > 180 ? currentVerticalRotation - 360 : currentVerticalRotation;
            var newVerticalRotation = Mathf.Clamp(adjustedVerticalRotation + verticalRotation, -90, 90);

            var newRotation = _cameraTarget.localRotation.eulerAngles;
            newRotation.y += horizontalCameraRotation;
            newRotation.x = newVerticalRotation;
            _cameraTarget.localRotation = Quaternion.Euler(newRotation);
        }

        private void Jump() {
            Vector3 normal;
            float force;
            switch (_state.Value) {
                case State.Grounded:
                    normal = Vector3.up;
                    force = _jumpForce;
                    break;
                case State.Sliding:
                    normal = _slidingContact.Normal;
                    force = _jumpForce;
                    break;
                case State.Climbing:
                    normal = _camera.CameraTransform.forward;
                    force = _climbingJumpForce;
                    break;
                default:
                    return;
            }

            SetState(State.Jumping);
            SoundController.Instance.PlaySound(IsMetalGround() ? "jump_metal" : "jump_default", 0.2f);
            _velocity.y = 0;
            _velocity += normal * force;
        }

        private bool CheckGroundCollision() {
            foreach (var collision in _groundCollisionTracker.Collisions) {
                var contact = _groundCollisionTracker.GetContact(collision);
                var angle = Vector3.Angle(Vector3.up, contact.Normal);
                if (angle <= _maxSlope) {
                    return true;
                }
            }

            return false;
        }

        private bool CheckSlidingCollision() {
            foreach (var collision in _groundCollisionTracker.Collisions) {
                var contact = _groundCollisionTracker.GetContact(collision);
                var angle = Vector3.Angle(Vector3.up, contact.Normal);
                if (angle > _maxSlope && angle < 90) {
                    _slidingContact = contact;
                    return true;
                }
            }

            return false;
        }

        private bool IsMetalGround() {
            foreach (var collision in _groundCollisionTracker.Collisions) {
                if (collision.gameObject.name.StartsWith("Metal")) {
                    return true;
                }
            }

            return false;
        }

        private State GetState(State current, float deltaTime) {
            switch (current) {
                case State.Climbing:
                    return _climbingComponent.ClimbContact != null ? State.Climbing : State.Falling;
                case State.NoClip:
                    return State.NoClip;
                case State.None:
                case State.Grounded:
                case State.Falling:
                case State.Sliding:
                    var groundCollision = CheckGroundCollision();
                    bool grounded;
                    if (current == State.Grounded) {
                        _remainingCoyoteTime -= deltaTime;
                        grounded = _remainingCoyoteTime > 0 || groundCollision;
                    } else {
                        grounded = groundCollision;
                    }

                    return grounded
                        ? State.Grounded
                        : CheckSlidingCollision()
                            ? State.Sliding
                            : State.Falling;
                case State.Jumping:
                    return _velocity.y > 0 ? State.Jumping : GetState(State.Falling, deltaTime);
                default:
                    throw new ArgumentOutOfRangeException(nameof(current), current, null);
            }
        }

        private void SetState(State state) {
            if (state == _state.Value) {
                return;
            }

            Debug.Log($"State {_state.Value} to {state}");

            if (_debugText != null) {
                _debugText.text = state.ToString();
            }

            _rb.isKinematic = state == State.NoClip;

            if (_state.Value == State.Climbing) {
                transform.rotation = Quaternion.Euler(_cameraTarget.rotation.eulerAngles.With(x: 0, z: 0));
                _cameraTarget.localRotation = Quaternion.Euler(Vector3.zero.With(x: _cameraTarget.localRotation.x));
                _climbingComponent.LatchOff();
            }

            switch (state) {
                case State.NoClip:
                    _velocity = Vector3.zero;
                    _rb.velocity = Vector3.zero;
                    break;
                case State.Grounded:
                    _remainingCoyoteTime = _coyoteTime;
                    _climbingComponent.Land();
                    break;
                case State.Climbing:
                    _velocity = Vector3.zero;
                    MoveToClimbingContact();
                    break;
            }

            UpdateSoundOnStateChange(_state.Value, state);

            _state.Value = state;
        }

        private void UpdateSoundOnStateChange(State oldState, State newState) {
            switch (oldState) {
                case State.Grounded:
                    SoundController.Instance.StopSound("walk_default", 0.3f);
                    SoundController.Instance.StopSound("walk_metal", 0.3f);
                    break;
                case State.Sliding:
                    SoundController.Instance.StopSound("slide", 0.1f);
                    break;
                case State.NoClip:
                    SoundController.Instance.StopSound("noclip", 0.5f);
                    break;
                case State.Falling:
                    SoundController.Instance.StopSound("fall", 0.5f);
                    break;
            }

            switch (newState) {
                case State.Grounded when _velocity.y < -5f:
                    var sound = IsMetalGround() ? "land_metal" : "land_default";
                    SoundController.Instance.PlaySound(sound, 0.05f, 1f, false);
                    break;
                case State.Grounded when _velocity.y < -2f:
                    SoundController.Instance.PlaySound("land_smooth", 0.2f, 1f, false);
                    break;
                case State.Falling:
                    SoundController.Instance.PlaySound("fall", 0f, 1.5f, false, true).DOFade(0.08f, 5f);
                    ;
                    break;
                case State.NoClip:
                    SoundController.Instance.PlaySound("noclip", 0.1f, 1f, false, true);
                    break;
            }
        }

        private enum State {
            None,
            Grounded,
            Sliding,
            Jumping,
            Falling,
            Climbing,
            NoClip
        }

        private void OnCollisionEnter(Collision collision) {
            if (_state.Value == State.NoClip) {
                return;
            }

            var normal = collision.GetContact(0).normal;
            var horizontalNormal = new Vector3(normal.x, 0, normal.z).normalized;
            var horizontalVelocity = new Vector3(_velocity.x, 0, _velocity.z);
            if (Vector3.Angle(horizontalNormal, horizontalVelocity) > 90) {
                _velocity += Vector3.Project(-_velocity, horizontalNormal);
            }
        }

        private Vector3 _lastClimbingNormal;

        private void MoveToClimbingContact() {
            var (position, direction) =
                _climbingComponent.GetLatchPositionAndPossibleDirection(_climbingComponent.ClimbContact);
            var newDirection = direction ?? transform.forward;

            _rb.position = position;
            transform.rotation =
                Quaternion.Euler(Vector3.up * Vector3.SignedAngle(Vector3.forward, newDirection, Vector3.up));
            _cameraTarget.localRotation = Quaternion.Euler(Vector3.zero.With(x: _cameraTarget.localRotation.x));
            _lastClimbingNormal = _climbingComponent.ClimbContact.Normal;

            if (direction == null) {
                SetState(State.Falling);
            }
        }

        private bool TryStartClimbing() {
            return _climbingComponent.LatchOnContact != null && _climbingComponent.LatchOn();
        }

        private void HandleClimbing(float deltaTime) {
            if (_climbingComponent.ClimbContact.Normal != _lastClimbingNormal) {
                MoveToClimbingContact();
            }

            var climbingInput = (transform.right * _moveInput.x + transform.up * _moveInput.y).normalized;
            var movement = Vector3.ProjectOnPlane(climbingInput, _lastClimbingNormal).normalized *
                           _climbingSpeed;

            if (climbingInput != Vector3.zero) {
                _lastClimbInput = climbingInput;
                _lastClimbVel = movement;
            }

            Move(movement);
            _climbingComponent.Move(_moveInput, movement, deltaTime);
        }

        private Vector3 _lastClimbInput;
        private Vector3 _lastClimbVel;

        private void OnDrawGizmos() {
            Gizmos.color = Color.magenta;
            Gizmos.DrawLine(transform.position, transform.position + _lastClimbInput);
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(transform.position, transform.position + _lastClimbVel);
        }
    }
}