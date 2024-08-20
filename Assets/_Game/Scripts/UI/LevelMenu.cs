using System;
using GeneralUtils.UI;
using UnityEngine;
using UnityEngine.UI;

namespace _Game.Scripts.UI {
    public class LevelMenu : UIElement {
        [SerializeField] private Button _backToMenuButton;
        [SerializeField] private Button _closeButton;

        private Action _backToMenu;

        protected override void Init() {
            _backToMenuButton.onClick.AddListener(BackToMenu);
            _closeButton.onClick.AddListener(Close);
        }

        public void Setup(Action backToMenu) {
            _backToMenu = backToMenu;
        }

        private void BackToMenu() {
            Close();
            _backToMenu?.Invoke();
        }

        private void Close() {
            Hide();
        }

        public void Toggle() {
            switch (State.Value) {
                case EState.Showing:
                case EState.Hiding:
                    break;
                case EState.Shown:
                    Hide();
                    break;
                case EState.Hided:
                    Show();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}