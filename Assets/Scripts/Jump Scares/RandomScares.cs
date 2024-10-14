using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomScares : MonoBehaviour
{
    private float scareTimer;
    public float minTimeTillNextScare = 35f;
    public float maxTimeTillNextScare = 60f;
    
    private int noOfScares = 4;
    private int randomScareNo;
    private int previousRandomScare;

    private AudioSource audioSource;
    public AudioClip audioClip1;
    public AudioClip audioClip2;
    public AudioClip audioClip3;
    public AudioClip audioClip4;

    private Dictionary<string, AudioClip> audioClips; // Dictionary to store different audio clips

    // If another scare is busy, don't interrupt it
    public GameObject notCaughtScares;
    public GameObject caughtScares;
    public GameObject torchDepleteScares;

    private void Start()
    {
        audioSource = this.GetComponent<AudioSource>();

        // Initialize audio clips
        audioClips = new Dictionary<string, AudioClip>
        {
            { "audioClip1", audioClip1 },
            { "audioClip2", audioClip2 },
            { "audioClip3", audioClip3 },
            { "audioClip4", audioClip4 },
        };

        // Countdown to the first scare
        scareTimer = 50.0f;
        StartCoroutine(NextScare());
    }

    private IEnumerator NextScare()
    {
        // Time to wait until next scare plays
        yield return new WaitForSeconds(scareTimer);

        // Choose a random scare (different from the most recent one played)
        do
        {
            randomScareNo = new System.Random().Next(0, noOfScares);
        }
        while (randomScareNo == previousRandomScare);

        switch (randomScareNo)
        {
            case 0:
                StartCoroutine(PlayScareSound("audioClip1"));
                break;
            case 1:
                StartCoroutine(PlayScareSound("audioClip2"));
                break;
            case 2:
                StartCoroutine(PlayScareSound("audioClip3"));
                break;
            case 3:
                StartCoroutine(PlayScareSound("audioClip4"));
                break;
            default:
                break;
        }

    }


    // Play sound effect for a given scare
    private System.Collections.IEnumerator PlayScareSound(string scareName)
    {
        if (audioClips.ContainsKey(scareName))
        {
            if (!PlayerMove.isColliding && !PlayerMove.isEnteringActionPoint && !PlayerMove.isLookingAround && !PlayerMove.isViewingActionPoint && !PlayerMove.stopPlayer && !PlayerMove.killPlayer && !notCaughtScares.GetComponent<NotCaughtScares>().enabled && !caughtScares.GetComponent<CaughtScares>().enabled && !torchDepleteScares.GetComponent<TorchDepleteScares>().enabled)
            {
                audioSource.clip = audioClips[scareName];
                audioSource.Play();
                previousRandomScare = randomScareNo;
            }

            yield return new WaitUntil(() => !audioSource.isPlaying);
        }
        else
        {
            Debug.LogWarning($"Scare audio clip for {scareName} not found!");
        }

        // Begin countdown till next random jump scare
        scareTimer = new System.Random().Next((int)minTimeTillNextScare, (int)maxTimeTillNextScare);
        StartCoroutine(NextScare());
    }
}
