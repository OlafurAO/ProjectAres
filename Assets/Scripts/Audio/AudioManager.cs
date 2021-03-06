﻿using UnityEngine.Audio;
using UnityEngine;
using System;

public class AudioManager : MonoBehaviour {
    public Sound[] sounds = new Sound[20];

    void Awake() {
        foreach (Sound s in sounds) {
            if(s.clip != null) {
                s.source = gameObject.AddComponent<AudioSource>();
                s.source.clip = s.clip;
                s.source.volume = s.volume;
                s.source.pitch = s.pitch;
            }
        }
    }

    // Start is called before the first frame update
    void Start() {
        
    }

    // Update is called once per frame
    void Update() {
        
    }

    public void Play(string name, float delay) {
        Sound s = Array.Find(sounds, sound => sound.name == name);
        if(s.source != null){
            s.source.PlayDelayed(delay);
        }
    }

    public void PlayLoop(string name, float delay, bool loop) {
        Sound s = Array.Find(sounds, sound => sound.name == name);
        if(s.source == null){
            return;
        }
        s.source.loop = loop;
        s.source.PlayDelayed(delay);
    }

    public void Stop(string name) {
        Sound s = Array.Find(sounds, sound => sound.name == name);
        if(s.source != null){
            s.source.Stop();    
        }

    }
}
