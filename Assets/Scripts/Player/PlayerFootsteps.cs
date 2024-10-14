using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerFootsteps : MonoBehaviour
{
    public GameObject footstep;
    private float originalVolume;
    AudioSource footstepSound;

    // Start is called before the first frame update
    void Start()
    {
        footstepSound = footstep.GetComponent<AudioSource>();
        originalVolume = footstepSound.volume;
        StartCoroutine(StopFootsteps(0));
    }

    private void Update()
    {
        if (PlayerMove.isColliding || PlayerMove.isLookingAround || PlayerMove.isViewingActionPoint || PlayerMove.killPlayer || PlayerMove.stopPlayer || PlayerMove.isEnteringActionPoint)
        {
            if (footstep.activeSelf) { 
                StartCoroutine(StopFootsteps(1f));
            }
        }
        else
        {
            if (!footstep.activeSelf)
            {
                StartFootsteps();
            }
        }
    }

    public void StartFootsteps()
    {
        footstepSound.volume = originalVolume;
        footstep.SetActive(true);
    }

    private System.Collections.IEnumerator StopFootsteps(float fadeDuration)
    {
        // Gradually decrease the audio volume over the specified duration
        float startVolume = footstepSound.volume;
        float elapsedTime = 0f;

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            footstepSound.volume = Mathf.Lerp(startVolume, 0f, elapsedTime / fadeDuration);
            yield return null;
        }

        // Ensure volume is set to 0 after fading out
        footstepSound.volume = 0f;
        footstepSound.Stop();

        footstep.SetActive(false);
    }
}
