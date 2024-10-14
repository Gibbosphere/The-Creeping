using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    private int playerLives = 3;
    
    public int GetPlayerLives()
    {
        return playerLives;
    }

    public void RemovePlayerLife()
    {
        playerLives--;
        Debug.Log("Removing player life. " + playerLives + " lives left.");
    }
}
