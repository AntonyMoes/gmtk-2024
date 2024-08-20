using System;
using DG.Tweening;
using GeneralUtils.UI;
using UnityEngine;
using UnityEngine.UI;

namespace _Game.Scripts.UI {
    public class MainMenu : UIElement {
        [SerializeField] private CanvasGroup _contentGroup;
        [SerializeField] private Button _start;
        [SerializeField] private Button _selectLevel;

        private Action _onStart;
        private Action _onSelectLevel;

        protected override void Init() {
            _start.onClick.AddListener(OnStartClick);
            _selectLevel.onClick.AddListener(OnSelectLevelClick);
        }

        public void Setup(Action onStart, Action onSelectLevel) {
            _onStart = onStart;
            _onSelectLevel = onSelectLevel;
        }

        private void OnStartClick() {
            _onStart?.Invoke();
        }

        private void OnSelectLevelClick() {
            _onSelectLevel?.Invoke();
        }

        // protected override void PerformShow(Action onDone = null) {
        //     _contentGroup.alpha = 0f;
        //     _contentGroup.DOFade(1f, 0.4f).OnComplete(() => onDone?.Invoke());
        // }
    }
}