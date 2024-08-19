using GeneralUtils;
using UnityEngine;

namespace _Game.Scripts {
    public class CheckpointController : MonoBehaviour {
        [SerializeField] private Transform _playerSpawn;
        [SerializeField] private Checkpoint[] _checkpoints;

        public Transform Spawn => _playerSpawn;

        private Checkpoint _currentCheckpoint;
        public Transform CurrentCheckpoint => _currentCheckpoint != null ? _currentCheckpoint.Spawn : null;

        public void Reset() {
            _currentCheckpoint = null;
        }

        private void Awake() {
            foreach (var checkpoint in _checkpoints) {
                checkpoint.OnEnter.Subscribe(OnCheckpointEnter);
            }
        }

        private void OnCheckpointEnter(Checkpoint checkpoint) {
            checkpoint.OnEnter.Unsubscribe(OnCheckpointEnter);

            if (_currentCheckpoint == null ||
                _checkpoints.IndexOf(_currentCheckpoint) < _checkpoints.IndexOf(checkpoint)) {
                _currentCheckpoint = checkpoint;
            }            
        }

        private void OnDestroy() {
            foreach (var checkpoint in _checkpoints) {
                checkpoint.OnEnter.Unsubscribe(OnCheckpointEnter);
            }
        }
    }
}