using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wheelchair : MonoBehaviour
{
    public float pushForce = 5;
    public AudioSource wheelchairSound;

    // Start is called before the first frame update
    void Start()
    {

    }

    public void Push(Vector3 pushDirection) { 
        Rigidbody rigidbody = this.GetComponent<Rigidbody>();

        // Apply the push force
        Debug.Log(pushDirection * pushForce);
        rigidbody.AddForce(pushDirection * pushForce, ForceMode.Impulse);

        // Play the wheelchair sound
        WheelchairSound();
    }

    public void WheelchairSound()
    {
        this.gameObject.GetComponent<AudioSource>().Play();
    }
}
