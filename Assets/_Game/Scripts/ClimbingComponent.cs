using System;
using System.Collections.Generic;
using System.Linq;
using GeneralUtils;
using JetBrains.Annotations;
using UnityEngine;

namespace _Game.Scripts {
    public class ClimbingComponent : MonoBehaviour {
        [Header("Climbing")]
        [SerializeField] private float _minClimbingAngle;
        [SerializeField] private float _maxClimbingAngle;
        [SerializeField] private float _minClimbingWidth;
        [SerializeField] private Transform _climbStraightOrigin;
        [SerializeField] private float _climbStraightDistance;
        [SerializeField] private Transform _climbLeftOrigin;
        [SerializeField] private Transform _climbLeftOutOrigin;
        [SerializeField] private Transform _climbRightOrigin;
        [SerializeField] private Transform _climbRightOutOrigin;
        [SerializeField] private Transform _climbUpOrigin;
        [SerializeField] private float _outClimbDirectionDistance;
        [SerializeField] private float _climbDirectionDistance;

        [Header("Stamina")]
        [SerializeField] private float _stamina;
        [SerializeField] private float _latchOnStamina;
        [SerializeField] private float _staminaPerSecond;

        private readonly RaycastHit[] _hitBuffer = new RaycastHit[15];

        private HashSet<Collider> _ignoredColliders = new HashSet<Collider>();
        private Func<float> _maxSlopeAngle;

        public Contact LatchOnContact { get; set; }
        public Contact ClimbContact { get; set; }

        public bool CantClimb => Stamina <= 0 || ClimbContact == null;

        public float MaxStamina => _stamina;
        public float Stamina { get; set; }

        private Vector2 _lastInput;

        public void Init(Func<float> maxSlopeAngle, IEnumerable<Collider> ignoredColliders) {
            _maxSlopeAngle = maxSlopeAngle;
            _ignoredColliders = new HashSet<Collider>(ignoredColliders);
            Land();
        }

        public bool LatchOn() {
            if (Stamina < _latchOnStamina) {
                return false;
            }

            Stamina -= _latchOnStamina;
            ClimbContact = LatchOnContact;
            LatchOnContact = null;
            return true;
        }

        public void Move(Vector2 input, Vector3 movement, float deltaTime) {
            _lastInput = input;

            if (movement == Vector3.zero) {
                return;
            }

            Stamina -= deltaTime * _staminaPerSecond;
        }

        public void LatchOff() {
            ClimbContact = null;
        }

        public void Land() {
            Stamina = MaxStamina;
        }

        private void Update() {
            var hit = PerformCast(_climbStraightOrigin, _climbStraightDistance);
            var contact = TryGetContactClimbFromHit(hit);

            if (ClimbContact != null) {
                if (ClimbContact != null && !hit.Hit) {
                    _lastFailedClimbing = hit;
                }

                SetClimbContact(contact);
            } else {
                SetLatchContact(contact);
            }

            var hits = Enumerable.Empty<(Transform, float)>()
                .Append((_climbStraightOrigin, _climbStraightDistance))
                .Append((_climbLeftOrigin, _climbDirectionDistance))
                .Append((_climbLeftOutOrigin, _outClimbDirectionDistance))
                .Append((_climbRightOrigin, _climbDirectionDistance))
                .Append((_climbRightOutOrigin, _outClimbDirectionDistance))
                .Append((_climbUpOrigin, _climbDirectionDistance))
                .Select(pair => PerformCast(pair.Item1, pair.Item2))
                .ToArray();
            _casts.Clear();
            _casts.AddRange(hits);

            if (contact != null) {
                var (position, direction) = GetLatchPositionAndDirection(contact);
                var horizontalShift = Quaternion.Euler(Vector3.up * 90) * direction * _minClimbingWidth / 2f;
                var rightHit = PerformCast(position + horizontalShift, direction, _climbStraightDistance);
                var leftHit = PerformCast(position - horizontalShift, direction, _climbStraightDistance);
                _casts.Add(rightHit);
                _casts.Add(leftHit);
            }
        }

        private static (Vector3, Vector3) GetLatchPositionAndDirection(Contact contact) {
            var horizontalNormal = contact.Normal.With(y: 0f);
            var position = contact.Point + horizontalNormal;
            var direction = -horizontalNormal.normalized;
            return (position, direction);
        }

        public (Vector3, Vector3?) GetLatchPositionAndPossibleDirection(Contact contact) {
            var (position, direction) = GetLatchPositionAndDirection(contact);
            if (Vector3.Angle(contact.Normal, Vector3.up) < _maxSlopeAngle()) {
                return (position + Vector3.up, null);
            }

            return (position, direction);
        }

