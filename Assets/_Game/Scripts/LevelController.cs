using System;
using _Game.Scripts.UI;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace _Game.Scripts {
    public class LevelController : MonoBehaviour {
        [SerializeField] private CheckpointController _checkpointController;
        [SerializeField] private bool _canClimb;

        private UIController _uiController;
        private CameraController _camera;
        private PlayerController _playerPrefab;

        private PlayerController _player;

        private void Start() {
            if (App.Instance == null) {
                App.AutoStartLevel = SceneManager.GetActiveScene().name;
                SceneManager.LoadScene("App");
                return;
            }

            App.Instance.InitLevel(this);
        }

        public void Init(UIController uiController, CameraController camera, PlayerController playerPrefab, PhysicMaterial levelMaterial) {
            _uiController = uiController;
            _camera = camera;
            _playerPrefab = playerPrefab;

            LevelUtils.SetMaterial(transform, levelMaterial);

            StartGame();
        }

        private void SpawnPlayer(PlayerController previousPlayer = null) {
            var spawn = _checkpointController.CurrentCheckpoint != null ? _checkpointController.CurrentCheckpoint : _checkpointController.Spawn;
            _player = Instantiate(_playerPrefab, _checkpointController.Spawn);
            _player.transform.position = spawn.position;
            _player.transform.rotation = Quaternion.Euler(0, spawn.rotation.eulerAngles.y, 0);
            _player.Init(_camera, _uiController.DebugText, _uiController.StaminaProgressBar, _canClimb);

            if (previousPlayer != null) {
                _player.ReloadInTheSameLevel(previousPlayer);
            }
        }

        private void KillPlayer() {
            _camera.SetTarget(null);
            if (_player != null) {
                Destroy(_player.gameObject);
            }
        }

        private void StartGame(PlayerController killedPlayer = null) {
            _uiController.RestartScreen.Hide();
            SoundController.Instance.PlayMusic("1_the_bottom", 0.5f);
            SpawnPlayer(killedPlayer);
        }

        public void Kill() {
            _uiController.RestartScreen.Show();
            EndGame();
        }

        private void RestartFromCheckpoint() {
            EndGame();
            StartGame(_player);
        }

        private void EndGame() {
            SoundController.Instance.StopAllSounds();
            KillPlayer();
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