using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OpenDoor : MonoBehaviour, IOpenable
{

    public void Open(GameObject currentHeldObjectParent)
    {
        // Play using
        currentHeldObjectParent.transform.GetChild(0).GetComponent<PickupObjectSoundManager>().PlayActionPointSound();

        // Remove object used to open action point
        StartCoroutine(RemoveUsedObject(currentHeldObjectParent));

        // Play opening animation
        StartCoroutine(OpenDoorAnimation());

        // Open door sound
        this.GetComponent<AudioSource>().Play();
    }

    private IEnumerator OpenDoorAnimation()
    {
        yield return new WaitForSeconds(1.0f);  // Wait for the specified delay
        this.GetComponent<Animator>().Play("open");
    }

    private IEnumerator RemoveUsedObject(GameObject currentHeldObjectParent)
    {
        // Wait until the sound finshed playing
        yield return new WaitUntil(() => !currentHeldObjectParent.transform.GetChild(0).GetComponent<PickupObjectSoundManager>().actionPointSound.isPlaying);
        currentHeldObjectParent.transform.SetParent(null);
        Destroy(currentHeldObjectParent);
    }


}
