using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCollisionGrunt : MonoBehaviour
{
    public GameObject gruntingObject;
    private AudioSource playerGruntsAudioSource;
    public AudioClip playerGrunt1;
    public AudioClip playerGrunt2;
    public AudioClip playerGrunt3;

    private Dictionary<string, AudioClip> gruntAudioClips; // Dictionary to store different audio clips

    private void Start()
    {
        playerGruntsAudioSource = gruntingObject.GetComponent<AudioSource>();

        // Initialize audio clips
        gruntAudioClips = new Dictionary<string, AudioClip>
        {
            { "grunt_1", playerGrunt1 },
            { "grunt_2", playerGrunt2 },
            { "grunt_3", playerGrunt3 },
        };
    }

    // Play grunt sound effect
    public void PlayGrunt()
    {
        string gruntSoundName = "grunt_" + new System.Random().Next(1, 4);
        if (gruntAudioClips.ContainsKey(gruntSoundName))
        {
            playerGruntsAudioSource.clip = gruntAudioClips[gruntSoundName];
            playerGruntsAudioSource.Play();
        }
        else
        {
            Debug.LogWarning($"Sound audio clip for {gruntSoundName} not found!");
        }
    }
}
