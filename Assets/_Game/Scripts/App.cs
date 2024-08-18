using System;
using System.Security.Cryptography;
using TMPro;
using UnityEngine;

namespace _Game.Scripts {
    public class App : MonoBehaviour {
        [SerializeField] private Transform _playerSpawn;
        [SerializeField] private CameraController _camera;
        [SerializeField] private PlayerController _playerPrefab;
        [SerializeField] private Transform _levelRoot;
        [SerializeField] private PhysicMaterial _levelMaterial;
        [SerializeField] private TextMeshProUGUI _stateText;

        private PlayerController _player;

        private void Start() {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;

            LevelUtils.SetMaterial(_levelRoot, _levelMaterial);

            StartGame();
        }

        private void StartGame() {
            _player = Instantiate(_playerPrefab, _playerSpawn);
            _player.Init(_camera, _stateText);
            SoundController.Instance.PlayMusic("1 - The Bottom", 0.5f);
        }

        private void EndGame() {
            Destroy(_player.gameObject);
            _player = null;
        }

        private void Update() {
            if (Input.GetButtonDown("Restart")) {
                EndGame();
                StartGame();
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