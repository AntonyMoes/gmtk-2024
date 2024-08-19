namespace _Game.Scripts.Interaction {
    public abstract class EnterTrigger : Trigger {
        protected sealed override void OnTrigger(bool enter, Interactor interactor) {
            if (enter) {
                OnTrigger(interactor);
            }
        }

        protected abstract void OnTrigger(Interactor interactor);
    }
}