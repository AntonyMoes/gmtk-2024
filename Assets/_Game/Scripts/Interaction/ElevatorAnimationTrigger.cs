using DG.Tweening;

namespace _Game.Scripts.Interaction {
    public class ElevatorAnimationTrigger : AnimationTrigger {
        protected override void StartAnimation() {
            SoundController.Instance.PlaySound("door_opening", 0f).DOFade(0.05f, 0.2f);
            DOTween.Sequence()
                .AppendInterval(2.5f)
                .AppendCallback(
                    () => SoundController.Instance.StopSound("door_opening", 0.5f)
                )
                .AppendInterval(1.2f)
                .AppendCallback(
                    () => SoundController.Instance.PlaySound("fall", 0f).DOFade(0.07f, 2f)
                )
                .AppendInterval(1.5f)
                .AppendCallback(
                    () => {
                        SoundController.Instance.StopSound("fall", 0.1f);
                        SoundController.Instance.PlaySound("door_opened", 5f, 0.8f);
                    }
                );
        }
    }
}