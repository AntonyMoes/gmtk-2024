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
        private ReloadData _reloadData;

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

        private void SpawnPlayer(ReloadData reloadData = null) {
            var spawn = _checkpointController.CurrentCheckpoint != null ? _checkpointController.CurrentCheckpoint : _checkpointController.Spawn;
            _player = Instantiate(_playerPrefab, _checkpointController.Spawn);
            _player.transform.position = spawn.position;
            _player.transform.rotation = Quaternion.Euler(0, spawn.rotation.eulerAngles.y, 0);
            _player.Init(_camera, _uiController.DebugText, _uiController.StaminaProgressBar, _canClimb);

            if (reloadData != null) {
                _player.ReloadInTheSameLevel(reloadData);
            }
        }

        private void KillPlayer() {
            if (_player == null) {
                return;
            }

            _camera.SetTarget(null);

            _reloadData = _player.GetReloadData();
            Destroy(_player.gameObject);
            _player = null;
        }

        private void StartGame(ReloadData reloadData = null) {
            _uiController.RestartScreen.Hide();
            SoundController.Instance.PlayMusic("1_the_bottom", 0.5f);
            SpawnPlayer(reloadData);
        }

        public void Kill() {
            _uiController.RestartScreen.Show();
            EndGame();
        }

        private void RestartFromCheckpoint() {
            EndGame();
            StartGame(_reloadData);
            _reloadData = null;
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