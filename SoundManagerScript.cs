using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManagerScript : MonoBehaviour
{
    public static AudioClip collect_itemSound, footstepsSound, landSound, jumpSound, enemyAttack, jumpAttack, enemyAggro;
    static AudioSource audioSrc;


    // Start is called before the first frame update
    void Start()
    {

        collect_itemSound = Resources.Load<AudioClip>("collect_item");
        footstepsSound = Resources.Load<AudioClip>("footsteps");
        landSound = Resources.Load<AudioClip>("land");
        jumpSound = Resources.Load<AudioClip>("jump");
        enemyAttack = Resources.Load<AudioClip> ("enemy_attack");
        jumpAttack = Resources.Load<AudioClip> ("jump_attack");
        enemyAggro = Resources.Load<AudioClip> ("enemy_aggro");

        audioSrc = GetComponent<AudioSource>();

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public static void PlaySound (string clip)
    {

        switch (clip)
        {
            case "collect_item":
                audioSrc.PlayOneShot(collect_itemSound);
                break;
            case "footsteps":
                audioSrc.PlayOneShot(footstepsSound);
                break;
            case "land":
                audioSrc.PlayOneShot(landSound);
                break;
            case "jump":
                audioSrc.PlayOneShot(jumpSound);
                break;
            case "jumpAttack":
                audioSrc.PlayOneShot (jumpAttack);
                break;
            case "enemyAttack":
                audioSrc.PlayOneShot (enemyAttack);
                break;
            case "aggro":
                audioSrc.PlayOneShot (enemyAggro);
                break;
        }
    }
}
