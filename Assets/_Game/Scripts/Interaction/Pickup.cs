using UnityEngine;

namespace _Game.Scripts.Interaction {
    public class Pickup : MonoBehaviour {
        [SerializeField] private string _type;
        public string Type => _type;

        private void OnTriggerEnter(Collider other) {
            var interactor = other.GetComponent<Interactor>();
            if (interactor != null) {
                interactor.Pickup(this);
                Destroy(gameObject);
            }
        }
    }
}