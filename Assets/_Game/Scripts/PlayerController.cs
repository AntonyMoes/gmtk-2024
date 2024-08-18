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

        [Header("Look Settings")] [SerializeField]
        private float _horizontalRotationSpeed;

        [SerializeField] private float _verticalRotationSpeed;

        [Header("Move Settings")] [SerializeField]
        private float _movementSpeed;

        [SerializeField] private float _maxSlope;

        [Header("Jump Settings")] [SerializeField]
        private float _jumpForce;

        [SerializeField] private float _coyoteTime;
        [SerializeField] private float _gravity;
        [SerializeField] private float _fallGravityMultiplier;
        [SerializeField] [Range(0f, 1f)] private float _jumpManeuverability;
        [SerializeField] [Range(0f, 1f)] private float _slideManeuverability;

        [Header("Climb Settings")] [SerializeField]
        private float _climbingSpeed;

        private CameraController _camera;
        private TextMeshProUGUI _stateText;
        private ProgressBar _staminaProgressBar;
        private Transform _originalParent;

        private readonly UpdatedValue<State> _state = new UpdatedValue<State>(State.None);
        private Vector2 _moveInput;
        private bool _jumpInput;
        private bool _climbInput;

        private Contact _slidingContact;
        private Vector3 _velocity;
        private float _remainingCoyoteTime;

        private bool _debugFreeze;

        public void Init(CameraController camera, TextMeshProUGUI stateText, ProgressBar staminaProgressBar) {
            _camera = camera;
            _camera.SetTarget(_cameraTarget);
            _stateText = stateText;
            _staminaProgressBar = staminaProgressBar;
            _originalParent = transform.parent;

            var ignoredColliders = GetComponentsInChildren<Collider>();
            _interactor.Init(_camera.CameraTransform, ignoredColliders);
            _climbingComponent.Init(ignoredColliders);
            _staminaProgressBar.Load(0f, _climbingComponent.MaxStamina);
        }

        public void ReloadInTheSameLevel(PlayerController previous) {
            _interactor.ReloadInTheSameLevel(previous._interactor);
        }

        public void ToggleNoClip() {
            SetState(_state.Value == State.NoClip ? State.Falling : State.NoClip);
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
            if (Input.GetButtonDown("DebugFreeze")) {
                _debugFreeze = !_debugFreeze;
            }

            if (_debugFreeze) {
                return;
            }

            var horizontalInput = Input.GetAxisRaw("Horizontal");
            var verticalInput = Input.GetAxisRaw("Vertical");
            _moveInput = new Vector2(horizontalInput, verticalInput);

            var horizontalRotationInput = Input.GetAxis("Mouse X");
            var horizontalRotation = horizontalRotationInput * _horizontalRotationSpeed /** deltaTime*/;
            Rotate(horizontalRotation);

            var verticalRotationInput = -Input.GetAxis("Mouse Y");
            var deltaVerticalRotation = verticalRotationInput * _verticalRotationSpeed /** deltaTime*/;

            var verticalRotation = _camera.transform.localRotation.eulerAngles.x;
            var adjustedVerticalRotation = verticalRotation > 180 ? verticalRotation - 360 : verticalRotation;
            var newVerticalRotation = Mathf.Clamp(adjustedVerticalRotation + deltaVerticalRotation, -90, 90);
            _camera.VerticalRotation = newVerticalRotation;

            _jumpInput = _jumpInput || Input.GetButtonDown("Jump");
            _climbInput = _climbInput || Input.GetButtonDown("Climb");

            if (_state.Value == State.NoClip) {
                UpdateNoClipMovement(Time.deltaTime);
            }
        }

        private void UpdateNoClipMovement(float deltaTime) {
            var movementRotation = Quaternion.Euler(transform.rotation.eulerAngles.With(x: _camera.VerticalRotation));

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
                    _rb.gameObject.transform.parent = collision.transform;
                    return;
                }
            }

            _rb.gameObject.transform.parent = _originalParent;
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

        private void Rotate(float rotation) {
            if (_state.Value != State.Climbing) {
                transform.Rotate(Vector3.up, rotation);
            } else {
                _cameraTarget.Rotate(Vector3.up, rotation);
            }
        }

        private void Jump() {
            Vector3 normal;
            switch (_state.Value) {
                case State.Grounded:
                    normal = Vector3.up;
                    break;
                case State.Sliding:
                    normal = _slidingContact.Normal;
                    break;
                case State.Climbing:
                    normal = _camera.CameraTransform.forward;
                    break;
                default:
                    return;
            }

            SetState(State.Jumping);
            SoundController.Instance.PlaySound(IsMetalGround() ? "jump_metal" : "jump_default", 0.1f);
            _velocity.y = 0;
            _velocity += normal * _jumpForce;
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

            if (_stateText != null) {
                _stateText.text = state.ToString();
            }

            _rb.isKinematic = state == State.NoClip;

            if (_state.Value == State.Climbing) {
                transform.rotation = Quaternion.Euler(_cameraTarget.rotation.eulerAngles.With(x: 0, z: 0));
                _cameraTarget.localRotation = Quaternion.Euler(Vector3.zero);
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
                _climbingComponent.GetLatchPositionAndDirection(_climbingComponent.ClimbContact);
            _rb.position = position;
            transform.rotation = Quaternion.Euler(Vector3.up * Vector3.SignedAngle(Vector3.forward, direction, Vector3.up));
            _lastClimbingNormal = _climbingComponent.ClimbContact.Normal;
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

            // TODO block movement if

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