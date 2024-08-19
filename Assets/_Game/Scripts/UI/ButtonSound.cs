using UnityEngine;

namespace _Game.Scripts.UI {
    public class ButtonSound : MonoBehaviour {
        public void ButtonClick() {
            SoundController.Instance.PlaySound("doorconsole_enable", 1.5f);
        }
    }
}