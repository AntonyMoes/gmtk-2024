using TMPro;
using UnityEngine;

namespace _Game.Scripts {
    public class App : MonoBehaviour {
        [SerializeField] private Transform _playerSpawn;
        [SerializeField] private PlayerController _playerPrefab;
        [SerializeField] private Transform _levelRoot;
        [SerializeField] private PhysicMaterial _levelMaterial;
        [SerializeField] private TextMeshProUGUI _stateText;

        private void Start() {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;

            var player = Instantiate(_playerPrefab, _playerSpawn);
            player.Init(_stateText);
            LevelUtils.SetMaterial(_levelRoot, _levelMaterial);
        }
    }
}