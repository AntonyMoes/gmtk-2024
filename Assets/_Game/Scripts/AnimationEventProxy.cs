using UnityEngine;
using UnityEngine.Events;

namespace _Game.Scripts {
    public class AnimationEventProxy : MonoBehaviour {
        [SerializeField] private UnityEvent _onTrigger;

        public void Trigger() {
            _onTrigger?.Invoke();
        }
    }
}