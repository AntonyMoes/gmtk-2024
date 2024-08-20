namespace _Game.Scripts.Interaction {
    public class EndCutsceneTrigger : OnceEnterTrigger {
        protected override void OnOnceTrigger(Interactor interactor) {
            App.Instance.StartEndCutscene();
        }
    }
}