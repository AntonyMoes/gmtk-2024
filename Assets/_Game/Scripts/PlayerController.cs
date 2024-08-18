using System;
using System.Collections.Generic;
using System.Linq;
using GeneralUtils;
using TMPro;
using UnityEngine;

namespace _Game.Scripts {
    public class PlayerController : MonoBehaviour {
        [SerializeField] private Rigidbody _rb;
        [SerializeField] private Collider _collider;
        [SerializeField] private Transform _cameraTarget;
        [SerializeField] private CollisionTracker _groundCollisionTracker;

        [Header("Settings")] [SerializeField] private float _movementSpeed;
        [SerializeField] private float _horizontalRotationSpeed;
        [SerializeField] private float _verticalRotationSpeed;
        [SerializeField] private float _jumpForce;
        [SerializeField] private float _gravity;
        [SerializeField] private float _fallGravityMultiplier;
        [SerializeField] [Range(0f, 1f)] private float _jumpManeuverability;
        [SerializeField] [Range(0f, 1f)] private float _slideManeuverability;
        [SerializeField] private float _maxSlope;

        private CameraController _camera;
        private TextMeshProUGUI _stateText;

        private readonly UpdatedValue<State> _state = new UpdatedValue<State>(State.None);
        private Vector2 _moveInput;
        private bool _jumpInput;

        private Contact _slidingContact;
        private Vector3 _velocity;

        public void Init(CameraController camera, TextMeshProUGUI stateText) {
            _camera = camera;
            _camera.SetTarget(_cameraTarget);
            _stateText = stateText;
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
        }

        private void FixedUpdate() {
            var state = GetState(_state.Value);
            SetState(state);

            UpdateMovement(Time.fixedDeltaTime);
        }

        private void UpdateInputs() {
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
            Move(newVelocity, Time.fixedDeltaTime);

            if (CanFall()) {
                var gravity = _state.Value == State.Falling ? _fallGravityMultiplier * _gravity : _gravity;
                _velocity.y -= gravity * deltaTime;
            } else {
                _velocity.y = 0;
            }

            if (_jumpInput) {
                _jumpInput = false;
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
                if ((lowest == null || contact.Point.y < lowest.Point.y) &&
                    (collision.attachedRigidbody == null /*|| collision.attachedRigidbody.mass > _rb.mass*/)) {
                    lowest = contact;
                }
            }

            if (lowest != null && Vector3.Angle(lowest.Normal, Vector3.up) <= _maxSlope) {
                var moveRotation = Quaternion.FromToRotation(Vector3.up, lowest.Normal);
                velocity = moveRotation * velocity;
            }

            return velocity;
        }

        private void Move(Vector3 speed, float deltaTime) {
            _lastSetVel = speed;
            _rb.velocity = speed;
        }

        private void Rotate(float rotation) {
            transform.Rotate(Vector3.up, rotation);
        }

        private void Jump() {
            switch (_state.Value) {
                case State.Grounded:
                case State.Sliding:
                    var normal = _state.Value == State.Sliding ? _slidingContact.Normal : Vector3.up;
                    SetState(State.Jumping);
                    _velocity.y = 0;
                    _velocity += normal * _jumpForce;
                    break;
            }
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

        private State GetState(State current) {
            switch (current) {
                case State.NoClip:
                    return State.NoClip;
                case State.None:
                case State.Grounded:
                case State.Falling:
                case State.Sliding:
                    return CheckGroundCollision()
                        ? State.Grounded
                        : CheckSlidingCollision()
                            ? State.Sliding
                            : State.Falling;
                case State.Jumping:
                    // return _rb.velocity.y > 0 ? State.Jumping : GetState(State.Falling);
                    return _velocity.y > 0 ? State.Jumping : GetState(State.Falling);
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
            if (state == State.NoClip) {
                _velocity = Vector3.zero;
                _rb.velocity = Vector3.zero;
            }

            _state.Value = state;
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

        private Vector3 _lastSetVel;
        private List<(Vector3, Vector3)> _rays = new List<(Vector3, Vector3)>();
        private List<(Vector3, Vector3)> _lastRays = new List<(Vector3, Vector3)>();

        private void OnDrawGizmos() {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, transform.position + _velocity);
            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position, transform.position + _lastSetVel);
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(transform.position, transform.position + _rb.velocity);
        }
    }
}