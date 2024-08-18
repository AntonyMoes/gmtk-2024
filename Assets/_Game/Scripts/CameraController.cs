using GeneralUtils;
using UnityEngine;

namespace _Game.Scripts {
    public class CameraController : MonoBehaviour {
        [SerializeField] private Camera _camera;
        [SerializeField] [Range(0f, 1f)] private float _lerp;

        private Transform _target;

        public float VerticalRotation { get; set; }

        public void SetTarget(Transform target) {
            _target = target;
            UpdateValues(1f);
        }

        private void LateUpdate() {
            UpdateValues(_lerp);
        }

        private void UpdateValues(float lerp) {
            if (_target == null) {
                return;
            }

            var t = _camera.transform;
            t.position = Vector3.Lerp(t.position, _target.position, lerp);
            t.rotation = Quaternion.Euler(_target.rotation.eulerAngles.With(x: VerticalRotation));
        }
    }
}