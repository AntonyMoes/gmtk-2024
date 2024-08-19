using UnityEngine;

namespace _Game.Scripts.Interaction {
    [RequireComponent(typeof(Collider))]
    public abstract class Trigger : MonoBehaviour {
        private void OnTriggerEnter(Collider other) {
            var interactor = other.GetComponent<Interactor>();
            if (interactor != null) {
                OnTrigger(interactor);
            }
        }

        protected abstract void OnTrigger(Interactor interactor);
    }
}