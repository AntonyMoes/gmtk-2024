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
        }
    }
}