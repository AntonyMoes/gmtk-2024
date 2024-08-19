using UnityEngine;

namespace _Game.Scripts.Interaction {
    [RequireComponent(typeof(Collider))]
    public abstract class Trigger : MonoBehaviour {
        private void OnTriggerEnter(Collider other) {
            var interactor = other.GetComponent<Interactor>();
            if (interactor != null) {
                OnTrigger(true, interactor);
            }
        }

        private void OnTriggerExit(Collider other) {
            var interactor = other.GetComponent<Interactor>();
            if (interactor != null) {
                OnTrigger(false, interactor);
            }
        }

        protected abstract void OnTrigger(bool enter, Interactor interactor);
    }
}