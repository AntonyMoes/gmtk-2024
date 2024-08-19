using _Game.Scripts.UI;
using GeneralUtils;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace _Game.Scripts {
    public class App : SingletonBehaviour<App> {
        [SerializeField] private CameraController _camera;
        [SerializeField] private PlayerController _playerPrefab;
        [SerializeField] private PhysicMaterial _levelMaterial;
        [SerializeField] private UIController _uiController;
        [SerializeField] private string _level;

        private PlayerController _player;
        private CheckpointController _currentCheckpoint;

        public static bool DevBuild => Application.isEditor || Debug.isDebugBuild;

        private void Awake() {
            DontDestroyOnLoad(gameObject);
        }

        private void Start() {
            SceneManager.LoadScene(_level, LoadSceneMode.Single);
        }

        public void InitLevel(LevelController controller) {
            controller.Init(_uiController, _camera, _playerPrefab, _levelMaterial);
        }
    }
}