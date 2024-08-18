using UnityEngine;

namespace _Game.Scripts.Interaction {
    [RequireComponent(typeof(Collider))]
    public abstract class Interactable : MonoBehaviour {
        public abstract void SetSelected(bool selected, bool canInteract = false);
        public abstract void Interact();
    }
}