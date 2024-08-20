using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;

namespace _Game.Scripts {
    public class CollisionTracker : MonoBehaviour {
        [SerializeField] private Collider _collider;
        [SerializeField] private Color _gizmoColor = Color.black;

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

            if (!other.isTrigger && !_ignored.Contains(other)) {
                _collisions.Add(other);
            }
        }

        private readonly List<Contact> _lastContacts = new List<Contact>();
        private readonly List<Contact> _contacts = new List<Contact>();
        private void OnDrawGizmos() {
            Gizmos.color = _gizmoColor;

            if (_contacts.Count > 0) {
                _lastContacts.Clear();
                _lastContacts.AddRange(_contacts);
                _contacts.Clear();
            }

            foreach (var contact in _lastContacts) {
                Gizmos.DrawSphere(contact.Point, .2f);
                Gizmos.DrawLine(contact.Point, contact.Point + contact.Normal);
            }
        }

        private readonly RaycastHit[] _hitBuffer = new RaycastHit[10];

        [CanBeNull]
        public Contact GetContact(Collider other) {
            if (!_collisions.Contains(other)) {
                return null;
                // throw new Exception($"{other.name} is not in collisions!");
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

                var contact = new Contact {
                    Point = hit.point,
                    Normal = hit.normal
                };
                _contacts.Add(contact);
                return contact;
            }

            return null;
        }

        private void OnTriggerExit(Collider other) {
            // if (other.TryGetComponent(out Trackable trackable))
            //     ForceExit(trackable);
            _collisions.Remove(other);
        }
    }
}