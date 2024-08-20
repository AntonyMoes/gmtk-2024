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
        private int _currentLevelIndex;
        private bool _showStartCutscene;

        public static bool DevBuild => Application.isEditor || Debug.isDebugBuild;

        [CanBeNull] public static string AutoStartLevel = null;

        private void Awake() {
            DontDestroyOnLoad(gameObject);

            _uiController.UiActive.Subscribe(SetupCursor, true);
            _uiController.MainMenu.Setup(StartLevel, OpenSelectLevel);
            _uiController.SelectLevelMenu.Setup(_levels, StartLevelFromMenu, CloseSelectLevel);
            _uiController.LevelMenu.Setup(() => FinishLevel(false));
        }

        private void Start() {
            if (AutoStartLevel != null) {
                var level = AutoStartLevel;
                AutoStartLevel = null;
                StartLevelFromMenu(level);
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
            _showStartCutscene = true;
            StartLevelFromMenu(_levels[0]);
        }

        private void StartLevelFromMenu(string level) {
            _uiController.LoadingScreen.ShowInstant();
            StartLevel(level);
        }
        
        private void StartLevel(string level) {
            _currentLevelIndex = _levels.IndexOf(level);
            _uiController.MainMenu.Hide();
            _uiController.SelectLevelMenu.Hide();
            SceneManager.LoadScene(level, LoadSceneMode.Single);
        }

        public void InitLevel(LevelController controller) {
            _currentLevel = controller;
            controller.Init(_uiController, _camera, _playerPrefab, _levelMaterial);
            _uiController.LoadingScreen.TriggerHide();

            if (_showStartCutscene) {
                _showStartCutscene = false;
                _uiController.StartUICutscene.Show();
            }
        }

        public void FinishLevel(bool complete) {
            _currentLevel.Deactivate();
            _currentLevel = null;

            if (!complete) {
                Start();
                return;
            }

            var lastCompleted = SaveManager.GetInt(SaveManager.IntData.LastCompletedLevel, -1);
            if (lastCompleted == -1 || _currentLevelIndex > lastCompleted) {
                SaveManager.SetInt(SaveManager.IntData.LastCompletedLevel, _currentLevelIndex);
            }

            if (_currentLevelIndex < _levels.Length - 1) {
                _uiController.LoadingScreen.Show(() => {
                    StartLevel(_levels[_currentLevelIndex + 1]);
                });
            } else {
                // TODO FINISH GAME
                Start();
            }
        }

        public void Kill() {
            _currentLevel.Kill();
        }

        public void StartEndCutscene() {
            _uiController.EndUICutscene.Show();
        }
    }
}