using System;
using System.Linq;
using TMPro;
using UnityEngine;

namespace _Game.Scripts {
    public class PlayerController : MonoBehaviour {
        // [SerializeField] private CharacterController _characterController;
        [SerializeField] private Rigidbody _rb;
        [SerializeField] private Collider _collider;
        [SerializeField] private float _movementSpeed;
        [SerializeField] private Camera _camera;
        [SerializeField] private float _horizontalRotationSpeed;
        [SerializeField] private float _verticalRotationSpeed;
        [SerializeField] private float _jumpForce;
        [SerializeField] private float _maxSlope;
        [SerializeField] private TextMeshProUGUI _stateText;
        [SerializeField] private CollisionTracker _collisionTracker;

        private State _state = State.None;
        private Vector2 _moveInput;

        private void Start() {
            Cursor.visible = false;
            _collisionTracker.Ignore(_collider);
            SetState(State.Grounded);
        }

        private void Update() {
            var deltaTime = Time.deltaTime;

            var state = GetState(_state);
            SetState(state);

            UpdateMovement(deltaTime);
        }

        private void UpdateMovement(float deltaTime) {
            var horizontalInput = Input.GetAxisRaw("Horizontal");
            var verticalInput = Input.GetAxisRaw("Vertical");
            _moveInput = new Vector2(horizontalInput, verticalInput);

            var horizontalRotationInput = Input.GetAxisRaw("Mouse X");
            var horizontalRotation = horizontalRotationInput * _horizontalRotationSpeed /** deltaTime*/;
            Rotate(horizontalRotation);

            var verticalRotationInput = -Input.GetAxisRaw("Mouse Y");
            var deltaVerticalRotation = verticalRotationInput * _verticalRotationSpeed /** deltaTime*/;

            var verticalRotation = _camera.transform.localRotation.eulerAngles.x;
            var adjustedVerticalRotation = verticalRotation > 180 ? verticalRotation - 360 : verticalRotation;
            var newVerticalRotation = Mathf.Clamp(adjustedVerticalRotation + deltaVerticalRotation, -90, 90);
            _camera.transform.localRotation = Quaternion.Euler(newVerticalRotation, 0, 0);

            var jump = Input.GetButtonDown("Jump");
            if (jump) {
                Jump();
            }
        }

        private void FixedUpdate() {
            var currentDirection = Quaternion.FromToRotation(Vector3.forward, transform.forward);
            var movementSpeed = new Vector3(_moveInput.x, 0, _moveInput.y).normalized * _movementSpeed;
            var movementSpeedRotated = currentDirection * movementSpeed;

            Move(movementSpeedRotated, Time.fixedDeltaTime);
        }

        private void Move(Vector3 speed, float deltaTime) {
            Contact lowest = null;
            foreach (var collision in _collisionTracker.Collisions) {
                var contact = _collisionTracker.GetContact(collision);
                if (lowest == null || contact.Point.y < lowest.Point.y) {
                    lowest = contact;
                }
            }

            if (lowest != null && Vector3.Angle(lowest.Normal, Vector3.up) <= _maxSlope) {
                var moveRotation = Quaternion.FromToRotation(Vector3.up, lowest.Normal);
                speed = moveRotation * speed;
            }
            
            // _rb.MovePosition(_rb.position + speed * deltaTime);

            var vertical = _state == State.Grounded && speed == Vector3.zero ? 0f : _rb.velocity.y;
            _rb.velocity = new Vector3(speed.x, vertical, speed.z);
        }

        private void Rotate(float rotation) {
            transform.Rotate(Vector3.up, rotation);
        }

        private void Jump() {
            switch (_state) {
                case State.Grounded:
                    SetState(State.Jumping);
                    _rb.AddForce(Vector3.up * _jumpForce, ForceMode.Impulse);
                    break;
            }
        }

        private State GetState(State current) {

            switch (current) {
                case State.None:
                case State.Grounded:
                case State.Falling:
                    return CheckGroundCollision() ? State.Grounded : State.Falling;
                case State.Jumping:
                    return _rb.velocity.y > 0 ? State.Jumping : GetState(State.Falling);
                default:
                    throw new ArgumentOutOfRangeException(nameof(current), current, null);
            }
        }

        private void SetState(State state) {
            if (state == _state) {
                return;
            }

            Debug.Log($"State {_state} to {state}");

            if (_stateText != null) {
                _stateText.text = state.ToString();
            }

            _state = state;
            _rb.useGravity = state != State.Grounded;
            // _rb.isKinematic = state == State.Grounded;
            // _collider.enabled = state != State.Grounded;
            // _characterController.enabled = state == State.Grounded;
        }

        private enum State {
            None,
            Grounded,
            Jumping,
            Falling
        }

        private bool CheckGroundCollision() {
            return _collisionTracker.Collisions.Any();
        }
    }
}