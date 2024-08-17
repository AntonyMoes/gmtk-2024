using System;
using System.Collections.Generic;
using UnityEngine;

namespace _Game.Scripts {
    public class CollisionTracker : MonoBehaviour {
        [SerializeField] private Collider _collider;

        private readonly HashSet<Collider> _ignored = new HashSet<Collider>();
        private readonly HashSet<Collider> _collisions = new HashSet<Collider>();
        public IReadOnlyCollection<Collider> Collisions => _collisions;

        public void Ignore(Collider ignored) {
            _ignored.Add(ignored);
            _collisions.Remove(ignored);
        }

        private void OnTriggerEnter(Collider other) {
            // if (!other.TryGetComponent(out Trackable trackable))
            //     return;

            // trackable.SubscribeTracker(this);
            // _currentTrackables.Add(trackable);
            // _onTriggerEnter(trackable);

            if (!_ignored.Contains(other)) {
                _collisions.Add(other);
            }
        }

        private Vector3 _p;
        private Vector3 _n;

        private void OnDrawGizmos() {
            Gizmos.color = Color.black;
            Gizmos.DrawSphere(_p, .2f);
            Gizmos.DrawLine(_p, _p + _n);
        }

        private readonly RaycastHit[] _hitBuffer = new RaycastHit[10];

        public Contact GetContact(Collider other) {
            if (!_collisions.Contains(other)) {
                throw new Exception($"{other.name} is not in collisions!");
            }

            var from = _collider.bounds.center;
            var point = other.ClosestPoint(from);
            point = point == from ? other.ClosestPointOnBounds(from) : point;
            var vector = point - from;
            var hitCount = Physics.RaycastNonAlloc(from, vector * 1.1f, _hitBuffer);
            for (var i = 0; i < hitCount; i++) {
                var hit = _hitBuffer[i];
                if (hit.collider != other) {
                    continue;
                }

                _p = hit.point;
                _n = hit.normal;

                return new Contact {
                    Point = hit.point,
                    Normal = hit.normal
                };
            }

            throw new Exception();
        }

        private void OnTriggerExit(Collider other) {
            // if (other.TryGetComponent(out Trackable trackable))
            //     ForceExit(trackable);
            _collisions.Remove(other);
        }
    }
}