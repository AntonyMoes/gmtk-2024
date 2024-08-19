using _Game.Scripts.UI;
using GeneralUtils;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace _Game.Scripts {
    public class App : SingletonBehaviour<App> {
        [SerializeField] private CameraController _camera;
        [SerializeField] private PlayerController _playerPrefab;
        [SerializeField] private PhysicMaterial _levelMaterial;
        [SerializeField] private UIController _uiController;
        [SerializeField] private string[] _levels;

        private LevelController _currentLevel;

        public static bool DevBuild => Application.isEditor || Debug.isDebugBuild;

        [CanBeNull] public static string AutoStartLevel= null;

        private void Awake() {
            DontDestroyOnLoad(gameObject);

            _uiController.UiActive.Subscribe(SetupCursor, true);
            _uiController.MainMenu.Setup(StartLevel, OpenSelectLevel);
            _uiController.SelectLevelMenu.Setup(_levels, StartLevel, CloseSelectLevel);
        }

        private void Start() {
            if (AutoStartLevel != null) {
                var level = AutoStartLevel;
                AutoStartLevel = null;
                StartLevel(level);
                return;
            }

            _uiController.MainMenu.Show();
        }

        private void SetupCursor(bool active) {
            Cursor.visible = active;
            Cursor.lockState = active ? CursorLockMode.None : CursorLockMode.Locked;
        }

        private void OpenSelectLevel() {
            _uiController.SelectLevelMenu.Show();
            _uiController.MainMenu.Hide();
        }

        private void CloseSelectLevel() {
            _uiController.MainMenu.Show();
            _uiController.SelectLevelMenu.Hide();
        }

        private void StartLevel() {
            StartLevel(_levels[0]);
        }

        private void StartLevel(string level) {
            _uiController.LoadingScreen.Show();
            _uiController.MainMenu.Hide();
            _uiController.SelectLevelMenu.Hide();
            SceneManager.LoadScene(level, LoadSceneMode.Single);
        }

        public void InitLevel(LevelController controller) {
            _currentLevel = controller;
            controller.Init(_uiController, _camera, _playerPrefab, _levelMaterial);
            _uiController.LoadingScreen.Hide();
        }

        public void FinishLevel() {
            _currentLevel = null;
            Start();
        }

        public void Kill() {
            _currentLevel.Kill();
        }
    }
}