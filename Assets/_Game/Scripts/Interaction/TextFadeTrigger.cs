using System;
using DG.Tweening;
using TMPro;
using UnityEngine;

namespace _Game.Scripts.Interaction {
    public class TextFadeTrigger : Trigger {
        [SerializeField] private TextMeshProUGUI _text;
        [SerializeField] private float _duration = 0.4f;

        private Tween _animation;

        private void Awake() {
            _text.alpha = 0f;
        }

        protected override void OnTrigger(bool enter, Interactor interactor) {
            _animation?.Kill();

            var targetAlpha = enter ? 1f : 0f;
            _animation = _text.DOFade(targetAlpha, _duration).OnComplete(() => _animation = null);
        }

        private void OnDestroy() {
            _animation?.Kill();
        }
    }
}