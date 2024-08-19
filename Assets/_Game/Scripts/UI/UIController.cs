using System;
using TMPro;
using UnityEngine;

namespace _Game.Scripts.UI {
    public class UIController : MonoBehaviour {
        [SerializeField] private TextMeshProUGUI _debugText;
        public TextMeshProUGUI DebugText => _debugText;
        
        [SerializeField] private ProgressBar _staminaProgressBar;
        public ProgressBar StaminaProgressBar => _staminaProgressBar;
    }
}