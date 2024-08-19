using UnityEngine;

namespace _Game.Scripts.Interaction {
    public class Pickup : EnterTrigger {
        [SerializeField] private string _type;
        public string Type => _type;

        protected override void OnTrigger(Interactor interactor) {
            interactor.Pickup(this);
            Destroy(gameObject);
        }
    }
}