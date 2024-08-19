using System;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using UnityEngine;

namespace _Game.Scripts.Interaction {
    public class EndElevator : OnceEnterTrigger {
        [SerializeField] private Vector3 _targetPosition;
        [SerializeField] private float _time;
        [SerializeField] private GameObject _walls;

        private Tween _animation;

        protected override void OnOnceTrigger(Interactor interactor) {
            _walls.SetActive(true);
            _animation = transform.DOLocalMove(_targetPosition, _time).SetEase(Ease.InSine);
        }

        private void OnDestroy() {
            _animation?.Kill();
        }
    }
}