using System;
using GeneralUtils;
using UnityEngine;

namespace _Game.Scripts {
    public class Checkpoint : MonoBehaviour {
        [SerializeField] private Transform _spawn;
        public Transform Spawn => _spawn;

        private readonly Event<Checkpoint> _onEnter = new Event<Checkpoint>();
        public IEvent<Checkpoint> OnEnter => _onEnter;

        private void OnTriggerEnter(Collider other) {
            if (other.GetComponent<PlayerController>() != null) {
                _onEnter?.Invoke(this);
            }
        }
    }
}