namespace _Game.Scripts.Interaction {
    public class PickaxePickup : EnterTrigger {
        protected override void OnTrigger(Interactor interactor) {
            interactor.Pickup(this);
            Destroy(gameObject);
        }
    }
}