        private bool CheckContactWidth(Contact contact) {
            var (position, direction) = GetLatchPositionAndDirection(contact);
            var horizontalShift = Quaternion.Euler(Vector3.up * 90) * direction * _minClimbingWidth / 2f;
            var rightHit = PerformCast(position + horizontalShift, direction, _climbStraightDistance);
            var leftHit = PerformCast(position - horizontalShift, direction, _climbStraightDistance);
            return leftHit.Hit && rightHit.Hit;
        }

        [CanBeNull]
        private Contact TryGetContactClimbFromHit(RayCastResult hit) {
            return TryGetContactFromHit(hit, _maxClimbingAngle, _minClimbingAngle);
        }

        [CanBeNull]
        private static Contact TryGetContactFromHit(RayCastResult hit, float maxAngle, float? minAngle = null) {
            var angle = Vector3.Angle(Vector3.up, hit.HitNormal);
            var appropriateAngle = angle <= maxAngle && (!(minAngle is { } ma) || angle >= ma);
            return hit.Hit && appropriateAngle
                ? hit.Contact
                : null;
        }

        private void SetClimbContact(Contact contact) {
            if (contact != null) {
                var outOrigin = _lastInput.x > 0 ? _climbRightOutOrigin : _climbLeftOutOrigin;
                var outHit = PerformCast(outOrigin, _outClimbDirectionDistance);
                var outContact = TryGetContactClimbFromHit(outHit);
                if (outContact != null && CheckContactWidth(outContact)) {
                    ClimbContact = outContact;
                    return;
                }

                ClimbContact = contact;
                return;
            }

            if (ClimbContact == null) {
                return;
            }

            if (_lastInput.y > 0) {
                var hit = PerformCast(_climbUpOrigin, _climbDirectionDistance);
                var upContact = TryGetContactFromHit(hit, _maxSlopeAngle());
                if (upContact != null) {
                    ClimbContact = upContact;
                    return;
                }
            }

            if (_lastInput.x != 0) {
                var origin = _lastInput.x > 0 ? _climbRightOrigin : _climbLeftOrigin;
                var hit = PerformCast(origin, _climbDirectionDistance);
                var sideContact = TryGetContactClimbFromHit(hit);
                if (sideContact != null && CheckContactWidth(sideContact)) {
                    ClimbContact = sideContact;
                    return;
                }
            }

            ClimbContact = null;
        }

        private void SetLatchContact(Contact contact) {
            if (contact == null) {
                LatchOnContact = null;
                return;
            }

            if (CheckContactWidth(contact)) {
                LatchOnContact = contact;
            } else {
                LatchOnContact = null;
            }
        }

        private RayCastResult PerformCast(Transform origin, float distance) {
            return PerformCast(origin.position, origin.forward, distance);
        }

        private RayCastResult PerformCast(Vector3 origin, Vector3 direction, float distance) {
            var hitCount = Physics.RaycastNonAlloc(origin, direction, _hitBuffer, distance);

            var closestObjectDistance = float.MaxValue;
            RaycastHit closestObject = default;

            for (var i = 0; i < hitCount; i++) {
                var hit = _hitBuffer[i];

                if (_ignoredColliders.Contains(hit.collider)) {
                    continue;
                }

                if (!hit.collider.isTrigger) {
                    if (hit.distance < closestObjectDistance) {
                        closestObjectDistance = hit.distance;
                        closestObject = hit;
                    }
                }
            }

            return new RayCastResult {
                Origin = origin,
                Vector = direction * distance,
                Hit = closestObject.collider != null,
                HitPoint = closestObject.point,
                HitNormal = closestObject.normal
            };
        }


        private readonly List<RayCastResult> _casts = new List<RayCastResult>();
        private RayCastResult _lastFailedClimbing;

        private void OnDrawGizmos() {
            foreach (var cast in _casts) {
                Gizmos.color = cast.Hit ? Color.green : Color.red;
                Gizmos.DrawLine(cast.Origin, cast.Origin + cast.Vector);
                if (cast.Hit) {
                    Gizmos.color = Color.blue;
                    Gizmos.DrawSphere(cast.HitPoint, .2f);
                    Gizmos.DrawLine(cast.HitPoint, cast.HitPoint + cast.HitNormal);
                }
            }

            if (_lastFailedClimbing != null) {
                Gizmos.color = Color.yellow;
                Gizmos.DrawLine(_lastFailedClimbing.Origin, _lastFailedClimbing.Origin + _lastFailedClimbing.Vector);
            }
        }

        private class RayCastResult {
            public Vector3 Origin;
            public Vector3 Vector;
            public bool Hit;
            public Vector3 HitPoint;
            public Vector3 HitNormal;

            public Contact Contact => new Contact {
                Point = HitPoint,
                Normal = HitNormal,
            };
        }
    }
}