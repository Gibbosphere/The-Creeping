using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Balloon : MonoBehaviour
{
    public float pushForce = 5;

    public void Push(Vector3 pushDirection) { 
        Rigidbody rigidbody = this.GetComponent<Rigidbody>();

        // Apply the push force
        Debug.Log(pushDirection * pushForce);
        rigidbody.AddForce(pushDirection * pushForce, ForceMode.Impulse);
    }

    public void Pop()
    {
        // Pop sound
        this.gameObject.GetComponent<AudioSource>().Play();

        // Destroy Balloon
        this.gameObject.GetComponent<MeshRenderer>().enabled = false;

        // Remove object entirely
        StartCoroutine(RemoveGameObject()); 
    }

    private System.Collections.IEnumerator RemoveGameObject()
    {
        // Give a delay to ensure audio plays completely
        yield return new WaitForSeconds(1.5f);
        
        Destroy(this.gameObject);
    }
}
