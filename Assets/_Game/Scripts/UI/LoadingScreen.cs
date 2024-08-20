using System;
using DG.Tweening;
using GeneralUtils.UI;
using UnityEngine;

namespace _Game.Scripts.UI {
    public class LoadingScreen : UIElement {
        [SerializeField] private CanvasGroup _group;

        private const float AnimationTime = 0.5f;

        private bool _instant;

        public void ShowInstant() {
            _instant = true;
            Show();
        }

        public void TriggerHide() {
            switch (State.Value) {
                case EState.Showing:
                case EState.Shown:
                    State.WaitFor(EState.Shown, () => Hide());
                    break;
                case EState.Hiding:
                case EState.Hided:
                    return;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        
        protected override void PerformShow(Action onDone = null) {
            var time = _instant ? 0f : AnimationTime;
            _instant = false;
            _group.DOFade(1f, time).OnComplete(() => onDone?.Invoke());
        }

        protected override void PerformHide(Action onDone = null) {
            var time = _instant ? 0f : AnimationTime;
            _instant = false;
            _group.DOFade(0f, time).OnComplete(() => onDone?.Invoke());
        }
    }
}