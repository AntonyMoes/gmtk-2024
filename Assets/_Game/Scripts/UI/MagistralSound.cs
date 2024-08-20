using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace _Game.Scripts.UI {
    public class MagistralSound : MonoBehaviour
    {
        void Start()
        {
            
        }

        void Update()
        {
            var playerPosition = FindObjectOfType<PlayerController>().gameObject.transform.position;
            var distanceToPlayer = (playerPosition - transform.position).magnitude;
            if (distanceToPlayer > 150f) {
                return;
            }
            
            if (Random.value < 0.0005f) {
                var soundName = Random.value < 0.66f ? "car": "truck";
                var soundVolume = Mathf.Exp(0.06f - 0.03f * distanceToPlayer);
                SoundController.Instance.PlaySound(soundName, soundVolume, Random.Range(0.5f, 1.5f));
            }
        }
    }
}
