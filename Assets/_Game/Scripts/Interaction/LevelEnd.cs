namespace _Game.Scripts.Interaction {
    public class LevelEnd : Trigger{
        protected override void OnTrigger(Interactor interactor) {
            App.Instance.FinishLevel();
        }
    }
}