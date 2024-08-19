using System;
using System.Collections.Generic;
using GeneralUtils;
using UnityEngine;

namespace _Game.Scripts.Interaction {
    public class Interactor : MonoBehaviour {
        [SerializeField] private float _distance;

        private readonly RaycastHit[] _hitBuffer = new RaycastHit[15];

        private Transform _origin;
        private HashSet<Collider> _ignoredColliders;

        private readonly HashSet<string> _items = new HashSet<string>();
        
        private Interactable _currentInteractable;
        private UpdatedValue<bool> _canInteract;

        public void Init(Transform origin, IEnumerable<Collider> ignoredColliders) {
            _origin = origin;
            _ignoredColliders = new HashSet<Collider>(ignoredColliders);
        }

        public void Pickup(Pickup item) {
            SoundController.Instance.PlaySound("key", 0.1f);
            _items.Add(item.Type);
        }

        private void Update() {
            if (Input.GetButtonDown("Interact") && _currentInteractable != null && CanInteract(_currentInteractable)) {
                Interact(_currentInteractable);
            }
        }

        public void LateUpdate() {
            var hitCount = Physics.RaycastNonAlloc(_origin.position, _origin.forward, _hitBuffer, _distance);

            var closestObjectDistance = float.MaxValue;
            Collider closestObject = null;

            for (var i = 0; i < hitCount; i++) {
                var hit = _hitBuffer[i];

                if (_ignoredColliders.Contains(hit.collider)) {
                    continue;
                }

                // var interactable = hit.collider.GetComponent<Interactable>();
                // if (interactable != null) {
                //     interactable
                //     _lastCollider = hit.collider;
                //     SetInteractable(interactable);
                //     return;
                // }

                if (!hit.collider.isTrigger) {
                    if (hit.distance < closestObjectDistance) {
                        closestObjectDistance = hit.distance;
                        closestObject = hit.collider;
                    }
                }
            }

            _lastCollider = closestObject;
            SetInteractable(closestObject != null ? closestObject.GetComponent<Interactable>() : null);
        }

        private void OnDestroy() {
            SetInteractable(null);
        }

        private Collider _lastCollider;
        private void OnDrawGizmos() {
            Gizmos.color = Color.magenta.WithAlpha(0.8f);
            Gizmos.DrawLine(_origin.position, _origin.position + _origin.forward * _distance);
            if (_lastCollider != null) {
                Gizmos.DrawCube(_lastCollider.bounds.center, _lastCollider.bounds.size);
            }
        }

        private void SetInteractable(Interactable interactable) {
            if (interactable == _currentInteractable) {
                return;
            }

            if (_currentInteractable != null) {
                _currentInteractable.SetSelected(false);
            }

            _currentInteractable = interactable;

            if (_currentInteractable != null) {
                _currentInteractable.SetSelected(true, CanInteract(_currentInteractable));
            }
        }

        private bool CanInteract(Interactable interactable) {
            switch (interactable) {
                case DoorConsole console:
                    return _items.Contains(console.Type);
                default:
                    throw new NotImplementedException();
            }
        }

        private void Interact(Interactable interactable) {
            interactable.Interact();

            switch (interactable) {
                case DoorConsole console:
                    _items.Remove(console.Type);
                    break;
                default:
                    throw new NotImplementedException();
            }

            interactable.SetSelected(true, CanInteract(interactable));
        }

        public void ReloadInTheSameLevel(Interactor previous) {
            foreach (var item in previous._items) {
                _items.Add(item);
            }
        }
    }
}