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


        private readonly UpdatedValue<bool> _uiActive = new UpdatedValue<bool>();
        public IUpdatedValue<bool> UiActive => _uiActive;

        private UIElement[] _elements;

        private void Awake() {
            _elements = new UIElement[] { _mainMenu, _selectLevelMenu, _levelMenu };
            foreach (var element in _elements) {
                element.State.Subscribe(OnStateChange);
            }
        }

        private void OnStateChange(UIElement.EState _) {
            var active = _elements.Any(e =>
                e.State.Value == UIElement.EState.Showing || e.State.Value == UIElement.EState.Shown);
            _uiActive.Value = active;
        }
    }
}