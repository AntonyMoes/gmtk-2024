using System;
using GeneralUtils;
using UnityEngine;

namespace _Game.Scripts {
    public class CameraController : MonoBehaviour {
        [SerializeField] private Camera _camera;
        [SerializeField] [Range(0f, 1f)] private float _lerp;

        private Transform _target;
        private float _verticalRotation;

        public void Init(Transform target) {
            _target = target;
            UpdateValues(1f);
        }

        public void SetVerticalRotation(float verticalRotation) {
            _verticalRotation = verticalRotation;
        }

        private void LateUpdate() {
            UpdateValues(_lerp);
        }

        private void UpdateValues(float lerp) {
            var t = _camera.transform;
            t.position = Vector3.Lerp(t.position, _target.position, lerp);
            t.rotation = Quaternion.Euler(_target.rotation.eulerAngles.With(x: _verticalRotation));
        }
    }
}