using UnityEngine;

namespace _Game.Scripts.UI {
    public abstract class ProgressBar : MonoBehaviour {
        public float MinValue { get; private set; }
        public float MaxValue { get; private set; }

        private float _value;
        public float Value {
            get => _value;
            set {
                _value = value;
                _progress = (_value - MinValue) / (MaxValue - MinValue);
                UpdateProgress(_progress);
            }
        }

        private float _progress;
        public float Progress {
            get => _progress;
            set {
                _progress = value;
                // value was set
                if (MinValue != MaxValue) {
                    _value = _progress * (MaxValue - MinValue) + MinValue;
                }
                UpdateProgress(_progress);
            }
        }

        public void Load(float minValue, float maxValue) {
            MinValue = minValue;
            MaxValue = maxValue;
        }

        protected abstract void UpdateProgress(float progress);
    }
}