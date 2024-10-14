using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerBreathing : MonoBehaviour
{
    public GameObject breathing;
    private float recentVolume;
    AudioSource breathingSound;

    // Start is called before the first frame update
    void Start()
    {
        breathingSound = breathing.GetComponent<AudioSource>();
        recentVolume = breathingSound.volume;
        StartCoroutine(SetBreathingVolume("high"));
    }

    public void StartBreathing()
    {
        breathingSound.volume = recentVolume;
        breathing.SetActive(true);
    }

    public System.Collections.IEnumerator SetBreathingVolume(string volume = "medium")
    {
        // Ensure player is indeed currently breathing
        StartBreathing();

        float fadeDuration = 1.3f;
        float finalVolume;

        switch (volume)
        {
            case "low":
                finalVolume = 0.3f;
                break;
            case "medium":
                finalVolume = 0.65f;
                break;
            case "high":
                finalVolume = 0.8f;
                break;
            default:
                finalVolume = 0.5f;
                break;
        }

        // Gradually increase/decrease the audio volume over the specified duration
        float startVolume = breathingSound.volume;
        float elapsedTime = 0f;

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            breathingSound.volume = Mathf.Lerp(startVolume, finalVolume, elapsedTime / fadeDuration);
            yield return null;
        }

        // Ensure volume and pitch correctly set
        breathingSound.volume = finalVolume;
        recentVolume = finalVolume;

    }

    public System.Collections.IEnumerator StopBreathing()
    {
        // Gradually decrease the audio volume over the specified duration
        float fadeDuration = 1f;
        float startVolume = breathingSound.volume;
        float elapsedTime = 0f;

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            breathingSound.volume = Mathf.Lerp(startVolume, 0f, elapsedTime / fadeDuration);
            yield return null;
        }

        // Ensure volume is set to 0 after fading out
        breathingSound.volume = 0f;
        breathingSound.Stop();

        breathing.SetActive(false);
    }
}
