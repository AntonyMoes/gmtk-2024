using UnityEngine;
using UnityEngine.Playables;

namespace _Game.Scripts.Interaction {
    public class AnimationTrigger : OnceEnterTrigger {
        [SerializeField] private PlayableDirector _director;

        protected override void OnOnceTrigger(Interactor interactor) {
            _director.Play();
            StartAnimation();
        }

        protected virtual void StartAnimation() { } 
    }
}