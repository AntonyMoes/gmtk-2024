
namespace _Game.Scripts.Interaction {
    public class KillZone : Trigger {
        protected override void OnTrigger(Interactor interactor) {
            App.Instance.Kill();
        }
    }
}