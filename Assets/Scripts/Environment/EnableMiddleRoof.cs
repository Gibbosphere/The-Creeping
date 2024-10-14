using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnableMiddleRoof : MonoBehaviour
{
    public GameObject middleRoof;

    // Start is called before the first frame update
    void Start()
    {
        middleRoof.SetActive(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            middleRoof.SetActive(true);
        }
    }

}
