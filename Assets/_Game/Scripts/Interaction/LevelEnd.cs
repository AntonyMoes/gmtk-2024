namespace _Game.Scripts.Interaction {
    public class LevelEnd : EnterTrigger{
        protected override void OnTrigger(Interactor interactor) {
            App.Instance.FinishLevel(true);
        }
    }
}