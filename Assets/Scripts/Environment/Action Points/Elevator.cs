using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Elevator : MonoBehaviour, IOpenable 
{
    public Material elevatorLights;

    public void Open(GameObject currentHeldObjectParent)
    {
        // Play using object sound
        currentHeldObjectParent.transform.GetChild(0).GetComponent<PickupObjectSoundManager>().PlayActionPointSound();

        // Remove object used to open action point
        StartCoroutine(RemoveUsedObject(currentHeldObjectParent));

        // Powerup elevator
        StartCoroutine(PowerupElevator());
    }


    private IEnumerator PowerupElevator()
    {
        // Allow time to turn the elevator lights on
        PlayerMove.watchActionPointTimer = 10.0f;

        // Play Elevator Powerup sound
        this.GetComponents<AudioSource>()[0].Play();

        // Elevator lights
        yield return new WaitForSeconds(3.5f);
        elevatorLights.EnableKeyword("_EMISSION");

        // Play opening animation
        yield return new WaitForSeconds(0.5f);
        StartCoroutine(OpenCloseElevatorAnimation());

        // Full elevator ride
        this.GetComponents<AudioSource>()[1].Play();
    }

    private IEnumerator RemoveUsedObject(GameObject currentHeldObjectParent)
    {
        // Wait until the sound finshed playing
        yield return new WaitUntil(() => !currentHeldObjectParent.transform.GetChild(0).GetComponent<PickupObjectSoundManager>().actionPointSound.isPlaying);
        currentHeldObjectParent.transform.SetParent(null);
        Destroy(currentHeldObjectParent);
    }

    private IEnumerator OpenCloseElevatorAnimation()
    {
        Animator elevatorAnimator = this.GetComponent<Animator>();

        // 1. Play the animation for the first time
        elevatorAnimator.Play("openclose");
        yield return new WaitForSeconds(14.5f);  // Wait for the animation to complete

        // 2. Stop the animation (otherwise it just loops)
        elevatorAnimator.Play("idle");

        // 3. Keep doors closed while the elevator moves down
        yield return new WaitForSeconds(21.0f);

        // 4. Play the animation when elevator gets to next floor
        elevatorAnimator.Play("openclose");

        yield return new WaitForSeconds(14.0f);  // Wait for the animation to complete

        // 5. Keep doors closed forever now
        elevatorAnimator.Play("idle");
    }
}
