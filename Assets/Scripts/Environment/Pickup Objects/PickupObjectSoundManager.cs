using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickupObjectSoundManager : MonoBehaviour
{
    public AudioSource pickUpSound; 
    public AudioSource actionPointSound; 

    void Start()
    {
        AudioSource[] audioSources = GetComponents<AudioSource>();
        if (audioSources.Length >= 1)
        {
            pickUpSound = audioSources[0];
        }
        if (audioSources.Length >= 2)
        {
            actionPointSound = audioSources[1];
        }
    }

    // Public methods to play sounds
    public void PlayPickUpSound()
    {
        if (pickUpSound != null)
            pickUpSound.Play();
    }

    public void PlayActionPointSound()
    {
        if (actionPointSound != null)
            actionPointSound.Play();
    }

}
