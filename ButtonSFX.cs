using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonSFX : MonoBehaviour
{
    public static ButtonSFX instance = null;

    AudioSource source;

        void Awake()
        {
            if (instance == null){
                instance = this;
            } else if (instance != this) {
        }
        DontDestroyOnLoad(this.gameObject);

        source = GetComponent<AudioSource>();
    }

    public void PlaySFX(AudioClip sfx)
    {
        instance.source.PlayOneShot(sfx);
    }
}
