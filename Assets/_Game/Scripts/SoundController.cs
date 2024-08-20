using System;
using System.Collections.Generic;
using System.Linq;
using GeneralUtils;
using DG.Tweening;
using UnityEngine;


namespace _Game.Scripts {
    public class SoundController : SingletonBehaviour<SoundController> {
        [SerializeField] private GameObject _sounds;

        [SerializeField] private AudioSource _currentMusic;
        [SerializeField] private AudioSource _nextMusic;

        [SerializeField] private AudioClip[] _clips;

        [SerializeField] private float globalVolume = 1f;
        [SerializeField] private float musicSwitchFadeDuration = 0.5f;
        [SerializeField] private bool isMuted = false;
        

        private readonly List<AudioSource> _soundSources = new List<AudioSource>();
        private Tween _musicTween;
        private Tween _musicLoopTween;

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
            source.volume = volume * GetGlobalVolumeMultiplier();
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

        public void StopAllSounds() {
            foreach (AudioSource ss in _soundSources) {
                ss.Stop();
            }
            // _currentMusic.Stop();
        }

        
        public AudioSource PlayMusic(string musicName, float volume = 1f) {
            _musicTween?.Kill();

            // Starting the music in the second player
            _nextMusic.clip = _clips.FirstOrDefault(clip => clip.name == musicName);
            _nextMusic.volume = 0f;
            _nextMusic.Play();
            _nextMusic.DOFade(volume * GetGlobalVolumeMultiplier(), musicSwitchFadeDuration);

            // Swapping the players
            (_currentMusic, _nextMusic) = (_nextMusic, _currentMusic);

            // If we had something playing in the first player =>
            // fade down & stop the player afterwards
            if (_nextMusic.isPlaying) {
                _nextMusic.DOFade(0f, musicSwitchFadeDuration);
                DOTween.Sequence()
                    .AppendInterval(musicSwitchFadeDuration)
                    .AppendCallback(() => {
                        _nextMusic.Stop();
                        _nextMusic.clip = null;
                    });
            } 

            if (_currentMusic.clip != null) {
                // Loop
                DOTween.Sequence()
                    .AppendInterval(_currentMusic.clip.length - musicSwitchFadeDuration)
                    .AppendCallback(() => PlayMusic(musicName, volume));
            }

            return _currentMusic;
        }

        public float GetGlobalVolumeMultiplier() {
            float multiplier = isMuted ? 0.0001f : 1f;
            return globalVolume * multiplier;
        }

        public void ToggleMute() {
            isMuted = !isMuted;
            var multiplier = GetGlobalVolumeMultiplier();
            if (!isMuted) {
                multiplier *= 10000f;
            }
            _soundSources.Where(ss => ss.isPlaying).ForEach(ss => { ss.volume *= multiplier; });
            _currentMusic.volume *= multiplier;
        }

        private void OnDestroy() {
            _musicTween?.Kill();
        }
    }
}
