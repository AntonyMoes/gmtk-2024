using DG.Tweening;
using UnityEngine;

namespace _Game.Scripts.Climbing {
    public class PickaxeAnimator : MonoBehaviour {
        [SerializeField] private Transform _left;
        [SerializeField] private Transform _right;
        // [SerializeField] private Transform _animationPoint;

        [Header("Settings")]
        [SerializeField] private float _initialSpread;
        [SerializeField] private float _movementImpact;
        [SerializeField] private float _minRequiredDistance;
        [SerializeField] private float _maxRequiredDistance;
        [SerializeField] private Vector3 _animationRotation;
        [SerializeField] private float _animationNormalMultiplier;
        [SerializeField] private float _animationDuration;

        // private Transform _leftParent;
        // private Transform _rightParent;

        private Contact _lastContact;
        private Info _leftInfo;
        private Info _rightInfo;

        private Tween _currentAnimation;

        public void LatchOn(Contact contact) {
            _lastContact = contact;
            _leftInfo = CreateInfo(_left, contact, -_initialSpread);
            _rightInfo = CreateInfo(_right, contact, _initialSpread);
        }

        private void Update() {
            TryUpdateAndMove(_leftInfo);
            TryUpdateAndMove(_rightInfo);
        }

        public void Move(Vector3 movement, Contact contact) {
            _lastContact = contact;
            var (close, far) = GetCloseAndFar();
            if (close.Distance > _minRequiredDistance || far.Distance > _maxRequiredDistance) {
                var oldContact = far.Contact;
                far.Contact = new Contact {
                    Point = contact.Point + movement.normalized * _movementImpact,
                    Normal = contact.Normal
                };
                Animate(far, oldContact);
            }
        }

        public void LatchOff() {
            _leftInfo = null;
            _rightInfo = null;
            _currentAnimation?.Kill();
            _currentAnimation = null;
            _left.localRotation = Quaternion.Euler(Vector3.zero);
            _left.localPosition = Vector3.zero;
            _right.localRotation = Quaternion.Euler(Vector3.zero);
            _right.localPosition = Vector3.zero;
        }

        private Info CreateInfo(Transform pickaxe, Contact contact, float rightShift) {
            return new Info {
                Pickaxe = pickaxe,
                Contact = contact,
                Distance = 0f,
                RightShift = rightShift
            };
        }

        private void TryUpdateAndMove(Info info) {
            if (info == null || _lastContact == null) {
                return;
            }

            if (!info.InAnimation) {
                info.Pickaxe.position = info.TargetPosition(transform);
                info.Pickaxe.rotation = info.TargetRotation();
            }

            info.Distance = Vector3.Distance(info.Contact.Point, _lastContact.Point);
        }

        private void Animate(Info info, Contact oldContact) {
            _currentAnimation?.Kill();
            info.InAnimation = true;



            var duration = _animationDuration;
            var half = duration / 2f;

            var midNormal = (oldContact.Normal + info.Contact.Normal) / 2f;
            var midPoint = (oldContact.Point + info.Contact.Point) / 2f + midNormal * _animationNormalMultiplier;
            var pathTween = info.Pickaxe.DOPath(new[] { midPoint, info.TargetPosition(transform) },
                duration, PathType.CatmullRom);
            var rotationTween = DOTween.Sequence()
                .Append(info.Pickaxe.DORotateQuaternion(Quaternion.LookRotation(-midNormal) * Quaternion.Euler(_animationRotation), half).SetEase(Ease.OutSine))
                .Append(info.Pickaxe.DORotateQuaternion(info.TargetRotation(), half).SetEase(Ease.InSine));

            _currentAnimation = DOTween.Sequence()
                .Insert(0, pathTween)
                .Insert(0, rotationTween)
                .OnComplete(() => {
                    _currentAnimation = null;
                    info.InAnimation = false;
                })
                .OnKill(() => {
                    info.InAnimation = false;
                });
        }

        private (Info, Info) GetCloseAndFar() {
            return _leftInfo.Distance < _rightInfo.Distance ? (_leftInfo, _rightInfo) : (_rightInfo, _leftInfo);
        }

        private class Info {
            public Transform Pickaxe;
            public Contact Contact;
            public float RightShift;
            public float Distance;
            public bool InAnimation;

            public Vector3 TargetPosition(Transform transform) {
                return Contact.Point + Vector3.ProjectOnPlane(transform.right, Contact.Normal).normalized * RightShift;
            }

            public Quaternion TargetRotation() {
                return Quaternion.LookRotation(-Contact.Normal);
            }
        }
    }
}