using System;
using DG.Tweening;
using GeneralUtils.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace _Game.Scripts.UI {
    public class EndUICutscene : UIElement {
        [SerializeField] private CanvasGroup _mainGroup;
        [SerializeField] private CanvasGroup _contentGroup;
        [SerializeField] private CanvasGroup _groupA;
        [SerializeField] private Image _image;

        protected override void PerformShow(Action onDone = null) {
            _image.color = Color.white;
            _mainGroup.alpha = 0f;
            _contentGroup.alpha = 0f;
            _groupA.alpha = 0f;

            SoundController.Instance.StopMusic(2.6f);
            DOTween.To(
                SoundController.Instance.GetGlobalVolume, 
                SoundController.Instance.SetGlobalVolume, 
                0f, 
                2.6f
            );

            DOTween.Sequence()
                .AppendInterval(.6f)
                .Append(_mainGroup.DOFade(1f, 5f))
                .AppendInterval(2f)
                .AppendCallback(() => {
                    _image.color = Color.black;
                    App.Instance.Kill(false);
                    SoundController.Instance.StopAllSounds(true);
                })
                .AppendInterval(2f)
                .Append(_contentGroup.DOFade(1f, .5f))
                .AppendInterval(1f)
                .AppendCallback(() => {
                    SoundController.Instance.PlaySound("wake_up", 0.5f).DOFade(1.5f, 7f);
                })
                .AppendInterval(6f)
                .Append(_contentGroup.DOFade(0f, 2f))
                .AppendInterval(3f)
                .Append(_groupA.DOFade(1f, 2f))
                .AppendInterval(3f)
                .Append(_groupA.DOFade(0f, 2f))
                // TODO THANK PLAYER FOR PLAYING
                .AppendCallback(() => {
                    onDone?.Invoke();
                    Hide();
                    App.Instance.FinishLevel(true);
                });
        }
    }
}