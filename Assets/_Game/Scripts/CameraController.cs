using GeneralUtils;
using UnityEngine;

namespace _Game.Scripts {
    public class CameraController : MonoBehaviour {
        [SerializeField] private GameObject _cameraContainer;
        [SerializeField] private Camera _camera;
        [SerializeField] [Range(0f, 1f)] private float _lerp;
        [SerializeField] [Range(0f, 1f)] private float _lookLerp;

        public Transform CameraTransform => _camera.transform;

        private Transform _target;

        public void SetTarget(Transform target) {
            _target = target;
            UpdateValues(1f, 1f);
        }

        private void LateUpdate() {
            UpdateValues(_lerp, _lookLerp);
        }

        private void UpdateValues(float lerp, float lookLerp) {
            if (_target == null) {
                return;
            }

            var t = _cameraContainer.transform;
            t.position = Vector3.Lerp(t.position, _target.position, lerp);

            var newRotation = _target.rotation;
            t.rotation = Quaternion.Lerp(t.rotation, newRotation, lookLerp);
        }
    }
}