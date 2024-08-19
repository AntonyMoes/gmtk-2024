using DG.Tweening;
using TMPro;
using UnityEngine;

namespace _Game.Scripts.Interaction {
    public class DoorConsole : Interactable {
        [SerializeField] private string _type;
        [SerializeField] private GameObject _screen;
        [SerializeField] private GameObject _closedText;
        [SerializeField] private GameObject _canOpenText;
        [SerializeField] private GameObject _openingText;
        [SerializeField] private GameObject _openedText;

        [Header("Door")]
        [SerializeField] private Transform _door;
        [SerializeField] private Vector3 _targetLocalRotation;
        [SerializeField] private Vector3 _targetLocalPosition;
        [SerializeField] private Ease _easing = Ease.InOutSine;
        [SerializeField] private float _time;

        private bool _opened;
        private bool _opening;

        public string Type => _type;
        
        public override void SetSelected(bool selected, bool canInteract = false) {
            _screen.SetActive(selected);
            if (!selected) {
                return;
            }
            SoundController.Instance.PlaySound("doorconsole_enable", 0.2f);
            _openingText.SetActive(_opened && _opening);
            _openedText.SetActive(_opened && !_opening);
            _closedText.SetActive(!_opened && !canInteract);
            _canOpenText.SetActive(!_opened && canInteract);
        }

        public override void Interact() {
            _opening = true;
            _opened = true;
            SoundController.Instance.PlaySound("doorconsole_interact", 0.1f);
            SoundController.Instance.PlaySound("door_opening", 0.4f);

            DOTween.Sequence()
                .Insert(0, _door.DOLocalRotate(_targetLocalRotation, _time).SetEase(_easing))
                .Insert(0, _door.DOLocalMove(_targetLocalPosition, _time).SetEase(_easing))
                .OnComplete(EndOpening);
        }

        private void EndOpening() {
            SoundController.Instance.StopSound("door_opening", 0.2f);
            SoundController.Instance.PlaySound("door_opened", 0.8f);
            _opening = false;
        }
    }
}