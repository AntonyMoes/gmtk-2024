using System;
using System.Linq;
using GeneralUtils;
using GeneralUtils.UI;
using TMPro;
using UnityEngine;

namespace _Game.Scripts.UI {
    public class UIController : MonoBehaviour {
        [SerializeField] private TextMeshProUGUI _debugText;
        public TextMeshProUGUI DebugText => _debugText;

        [SerializeField] private GameObject _debug;
        [SerializeField] private GameObject _help;
        
        [SerializeField] private ProgressBar _staminaProgressBar;
        public ProgressBar StaminaProgressBar => _staminaProgressBar;

        [SerializeField] private MainMenu _mainMenu;
        public MainMenu MainMenu => _mainMenu;

        [SerializeField] private LoadingScreen _loadingScreen;
        public LoadingScreen LoadingScreen => _loadingScreen;

        [SerializeField] private SelectLevelMenu _selectLevelMenu;
        public SelectLevelMenu SelectLevelMenu => _selectLevelMenu;

        [SerializeField] private LevelMenu _levelMenu;
        public LevelMenu LevelMenu => _levelMenu;

        [SerializeField] private UIElement _restartScreen;
        public UIElement RestartScreen => _restartScreen;

        [SerializeField] private StartUICutscene _startUICutscene;
        public StartUICutscene StartUICutscene => _startUICutscene;


        private readonly UpdatedValue<bool> _uiActive = new UpdatedValue<bool>();
        public IUpdatedValue<bool> UiActive => _uiActive;


        private readonly UpdatedValue<bool> _uiNoLookingActive = new UpdatedValue<bool>();
        public IUpdatedValue<bool> UiNoLookingActive => _uiNoLookingActive;


        private readonly UpdatedValue<bool> _uiNoLevelMenuActive = new UpdatedValue<bool>();
        public IUpdatedValue<bool> UiNoLevelMenuActive => _uiNoLevelMenuActive;

        private UIElement[] _elements;
        private UIElement[] _elementsNoLooking;
        private UIElement[] _elementsNoLevelMenu;

        private void Awake() {
            _elements = new UIElement[] { _mainMenu, _selectLevelMenu, _levelMenu };
            foreach (var element in _elements) {
                element.State.Subscribe(OnStateChange);
            }

            _elementsNoLooking = new UIElement[] { _levelMenu, _loadingScreen, _startUICutscene };
            foreach (var element in _elementsNoLooking) {
                element.State.Subscribe(OnStateChangeNoLooking);
            }

            _elementsNoLevelMenu = new UIElement[] { _loadingScreen, _startUICutscene, _restartScreen };
            foreach (var element in _elementsNoLevelMenu) {
                element.State.Subscribe(OnStateChangeNoLevelMenu);
            }

            var debugUI = new[] { _debug, _help };
            foreach (var ui in debugUI) {
                ui.SetActive(App.DevBuild);
            }
        }

        private void OnStateChange(UIElement.EState _) {
            var active = _elements.Any(e =>
                e.State.Value == UIElement.EState.Showing || e.State.Value == UIElement.EState.Shown);
            _uiActive.Value = active;
        }

        private void OnStateChangeNoLooking(UIElement.EState _) {
            var active = _elementsNoLooking.Any(e =>
                e.State.Value == UIElement.EState.Showing || e.State.Value == UIElement.EState.Shown);
            _uiNoLookingActive.Value = active;
        }

        private void OnStateChangeNoLevelMenu(UIElement.EState _) {
            var active = _elementsNoLevelMenu.Any(e =>
                e.State.Value == UIElement.EState.Showing || e.State.Value == UIElement.EState.Shown);
            _uiNoLevelMenuActive.Value = active;
        }
    }
}