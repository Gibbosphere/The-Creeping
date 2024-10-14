using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum Difficulty { Easy, Medium, Hard }

public class Menu : MonoBehaviour
{
    public TextMeshProUGUI difficultyText;
    Difficulty currentDifficulty;

    void Start()
    {
        currentDifficulty = Difficulty.Easy;
    }


    public void ChangeToNextDifficulty()
    {
        currentDifficulty++;
        if (currentDifficulty > Difficulty.Hard)
        {
            currentDifficulty = Difficulty.Easy; // Wrap around to Easy
        }

        difficultyText.text = $"Difficulty: {currentDifficulty}";
    }



    public void QuitGame(){
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
     }

    public void Play(){
        SceneManager.LoadScene("Level1");
    }

    public void Controls(){
        SceneManager.LoadScene("Controls");
    }

}
