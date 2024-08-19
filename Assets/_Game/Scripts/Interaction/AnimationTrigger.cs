using UnityEngine;
using UnityEngine.Playables;

namespace _Game.Scripts.Interaction {
    public class AnimationTrigger : Trigger {
        [SerializeField] private PlayableDirector _director;

        private bool _activated;
        
        protected override void OnTrigger(Interactor interactor) {
            if (_activated) {
                return;
            }

            _activated = true;
            _director.Play();
            StartAnimation();
        }

        protected virtual void StartAnimation() { } 
    }
}