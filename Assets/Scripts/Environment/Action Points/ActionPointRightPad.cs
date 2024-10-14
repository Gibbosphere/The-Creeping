using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActionPointRightPad : MonoBehaviour
{
    public GameObject actionPoint;

    // Start is called before the first frame update
    void Start()
    {
        this.gameObject.SetActive(false);
        this.gameObject.GetComponent<MeshRenderer>().enabled = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Activating Right AP Pad");

        // No reoccuring collisions causing weird stuff
        this.gameObject.SetActive(false);

        if (other.gameObject.CompareTag("Player"))
        {
            actionPoint.GetComponent<ActionPoint>().TryToEnter(other, false);
        }
    }
}
