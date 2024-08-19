using DG.Tweening;
using UnityEngine;

namespace _Game.Scripts.Interaction {
    public class CanvasFadeTrigger : Trigger {
        [SerializeField] private CanvasGroup[] _groups;
        [SerializeField] private float _duration = 0.4f;

        private Tween _animation;

        private void Awake() {
            foreach (var group in _groups) {
                group.alpha = 0f;
            }
        }

        protected override void OnTrigger(bool enter, Interactor interactor) {
            _animation?.Kill();

            var targetAlpha = enter ? 1f : 0f;
            var sequence = DOTween.Sequence();
            foreach (var group in _groups) {
                sequence = sequence.Insert(0, group.DOFade(targetAlpha, _duration));
            }

            _animation = sequence.OnComplete(() => _animation = null);
        }

        private void OnDestroy() {
            _animation?.Kill();
        }
    }
}