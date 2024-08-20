using System;
using DG.Tweening;
using GeneralUtils.UI;
using UnityEngine;

namespace _Game.Scripts.UI {
    public class StartScreen : UIElement {
        [SerializeField] private CanvasGroup _mainGroup;
        
        protected override void PerformShow(Action onDone = null) {
            _mainGroup.alpha = 1f;
            DOTween.Sequence()
                .AppendInterval(4f)
                .Append(_mainGroup.DOFade(0f, 1f))
                .OnComplete(() => {
                    onDone?.Invoke();
                    Hide();
                });
        }
    }
}