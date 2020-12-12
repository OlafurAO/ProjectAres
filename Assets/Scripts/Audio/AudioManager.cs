using UnityEngine.Audio;
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
        s.source.PlayDelayed(delay);
    }
}
