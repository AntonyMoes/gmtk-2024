namespace _Game.Scripts.Interaction {
    public abstract class OnceEnterTrigger : EnterTrigger {
        private bool _activated;

        protected sealed override void OnTrigger(Interactor interactor) {
            if (_activated) {
                return;
            }

            _activated = true;
            OnOnceTrigger(interactor);
        }

        protected abstract void OnOnceTrigger(Interactor interactor);
    }
}