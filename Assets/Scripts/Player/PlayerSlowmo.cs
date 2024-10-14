using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSlowmo : MonoBehaviour
{
    public GameObject slowDown;
    public GameObject speedUp;
    AudioSource slowDownSound;
    AudioSource speedUpSound;

    // Start is called before the first frame update
    void Start()
    {
        slowDownSound = slowDown.GetComponent<AudioSource>();
        speedUpSound = speedUp.GetComponent<AudioSource>();
    }

    public void PlaySpeedUp()
    {
        speedUpSound.Play();
    }
    public void PlaySlowDown()
    {
        slowDownSound.Play();
    }


}
