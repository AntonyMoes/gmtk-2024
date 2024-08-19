using System;
using _Game.Scripts.UI;
using UnityEngine;

namespace _Game.Scripts {
    public class LevelController : MonoBehaviour {
        [SerializeField] private CheckpointController _checkpointController;

        private UIController _uiController;
        private CameraController _camera;
        private PlayerController _playerPrefab;

        private PlayerController _player;

        private void Start() {
            App.Instance.InitLevel(this);
        }

        public void Init(UIController uiController, CameraController camera, PlayerController playerPrefab, PhysicMaterial levelMaterial) {
            _uiController = uiController;
            _camera = camera;
            _playerPrefab = playerPrefab;

            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;

            LevelUtils.SetMaterial(transform, levelMaterial);

            StartGame();
        }

        private void SpawnPlayer(PlayerController previousPlayer = null) {
            var spawn = _checkpointController.CurrentCheckpoint != null ? _checkpointController.CurrentCheckpoint : _checkpointController.Spawn;
            _player = Instantiate(_playerPrefab, _checkpointController.Spawn);
            _player.transform.position = spawn.position;
            _player.transform.rotation = Quaternion.Euler(0, spawn.rotation.eulerAngles.y, 0);
            _player.Init(_camera, _uiController.DebugText, _uiController.StaminaProgressBar);

            if (previousPlayer != null) {
                _player.ReloadInTheSameLevel(previousPlayer);
            }
        }

        private PlayerController KillPlayer() {
            _camera.SetTarget(null);
            Destroy(_player.gameObject);
            var killedPlayer = _player;
            _player = null;
            return killedPlayer;
        }

        private void StartGame(PlayerController killedPlayer = null) {
            SoundController.Instance.PlayMusic("1_the_bottom", 0.5f);
            SpawnPlayer(killedPlayer);
        }

        private PlayerController EndGame() {
            SoundController.Instance.StopAllSounds();
            return KillPlayer();
        }

        private void RestartFromCheckpoint() {
            StartGame(EndGame());
        }

        private void Update() {
            if (Input.GetButtonDown("Restart")) {
                RestartFromCheckpoint();
            }

            if (Input.GetButtonDown("NoClip")) {
                if (_player != null) {
                    _player.ToggleNoClip();
                }
            }

            if (Input.GetButtonDown("Mute")) {
                if (_player != null) {
                    SoundController.Instance.ToggleMute();
                }
            }
        }
    }
}