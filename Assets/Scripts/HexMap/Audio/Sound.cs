using UnityEngine.Audio;
using UnityEngine;

[System.Serializable]
public class Sound {
    public AudioClip clip;
    public string name;

    [Range(0.0f, 1.0f)]
    public float volume;
    [Range(0.0f, 3.0f)]
    public float pitch;

    [HideInInspector]
    public AudioSource source;
}
