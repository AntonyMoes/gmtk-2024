using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace _Game.Scripts.UI {
    public class LevelButton : MonoBehaviour {
        [SerializeField] private Button _button;
        [SerializeField] private TextMeshProUGUI _text;

        public void Load(int index, bool canSelect, Action<int> onSelect) {
            _button.interactable = canSelect;
            _button.onClick.AddListener(() => onSelect(index));
            _text.text = (index + 1).ToString();
        }
    }
}