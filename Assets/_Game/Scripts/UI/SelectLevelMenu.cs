using System;
using System.Collections.Generic;
using GeneralUtils.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace _Game.Scripts.UI {
    public class SelectLevelMenu : UIElement {
        [SerializeField] private LevelButton _levelButtonPrefab;
        [SerializeField] private Transform _levelButtonParent;
        [SerializeField] private Button _backButton;

        private readonly List<LevelButton> _buttons = new List<LevelButton>();
        private string[] _levels;
        private Action<string> _startLevel;
        private Action _onBack;

        protected override void Init() {
            _backButton.onClick.AddListener(OnBackClick);
        }

        public void Setup(string[] levels, Action<string> startLevel, Action onBack) {
            _levels = levels;
            _startLevel = startLevel;
            _onBack = onBack;
        }

        protected override void PerformShow(Action onDone = null) {
            var lastCompletedLevel = SaveManager.GetInt(SaveManager.IntData.LastCompletedLevel, -1);
            for (var i = 0; i < _levels.Length; i++) {
                var level = _levels[i];
                var button = Instantiate(_levelButtonPrefab, _levelButtonParent);
                button.Load(i, i <= lastCompletedLevel + 1, StartLevel);
                _buttons.Add(button);
            }

            base.PerformShow(onDone);
        }

        private void StartLevel(int i) {
            _startLevel(_levels[i]);
        }

        public override void Clear() {
            foreach (var button in _buttons) {
                Destroy(button.gameObject);
            }

            _buttons.Clear();
        }

        private void OnBackClick() {
            _onBack?.Invoke();
        }
    }
}