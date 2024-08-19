using System;
using GeneralUtils;
using UnityEngine;

namespace _Game.Scripts {
    public class Follower : MonoBehaviour {
        [SerializeField] private Transform _target;
        [SerializeField] [Range(0f, 1f)] private float _lerp;
        [SerializeField] [Range(0f, 1f)] private float _lookLerp;

        private void Awake() {
            transform.parent = null;
            UpdateValues(1f, 1f);
        }

        private void LateUpdate() {
            UpdateValues(_lerp, _lookLerp);
        }

        private void UpdateValues(float lerp, float lookLerp) {
            if (_target == null) {
                Destroy(gameObject);
                return;
            }

            var t = transform;
            t.position = Vector3.Lerp(t.position, _target.position, lerp);

            var newRotation = _target.rotation;
            t.rotation = Quaternion.Lerp(t.rotation, newRotation, lookLerp);
        }
    }
}