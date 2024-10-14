using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TorchBatteryManager : MonoBehaviour
{
    public GameObject bar1;
    public GameObject bar2;
    public GameObject bar3;
    public GameObject bar4;

    private Dictionary<int, GameObject> batteryBars;

    // Need to pause battery depletion if a scare is busy happening
    public GameObject handleJumpScares;
    public GameObject notCaughtScares;
    public GameObject caughtScares;
    public GameObject torchDepleteScares;

    private void Start()
    {
        // Dictionary to map bar numbers to their respective GameObjects
        batteryBars = new Dictionary<int, GameObject>
        {
            { 1, bar1 },
            { 2, bar2 },
            { 3, bar3 },
            { 4, bar4 }
        };
    }

    public IEnumerator DecreaseBattery(int lightToPutOff)
    {
        if (batteryBars.ContainsKey(lightToPutOff) && lightToPutOff != 1)
        {
            // Turn off the Mesh Renderer for the specified bar to make it disappear
            batteryBars[lightToPutOff].GetComponent<MeshRenderer>().enabled = false;
        }

        // If you're turing off the last bar, flash continually faster for 20 seconds before finally turning off
        else if (lightToPutOff == 1)
        {
            float flashDuration = 0.9f;  // Initial flash duration
            float timeElapsed = 0f;      // Track total time elapsed
            float maxFlashSpeed = 0.1f;
            bool batteryCollectedInTime = false;  // if a battery is collected during final flashing

            MeshRenderer lastBarRenderer = batteryBars[1].GetComponent<MeshRenderer>();

            while (timeElapsed < 20.0f)
            {
                // Check if a battery has been collected in time (if so, stop countdown)
                if (TorchManagement.batteryBars != 0)
                {
                    batteryCollectedInTime = true;
                    break;
                }

                // Check if we should pause the timer based on player state
                if (PlayerMove.isColliding || PlayerMove.isEnteringActionPoint || PlayerMove.isLookingAround || PlayerMove.isViewingActionPoint || PlayerMove.stopPlayer || PlayerMove.killPlayer ||
                    notCaughtScares.GetComponent<NotCaughtScares>().enabled || caughtScares.GetComponent<CaughtScares>().enabled || torchDepleteScares.GetComponent<TorchDepleteScares>().enabled)
                {
                    // Wait until all the conditions are no longer true
                    yield return null;
                    continue;  // Skip this iteration, don't progress timer or flash
                }

                // Toggle the bar visibility
                lastBarRenderer.enabled = !lastBarRenderer.enabled;

                // Wait for the current flash duration
                yield return new WaitForSeconds(flashDuration);

                // Increase flashing speed
                flashDuration = Mathf.Max(maxFlashSpeed, flashDuration * 0.95f);

                timeElapsed += flashDuration;
            }

            // If a battery was collected in time, exit without turning off the bar
            if (batteryCollectedInTime)
            {
                yield break;
            }

            // Ensure the bar is completely turned off after 20 seconds
            lastBarRenderer.enabled = false;
            // Turn the flashlight off
            handleJumpScares.GetComponent<HandleJumpScares>().TorchDepleteScare();  // Call a torch deplete scare and kill player
        }
    }

    public void IncreaseBattery(int lightToPutOn)
    {
        if (batteryBars.ContainsKey(lightToPutOn))
        {
            // Turn on the Mesh Renderer for the specified bar to make it visible again
            batteryBars[lightToPutOn].GetComponent<MeshRenderer>().enabled = true;
        }
    }
}
