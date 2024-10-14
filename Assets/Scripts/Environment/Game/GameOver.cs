using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOver : MonoBehaviour
{
    public void TryAgain()
    {
        Debug.Log("Try Again button pressed");
        SceneManager.LoadScene("Level");
    }

    public void MainMenu()
    {
        Debug.Log("Main Menu button pressed");
        SceneManager.LoadScene("MainMenu");
    }
}
