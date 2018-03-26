using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour {

    public AudioSource source;
    public AudioSource musicSource;
    public static SoundManager instance = null;

    void Awake() {
        if (instance == null) {
            instance = this;
        }
        else if (instance != this) {
            Destroy(gameObject);

            DontDestroyOnLoad(gameObject);
        }
    }

    public void PlaySingle(AudioClip clip) {
        source.clip = clip;
        source.Play();
    }

    
}