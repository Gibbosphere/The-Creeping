using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LevelStarter : MonoBehaviour
{
    public Image fadeImage;     // Reference to the black fade in screen
    public AudioSource openingSound;
    private float fadeDuration = 5f;  // Duration of the fade (originally 5)

    private void Start()
    {
        // Player not moving
        PlayerMove.stopPlayer = true;

        // Set the initial alpha to fully opaque (black)
        fadeImage.color = new Color(0, 0, 0, 1);

        // Play eerie opening music
        StartCoroutine(OpeningMusic(7, 4));

        // Start the fade-in effect
        StartCoroutine(FadeInScreen());
    }

    private System.Collections.IEnumerator OpeningMusic(float playDuration, float fadeDuration)
    {
        // Play the sound
        openingSound.Play();
        yield return new WaitForSeconds(playDuration);

        // Fade music out
        StartCoroutine(FadeOutMusic(fadeDuration));
    }

    private System.Collections.IEnumerator FadeOutMusic(float fadeDuration)
    {
        // Gradually decrease the audio volume over the specified duration
        float startVolume = openingSound.volume;
        float elapsedTime = 0f;

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            openingSound.volume = Mathf.Lerp(startVolume, 0f, elapsedTime / fadeDuration);
            yield return null;
        }

        // Ensure volume is set to 0 after fading out
        openingSound.volume = 0f;
        openingSound.Stop();
    }

    private IEnumerator FadeInScreen()
    {
        // Hold the black screen for a moment
        yield return new WaitForSeconds(2.0f);


        float elapsedTime = 0f;

        // Gradually decrease the alpha value of the image
        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Clamp01(1 - (elapsedTime / fadeDuration));
            fadeImage.color = new Color(0, 0, 0, alpha);
            yield return null;
        }

        // Once fade-in is complete, completely disable fade in screen
        fadeImage.gameObject.SetActive(false);

        // Start player running
        StartCoroutine(StartPlayer());
    }

    private IEnumerator StartPlayer()
    {
        // Keep the player still for a moment (while torch flickers)
        yield return new WaitForSeconds(5.0f);

        PlayerMove.stopPlayer = false;
    }

}
