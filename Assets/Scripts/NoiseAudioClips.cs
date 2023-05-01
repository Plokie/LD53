using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct AudioSoundPair {
    public NoiseSound.Type type;
    public AudioClip clip;
}

public class NoiseAudioClips : MonoBehaviour
{
    public static NoiseAudioClips Instance { get; private set; }
    void Awake() 
    { 
        if (Instance != null && Instance != this) Destroy(this); 
        else Instance = this;
    }
    public List<AudioSoundPair> audioSounds;
    // public AnimationCurve rolloff;
    public AudioClip GetClip(NoiseSound.Type type) {
        // print(audioSounds.Count);
        // print(type);
        foreach(AudioSoundPair pair in audioSounds) {
            if(pair.type == type) return pair.clip;
            
        }
        return audioSounds[0].clip;
    }
}
