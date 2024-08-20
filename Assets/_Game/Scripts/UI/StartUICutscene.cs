using System;
using DG.Tweening;
using GeneralUtils.UI;
using UnityEngine;

namespace _Game.Scripts.UI {
    public class StartUICutscene : UIElement {
        [SerializeField] private CanvasGroup _mainGroup;
        [SerializeField] private CanvasGroup _contentGroup;

        protected override void PerformShow(Action onDone = null) {
            _mainGroup.alpha = 1f;
            _contentGroup.alpha = 0f;
            DOTween.Sequence()
                .AppendInterval(.6f)
                .Append(_contentGroup.DOFade(1f, 1f).OnComplete(() => {
                    onDone?.Invoke();
                    Animate();
                }));
        }

        private void Animate() {
            DOVirtual.DelayedCall(5f, () => Hide());
        }

        protected override void PerformHide(Action onDone = null) {
            _mainGroup.alpha = 1f;
            _contentGroup.alpha = 1f;
            _mainGroup.DOFade(0f, 1f).OnComplete(() => onDone?.Invoke());
        }
    }
}