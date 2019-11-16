using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicManager : MonoBehaviour
{
    public static MusicManager instance = null;

    [SerializeField] AudioClip levelCompleteClip = null; // The music triggered on completion of the level.
    [SerializeField] AudioClip[] levelMusicClips = null;

    AudioSource source;

    public AudioSource Source {
        get {
            return source;
        }
    }

    public AudioClip LevelCompleteClip {
        get {
            return levelCompleteClip;
        }
    }


    public AudioClip[] LevelMusicClips
    {
        get
        {
            return levelMusicClips;
        }
    }
    void Awake () {
        if (instance == null) {
            instance = this;
        } else if (instance != this) {
            Destroy (gameObject);
        }

            DontDestroyOnLoad (gameObject);
        source = GetComponent<AudioSource>();
    }
}
