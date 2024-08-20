using System.Collections;
using UnityEngine;
using DG.Tweening;
using Random = UnityEngine.Random;

namespace _Game.Scripts.UI {
    public class MagistralSound : MonoBehaviour {
        private Coroutine _beepRoutine;
        private Tween _currentAnimation;
        private PlayerController _player;

        private void Start() {
            _beepRoutine = StartCoroutine(BeepRoutine());
        }

        private void OnDestroy() {
            if (_beepRoutine != null) {
                StopCoroutine(_beepRoutine);
                _beepRoutine = null;
            }

            _currentAnimation?.Kill();
            _currentAnimation = null;
        }

        private IEnumerator BeepRoutine() {
            while (true) {
                DoBeep();
                yield return new WaitForSeconds(20f);
            }
        }

        private void DoBeep() {
            if (_player == null) {
                _player = FindObjectOfType<PlayerController>();
                if (_player == null) {
                    return;
                }
            }

            var playerPosition = _player.transform.position;
            var distanceToPlayer = (playerPosition - transform.position).magnitude;

            if (distanceToPlayer > 250f) {
                return;
            }

            var soundName = Random.value < 0.66f ? "car" : "truck";
            var soundVolume = Mathf.Exp(0.06f - 0.03f * distanceToPlayer);

            _currentAnimation = DOTween.Sequence()
                .AppendInterval(Random.Range(0.2f, 10f))
                .AppendCallback(() => {
                    SoundController.Instance.PlaySound(soundName, soundVolume, Random.Range(0.5f, 1.5f));
                    _currentAnimation = null;
                });
        }
    }
}