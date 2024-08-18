using _Game.Scripts.UI;
using TMPro;
using UnityEngine;

namespace _Game.Scripts {
    public class App : MonoBehaviour {
        [SerializeField] private CameraController _camera;
        [SerializeField] private PlayerController _playerPrefab;
        [SerializeField] private Transform _levelRoot;
        [SerializeField] private PhysicMaterial _levelMaterial;
        [SerializeField] private TextMeshProUGUI _stateText;
        [SerializeField] private ProgressBar _staminaProgressBar;

        private PlayerController _player;
        private LevelController _currentLevel;

        private void Start() {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;

            LevelUtils.SetMaterial(_levelRoot, _levelMaterial);

            StartLevel(_levelRoot.GetComponentInChildren<LevelController>());
        }

        private void StartLevel(LevelController level) {
            _currentLevel = level;
            StartGame();
        }

        private void SpawnPlayer(PlayerController previousPlayer = null) {
            var spawn = _currentLevel.CurrentCheckpoint != null ? _currentLevel.CurrentCheckpoint : _currentLevel.Spawn;
            _player = Instantiate(_playerPrefab, _currentLevel.Spawn);
            _player.transform.position = spawn.position;
            _player.transform.rotation = Quaternion.Euler(0, spawn.rotation.eulerAngles.y, 0);
            _camera.VerticalRotation = 0;
            _player.Init(_camera, _stateText, _staminaProgressBar);

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