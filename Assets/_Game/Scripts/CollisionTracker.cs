using System.Collections.Generic;
using UnityEngine;

namespace _Game.Scripts {
    public class CollisionTracker : MonoBehaviour {
        private readonly HashSet<Collider> _collisions = new HashSet<Collider>();
        public IReadOnlyCollection<Collider> Collisions => _collisions;
        
        private void OnTriggerEnter(Collider other) {
            // if (!other.TryGetComponent(out Trackable trackable))
            //     return;

            // trackable.SubscribeTracker(this);
            // _currentTrackables.Add(trackable);
            // _onTriggerEnter(trackable);

            _collisions.Add(other);
        }

        private void OnTriggerExit(Collider other) {
            // if (other.TryGetComponent(out Trackable trackable))
            //     ForceExit(trackable);
            _collisions.Remove(other);
        }
    }
}