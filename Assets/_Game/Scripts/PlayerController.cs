using System;
using System.Linq;
using TMPro;
using UnityEngine;

namespace _Game.Scripts {
    public class PlayerController : MonoBehaviour {
        [SerializeField] private Rigidbody _rb;
        [SerializeField] private Collider _collider;
        [SerializeField] private float _movementSpeed;
        [SerializeField] private Camera _camera;
        [SerializeField] private float _horizontalRotationSpeed;
        [SerializeField] private float _verticalRotationSpeed;
        [SerializeField] private float _jumpForce;
        [SerializeField] private float _maxSlope;
        [SerializeField] private CollisionTracker _groundCollisionTracker;
        [SerializeField] private CollisionTracker _anyCollisionTracker;
        [SerializeField] private CollisionTracker _wallCollisionTracker;

        private TextMeshProUGUI _stateText;

        private State _state = State.None;
        private Vector2 _moveInput;
        private bool _jumpInput;

        public void Init(TextMeshProUGUI stateText) {
            _stateText = stateText;
        }

        private void Start() {
            _groundCollisionTracker.Ignore(_collider);
            SetState(State.Grounded);
        }

        private void Update() {
            UpdateInputs();
        }

        private void FixedUpdate() {
            var state = GetState(_state);
            SetState(state);

            UpdateMovement();
        }

        private void UpdateInputs() {
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

            _jumpInput = _jumpInput || Input.GetButtonDown("Jump");
        }

        private void UpdateMovement() {
            var currentDirection = Quaternion.FromToRotation(Vector3.forward, transform.forward);
            var movementSpeed = new Vector3(_moveInput.x, 0, _moveInput.y).normalized * _movementSpeed;
            var movementSpeedRotated = currentDirection * movementSpeed;

            Move(movementSpeedRotated, Time.fixedDeltaTime);

            if (_jumpInput) {
                _jumpInput = false;
                Jump();
            }
        }
        
        private void Move(Vector3 speed, float deltaTime) {
            // if (_state == State.Falling) {
            //     return;
            // }

            if (_state == State.Grounded) {
                Contact lowest = null;
                foreach (var collision in _groundCollisionTracker.Collisions) {
                    var contact = _groundCollisionTracker.GetContact(collision);
                    if ((lowest == null || contact.Point.y < lowest.Point.y) &&
                        (collision.attachedRigidbody == null || collision.attachedRigidbody.mass > _rb.mass)) {
                        lowest = contact;
                    }
                }

                if (lowest != null && Vector3.Angle(lowest.Normal, Vector3.up) <= _maxSlope) {
                    var moveRotation = Quaternion.FromToRotation(Vector3.up, lowest.Normal);
                    speed = moveRotation * speed;
                }
            }

            
            foreach (var wallCollision in _wallCollisionTracker.Collisions) {
                var contact = _wallCollisionTracker.GetContact(wallCollision);
                var reverseSpeed = -speed;
                if (Vector3.Angle(reverseSpeed, contact.Normal) < 90) {
                    speed += Vector3.Project(reverseSpeed, contact.Normal);
                } 
            }

            // TODO this shit helps traverse slopes and edges but is really bad when walls
            _rb.MovePosition(_rb.position + speed * deltaTime);

            // var vertical = _state == State.Grounded && speed == Vector3.zero ? 0f : _rb.velocity.y;
            // _rb.velocity = new Vector3(speed.x, vertical, speed.z);
            // _lastSetVel = new Vector3(speed.x, vertical, speed.z);

            if (_state == State.Grounded) {
                _rb.velocity = Vector3.zero;
            }
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

        private bool CheckGroundCollision() {
            // return _groundCollisionTracker.Collisions.Any();
            
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
            foreach (var collision in _anyCollisionTracker.Collisions) {
                var contact = _anyCollisionTracker.GetContact(collision);
                var angle = Vector3.Angle(Vector3.up, contact.Normal);
                if (angle < 90 && angle > _maxSlope) {
                    return true;
                }
            }

            return false;
        }

        private State GetState(State current) {
            switch (current) {
                case State.None:
                case State.Grounded:
                case State.Falling:
                case State.Sliding:
                    return CheckGroundCollision() ? State.Grounded : /*CheckSlidingCollision() ? State.Sliding :*/ State.Falling;
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
            // if (_state == State.Grounded) {
            //     _rb.velocity = Vector3.zero;
            // }
            // _rb.isKinematic = state == State.Grounded;
            // _collider.enabled = state != State.Grounded;
            // _characterController.enabled = state == State.Grounded;
        }

        private enum State {
            None,
            Grounded,
            Sliding,
            Jumping,
            Falling
        }

        private Vector3 _lastSetVel;
        private void OnDrawGizmos() {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, transform.position + _lastSetVel);
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(transform.position, transform.position + _rb.velocity);
        }
    }
}