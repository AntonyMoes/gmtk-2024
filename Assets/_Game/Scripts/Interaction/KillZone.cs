
namespace _Game.Scripts.Interaction {
    public class KillZone : EnterTrigger {
        protected override void OnTrigger(Interactor interactor) {
            App.Instance.Kill();
        }
    }
}