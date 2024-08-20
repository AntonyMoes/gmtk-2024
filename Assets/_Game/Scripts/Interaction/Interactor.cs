using System;
using System.Collections.Generic;
using System.Linq;
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
        private Action _pickupPickaxe;

        public void Init(Transform origin, IEnumerable<Collider> ignoredColliders, Action pickupPickaxe) {
            _origin = origin;
            _ignoredColliders = new HashSet<Collider>(ignoredColliders);
            _pickupPickaxe = pickupPickaxe;
        }

        public void Pickup(PickaxePickup pickaxe) {
            SoundController.Instance.PlaySound("key", 0.1f);
            // TODO sound pickaxe
            _pickupPickaxe?.Invoke();
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

                if (!hit.collider.isTrigger) {
                    if (hit.distance < closestObjectDistance) {
                        closestObjectDistance = hit.distance;
                        closestObject = hit.collider;
                    }
                }
            }

            SetInteractable(closestObject != null ? closestObject.GetComponent<Interactable>() : null);
        }

        private void OnDestroy() {
            SetInteractable(null);
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

        public ReloadData GetReloadData() {
            return new ReloadData {
                Items = _items.ToArray()
            };
        }

        public void ReloadInTheSameLevel(ReloadData data) {
            foreach (var item in data.Items) {
                _items.Add(item);
            }
        }
    }
}