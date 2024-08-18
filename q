warning: in the working copy of 'Assets/Resources/DOTweenSettings.asset', LF will be replaced by CRLF the next time Git touches it
warning: in the working copy of 'Assets/_Game/Scenes/App.unity', LF will be replaced by CRLF the next time Git touches it
[1mdiff --git a/Assets/_Game/Scenes/App.unity b/Assets/_Game/Scenes/App.unity[m
[1mindex aef2ed2..f9edf26 100644[m
[1m--- a/Assets/_Game/Scenes/App.unity[m
[1m+++ b/Assets/_Game/Scenes/App.unity[m
[36m@@ -1708,6 +1708,22 @@[m [mPrefabInstance:[m
   m_Modification:[m
     m_TransformParent: {fileID: 0}[m
     m_Modifications:[m
[32m+[m[32m    - target: {fileID: -4343846941915050800, guid: 9daa3fef120f64648befe86e01f74127, type: 3}[m
[32m+[m[32m      propertyPath: _clips.Array.size[m
[32m+[m[32m      value: 0[m
[32m+[m[32m      objectReference: {fileID: 0}[m
[32m+[m[32m    - target: {fileID: -4343846941915050800, guid: 9daa3fef120f64648befe86e01f74127, type: 3}[m
[32m+[m[32m      propertyPath: _clips.Array.data[0][m
[32m+[m[32m      value:[m[41m [m
[32m+[m[32m      objectReference: {fileID: 8300000, guid: b7b94b316ac73ca4cbab336675e986b4, type: 3}[m
[32m+[m[32m    - target: {fileID: -4343846941915050800, guid: 9daa3fef120f64648befe86e01f74127, type: 3}[m
[32m+[m[32m      propertyPath: _clips.Array.data[1][m
[32m+[m[32m      value:[m[41m [m
[32m+[m[32m      objectReference: {fileID: 8300000, guid: b7b94b316ac73ca4cbab336675e986b4, type: 3}[m
[32m+[m[32m    - target: {fileID: -4343846941915050800, guid: 9daa3fef120f64648befe86e01f74127, type: 3}[m
[32m+[m[32m      propertyPath: _clips.Array.data[2][m
[32m+[m[32m      value:[m[41m [m
[32m+[m[32m      objectReference: {fileID: 8300000, guid: e80f0344def8ddb44b0811a2ad529237, type: 3}[m
     - target: {fileID: 3841635818429146153, guid: 9daa3fef120f64648befe86e01f74127, type: 3}[m
       propertyPath: m_Name[m
       value: SoundController[m
[1mdiff --git a/Assets/_Game/Scripts/PlayerController.cs b/Assets/_Game/Scripts/PlayerController.cs[m
[1mindex 9a668ca..e5269f4 100644[m
[1m--- a/Assets/_Game/Scripts/PlayerController.cs[m
[1m+++ b/Assets/_Game/Scripts/PlayerController.cs[m
[36m@@ -3,6 +3,7 @@[m [musing System.Collections.Generic;[m
 using System.Linq;[m
 using GeneralUtils;[m
 using TMPro;[m
[32m+[m[32musing DG.Tweening;[m
 using UnityEngine;[m
 [m
 namespace _Game.Scripts {[m
[36m@@ -153,6 +154,9 @@[m [mnamespace _Game.Scripts {[m
         }[m
 [m
         private void Move(Vector3 speed, float deltaTime) {[m
[32m+[m[32m            if (speed.magnitude > 0 && _state.Value == State.Grounded) {[m
[32m+[m[32m                SoundController.Instance.PlaySound("walk_default", 0.3f);[m[41m       [m
[32m+[m[32m            }[m
             _lastSetVel = speed;[m
             _rb.velocity = speed;[m
         }[m
[36m@@ -162,6 +166,9 @@[m [mnamespace _Game.Scripts {[m
         }[m
 [m
         private void Jump() {[m
[32m+[m[32m            if (_state.Value != State.Falling) {[m
[32m+[m[32m                SoundController.Instance.PlaySound("jump_default_temp", 0.3f);[m
[32m+[m[32m            }[m
             switch (_state.Value) {[m
                 case State.Grounded:[m
                 case State.Sliding:[m
[36m@@ -236,6 +243,16 @@[m [mnamespace _Game.Scripts {[m
                 _rb.velocity = Vector3.zero;[m
             }[m
 [m
[32m+[m[32m            switch(state) {[m
[32m+[m[32m                case State.Grounded:[m
[32m+[m[32m                    if (_state.Value == State.Falling) {[m
[32m+[m[32m                        SoundController.Instance.PlaySound("land_default", 0.3f);[m
[32m+[m[32m                    }[m[41m    [m
[32m+[m[32m                    break;[m
[32m+[m[32m                default:[m
[32m+[m[32m                    break;[m
[32m+[m[32m            }[m
[32m+[m
             _state.Value = state;[m
         }[m
 [m
[1mdiff --git a/Assets/_Game/Scripts/SoundController.cs b/Assets/_Game/Scripts/SoundController.cs[m
[1mindex c1a4d7f..8b22f74 100644[m
[1m--- a/Assets/_Game/Scripts/SoundController.cs[m
[1m+++ b/Assets/_Game/Scripts/SoundController.cs[m
[36m@@ -1,13 +1,16 @@[m
 ﻿using System;[m
 using System.Collections.Generic;[m
 using System.Linq;[m
[32m+[m[32musing GeneralUtils;[m
 using DG.Tweening;[m
 using GeneralUtils;[m
 using UnityEngine;[m
 [m
[32m+[m
 namespace _Game.Scripts {[m
     public class SoundController : SingletonBehaviour<SoundController> {[m
         [SerializeField] private GameObject _sounds;[m
[32m+[m
         [SerializeField] private AudioSource _music;[m
 [m
         [SerializeField] private AudioClip[] _clips;[m
[36m@@ -15,15 +18,18 @@[m [mnamespace _Game.Scripts {[m
         private readonly List<AudioSource> _soundSources = new List<AudioSource>();[m
         private Tween _musicTween;[m
 [m
[31m-        public AudioSource PlaySound(string soundName, float volume = 1f, float pitch = 1f) {[m
[31m-            var source = _soundSources.FirstOrDefault(ss => !ss.isPlaying);[m
[32m+[m[32m        public AudioSource PlaySound(string soundName, float volume = 1f, float pitch = 1f, bool interruptable = true) {[m
[32m+[m[32m            var source = _soundSources.FirstOrDefault(ss => ss?.clip?.name == soundName);[m
             if (source == null) {[m
                 source = _sounds.AddComponent<AudioSource>();[m
                 _soundSources.Add(source);[m
[32m+[m[32m            } else if (source.isPlaying && !interruptable) {[m
[32m+[m[32m                return source;[m
             }[m
 [m
             source.DOKill();[m
             source.clip = _clips.First(clip => clip.name == soundName);[m
[32m+[m
             source.Play();[m
             source.volume = volume;[m
             source.pitch = pitch;[m
