using UnityEngine;

namespace _Game.Scripts {
    public class App : MonoBehaviour {
        [SerializeField] private Transform _playerSpawn;
        [SerializeField] private PlayerController _playerPrefab;
        [SerializeField] private Transform _levelRoot;
        [SerializeField] private PhysicMaterial _levelMaterial;

        private void Start() {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;

            var player = Instantiate(_playerPrefab, _playerSpawn);
            LevelUtils.SetMaterial(_levelRoot, _levelMaterial);
        }
    }
}