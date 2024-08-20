using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

namespace _Game.Scripts.UI {
    public class MagistralSound : MonoBehaviour
    {
        void Start()
        {
            InvokeRepeating("DoBeep", 0f, 20f);
        }

        void DoBeep()
        {
            Debug.Log("BEEP");
            var playerPosition = FindObjectOfType<PlayerController>().gameObject.transform.position;
            var distanceToPlayer = (playerPosition - transform.position).magnitude;
            Debug.Log(distanceToPlayer);
            if (distanceToPlayer > 250f) {
                return;
            }
            
            var soundName = Random.value < 0.66f ? "car": "truck";
            var soundVolume = Mathf.Exp(0.06f - 0.03f * distanceToPlayer);

            DOTween.Sequence()
                .AppendInterval(Random.Range(0f, 10f))
                .AppendCallback(
                    () => {
                        SoundController.Instance.PlaySound(soundName, soundVolume, Random.Range(0.5f, 1.5f));
                    }
                );
            
        }
    }
}
