using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LevelEnder : MonoBehaviour
{
    public Image fadeImage;     // Reference to the black screen
    //public AudioSource openingSound;
    private float fadeDuration = 3f;  // Duration of the fade

    void Start()
    {
        fadeImage.color = new Color(0, 0, 0, 0);  // Start fully transparent black
    }

    public IEnumerator EndLevel(bool survived)
    {
        // Hold the normal screen for a moment before fading black
        yield return new WaitForSeconds(0.5f);

        float elapsedTime = 0f;

        // Gradually increase the alpha value of the image
        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Clamp01(elapsedTime / fadeDuration); 
            fadeImage.color = new Color(0, 0, 0, alpha);
            yield return null;
        }

        // Hold the full black screen for a moment before ending
        yield return new WaitForSeconds(3.5f);

        // Change the scene
        if (survived)
        {
            print("You survived! Starting end scene");
            SceneManager.LoadScene("EndScene");   // Load end scene
        }
        else
        {
            print("You died! Starting try again scene");
            SceneManager.LoadScene("GameOver");   // Load try again menu scene
        }
    }

}
