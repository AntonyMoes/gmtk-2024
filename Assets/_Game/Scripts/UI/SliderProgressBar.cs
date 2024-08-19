using UnityEngine;
using UnityEngine.UI;

namespace _Game.Scripts.UI {
    public class SliderProgressBar : ProgressBar {
        [SerializeField] private Slider _slider;

        protected override void UpdateProgress(float progress) {
            _slider.value = progress;
        }
    }
}