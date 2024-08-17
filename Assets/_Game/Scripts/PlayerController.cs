using System;
using GeneralUtils;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

namespace _Game.Scripts {
    public class PlayerController : MonoBehaviour {
        [SerializeField] private CharacterController _characterController;
        [SerializeField] private Rigidbody _rb;
        [SerializeField] private Collider _collider;
        [SerializeField] private float _movementSpeed;
        [SerializeField] private Camera _camera;
        [SerializeField] private float _horizontalRotationSpeed;
        [SerializeField] private float _verticalRotationSpeed;
        [SerializeField] private float _jumpForce;
        [SerializeField] private TextMeshProUGUI _stateText;
        [SerializeField] private CollisionTracker _collisionTracker;

        private State _state = State.None;
        // private bool _groundCollision;

        private void Start() {
            Cursor.visible = false;
            SetState(State.Grounded);
        }

        private void Update() {
            var deltaTime = Time.deltaTime;

            var state = GetState(_state);
            SetState(state);

            UpdateMovement(deltaTime);
        }

        private void UpdateMovement(float deltaTime) {
            var currentDirection = Quaternion.FromToRotation(Vector3.forward, transform.forward);

            var horizontalInput = Input.GetAxisRaw("Horizontal");
            var verticalInput = Input.GetAxisRaw("Vertical");
            var movementSpeed = new Vector3(horizontalInput, 0f, verticalInput).normalized * _movementSpeed;
            var movementSpeedRotated = currentDirection * movementSpeed;

            Move(movementSpeedRotated, deltaTime);
            // _characterController.SimpleMove(movementSpeedRotated);

            var horizontalRotationInput = Input.GetAxisRaw("Mouse X");
            var horizontalRotation = horizontalRotationInput * _horizontalRotationSpeed /** deltaTime*/;
            // transform.Rotate(Vector3.up, horizontalRotation);
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

        private void Move(Vector3 speed, float deltaTime) {
            if (_state == State.Grounded) {
                _characterController.SimpleMove(speed);
            } else {
                _rb.MovePosition(_rb.position + speed * deltaTime);
            }
        }

        private void Rotate(float rotation) {
            if (_state == State.Grounded) {
                transform.Rotate(Vector3.up, rotation);
            } else {
                _rb.MoveRotation(Quaternion.Euler(0, _rb.rotation.eulerAngles.y + rotation, 0));
            }
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
            // switch (current) {
            //     case State.Grounded:
            //         // Debug.Log($"Grounded: {CheckGroundCollision()}");
            //         return _characterController.isGrounded ? State.Grounded : State.NotGrounded;
            //     case State.NotGrounded:
            //         // var groundCollision = _groundCollision;
            //         // _groundCollision = false;
            //         // return groundCollision ? State.Grounded : State.NotGrounded;
            //         return CheckGroundCollision() ? State.Grounded : State.NotGrounded;
            //     default:
            //         throw new ArgumentOutOfRangeException(nameof(current), current, null);
            // }

            switch (current) {
                case State.None:
                case State.Grounded:
                case State.Falling:
                    return CheckGroundCollision() ? State.Grounded : State.Falling;
                    break;
                case State.Jumping:
                    return _rb.velocity.y > 0 ? State.Jumping : GetState(State.Falling);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(current), current, null);
            }
            // return CheckGroundCollision() ? State.Grounded : State.NotGrounded;
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
            _rb.isKinematic = state == State.Grounded;
            _collider.enabled = state != State.Grounded;
            _characterController.enabled = state == State.Grounded;
        }

        private enum State {
            None,
            Grounded,
            Jumping,
            Falling
        }

        // private void OnCollisionEnter(Collision collision) {
        //     if (_state != State.NotGrounded) {
        //         return;
        //     }
        //
        //     _groundCollision = CheckGroundCollision(collision);
        // }

        // private bool CheckGroundCollision(Collision collision) {
        //     var slopeLimit = _characterController.slopeLimit;
        //     for (var i = 0; i < collision.contactCount; i++) {
        //         var contact = collision.GetContact(i);
        //
        //         var angle = Vector3.Angle(Vector3.up, contact.normal);
        //         if (angle <= slopeLimit) {
        //             return true;
        //         }
        //     }
        //
        //     return false;
        // }

        private Vector3 _pos;
        private Vector3 _posThreshold;
        private float _dis;

        private readonly RaycastHit[] _hitBuffer = new RaycastHit[10];

        private bool CheckGroundCollision() {
            foreach (var collider in _collisionTracker.Collisions) {
                if (collider != _collider && collider != _characterController) {
                    return true;
                }
            }

            return false;

            // var startPosition = _characterController.transform.position + _characterController.center - Vector3.up *
            //     (_characterController.height / 2 - _characterController.radius);
            // var checkDistance = _characterController.radius /*+ _characterController.skinWidth * 3f*/;
            //
            // var tresholdPosition = startPosition -
            //                        Vector3.up * (Mathf.Cos(Mathf.Deg2Rad * _characterController.slopeLimit) *
            //                                      _characterController.radius);
            // _pos = startPosition/* + Vector3.down * _characterController.height / 2*/;
            // _posThreshold = tresholdPosition;
            // _dis = checkDistance;
            // var hitCount = Physics.SphereCastNonAlloc(startPosition, checkDistance, Vector3.down, _hitBuffer, _characterController.skinWidth * 3f);
            // for (var i = 0; i < hitCount; i++) {
            //     var hit = _hitBuffer[i];
            //     if (hit.collider != _collider && hit.collider != _characterController && hit.point.y <= tresholdPosition.y) {
            //         return true;
            //     }
            // }
            //
            // return false;

            // var checkPosition = _characterController.transform.position + _characterController.center - Vector3.up *
            //     (_characterController.height / 2 - _characterController.radius);

            // var mask = LayerMask.NameToLayer("Player");
            // return Physics.CheckSphere(checkPosition, checkDistance, mask);


            // return Physics.CheckSphere(checkPosition, checkDistance);

            // var slopeLimit = _characterController.slopeLimit;
            // for (var i = 0; i < collision.contactCount; i++) {
            //     var contact = collision.GetContact(i);
            //
            //     var angle = Vector3.Angle(Vector3.up, contact.normal);
            //     if (angle <= slopeLimit) {
            //         return true;
            //     }
            // }
            //
            // return false;
        }

        private void OnDrawGizmos() {
            Gizmos.color = Color.magenta.WithAlpha(0.5f);
            Gizmos.DrawSphere(_pos, _dis);

            Gizmos.color = Color.red;
            Gizmos.DrawSphere(_posThreshold, .2f);
        }

        // private void UpdateInteraction() {
        //     _playerInteractionHandler.Update();
        //
        //     if (Input.GetButtonDown("Submit"))
        //         _playerInteractionHandler.Interact();
        //     if (Input.GetButtonDown("Cancel"))
        //         _playerInteractionHandler.DropCurrent();
        // }
        //
        // public void Capture() {
        //     _onLose();
        // }
        //
        // public GeneralUtils.Event OnLose { get; }
        // public Vector3 Position {
        //     get => _playerGroundPosition.position;
        //     set => transform.position += value - _playerGroundPosition.position;
        // }
        //
        // private void OnDestroy() {
        //     _playerInteractionHandler.Dispose();
        // }
    }
}