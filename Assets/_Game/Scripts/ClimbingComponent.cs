using System;
using System.Collections.Generic;
using System.Linq;
using GeneralUtils;
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
        [SerializeField] private Transform _climbRightOrigin;
        [SerializeField] private Transform _climbUpOrigin;
        [SerializeField] private float _climbDirectionDistance;

        [Header("Stamina")]
        [SerializeField] private float _stamina;
        [SerializeField] private float _latchOnStamina;
        [SerializeField] private float _staminaPerSecond;

        private readonly RaycastHit[] _hitBuffer = new RaycastHit[15];

        private HashSet<Collider> _ignoredColliders = new HashSet<Collider>();
        public Contact LatchOnContact { get; set; }
        public Contact ClimbContact { get; set; }

        public bool CantClimb => Stamina <= 0 || ClimbContact == null;

        public float MaxStamina => _stamina;
        public float Stamina { get; set; }

        // public Contact TurnLeftContact { get; set; }

        public void Init(IEnumerable<Collider> ignoredColliders) {
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
            var angle = Vector3.Angle(Vector3.up, hit.HitNormal);
            var appropriateAngle = angle >= _minClimbingAngle && angle <= _maxClimbingAngle;
            var contact = hit.Hit && appropriateAngle
                ? new Contact {
                    Point = hit.HitPoint,
                    Normal = hit.HitNormal,
                }
                : null;

            if (ClimbContact != null) {
                if (ClimbContact != null && !hit.Hit) {
                    _lastFailedClimbing = hit;
                }
                ClimbContact = contact;
            } else if (contact != null) {
                var (position, direction) = GetLatchPositionAndDirection(contact);
                var horizontalShift = Quaternion.Euler(Vector3.up * 90) * direction * _minClimbingWidth / 2f;
                var rightHit = PerformCast(position + horizontalShift, direction, _climbStraightDistance);
                var leftHit = PerformCast(position - horizontalShift, direction, _climbStraightDistance);
                
                if (leftHit.Hit && rightHit.Hit) {
                    LatchOnContact = contact;
                } else {
                    LatchOnContact = null;
                }
            } else {
                LatchOnContact = null;
            }

            var hits = Enumerable.Empty<(Transform, float)>()
                .Append((_climbStraightOrigin, _climbStraightDistance))
                // .Append((_climbLeftOrigin, _climbDirectionDistance))
                // .Append((_climbRightOrigin, _climbDirectionDistance))
                // .Append((_climbUpOrigin, _climbDirectionDistance))
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

        public (Vector3, Vector3) GetLatchPositionAndDirection(Contact contact) {
            var horizontalNormal = contact.Normal.With(y: 0f);
            var position = contact.Point + horizontalNormal;
            var direction = -horizontalNormal.normalized;
            return (position, direction);
        }

        private RayCastResult PerformCast(Transform origin, float distance) {
            return PerformCast(origin.position, origin.forward, distance);
        }

        private RayCastResult PerformCast(Vector3 origin, Vector3 direction,  float distance) {
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
        }
    }
}