using System;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using UnityEngine;

namespace _Game.Scripts.Interaction {
    public class EndElevator : Trigger {
        [SerializeField] private Vector3 _targetPosition;
        [SerializeField] private float _time;
        [SerializeField] private GameObject _walls;

        private bool _activated;
        private Tween _animation;

        protected override void OnTrigger(Interactor interactor) {
            if (_activated) {
                return;
            }

            _activated = true;

            _walls.SetActive(true);
            _animation = transform.DOLocalMove(_targetPosition, _time).SetEase(Ease.InSine);
        }

        private void OnDestroy() {
            _animation?.Kill();
        }
    }
}