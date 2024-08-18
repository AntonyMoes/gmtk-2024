using System;
using System.Collections.Generic;
using System.Linq;
using GeneralUtils;
using DG.Tweening;
using GeneralUtils;
using UnityEngine;


namespace _Game.Scripts {
    public class SoundController : SingletonBehaviour<SoundController> {
        [SerializeField] private GameObject _sounds;

        [SerializeField] private AudioSource _music;

        [SerializeField] private AudioClip[] _clips;

        [SerializeField] private float globalVolume = 1f;

        private readonly List<AudioSource> _soundSources = new List<AudioSource>();
        private Tween _musicTween;

        public AudioSource PlaySound(
            string soundName, 
            float volume = 1f, 
            float pitch = 1f, 
            bool restart = true,
            bool loop = false
        ) {
            var source = _soundSources.FirstOrDefault(ss => ss?.clip?.name == soundName);
            if (source == null) {
                source =  _soundSources.FirstOrDefault(ss => !ss.isPlaying);
            }
            if (source == null) {
                source = _sounds.AddComponent<AudioSource>();
                _soundSources.Add(source);
            } else if (source.isPlaying && !restart) {
                return source;
            }

            source.DOKill();
            source.clip = _clips.First(clip => clip.name == soundName);

            source.Play();
            source.volume = volume * globalVolume;
            source.pitch = pitch;
            source.loop = loop;

            return source;
        }

        public void StopSound(string soundName, float fade = 0f) {
            var source = _soundSources.FirstOrDefault(ss => ss?.clip?.name == soundName);
            if (source == null || !source.isPlaying) {
                return;
            }
            DOTween.Sequence()
                .Append(source.DOFade(0f, fade))
                .AppendCallback(()=> {
                    source.Stop();
                    _soundSources.Remove(source);
                });
        }

        public AudioSource PlayMusic(string musicName, float volume = 1f) {
            _musicTween?.Kill();
            const float fadeDuration = 0.3f;
            if (_music.isPlaying) {
                _musicTween = DOTween.Sequence()
                    .Append(_music.DOFade(0f, fadeDuration))
                    .AppendCallback(SetNew)
                    .Append(_music.DOFade(volume * globalVolume, fadeDuration));
            } else {
                SetNew();
                _music.DOFade(volume, fadeDuration);
            }

            return _music;

            void SetNew() {
                _music.Stop();
                _music.clip = _clips.First(clip => clip.name == musicName);
                _music.loop = true;
                _music.Play();
            }
        }

        public void ToggleMute() {
            globalVolume *= -1;
            float multiplier = globalVolume > 0 ? 10000f : 0.0001f;
            _soundSources.Where(ss => ss.isPlaying).ForEach(ss => { ss.volume *= multiplier; });
            _music.volume *= multiplier;
            Debug.Log(_music.volume);
            
        }

        private void OnDestroy() {
            _musicTween?.Kill();
        }
    }
}
