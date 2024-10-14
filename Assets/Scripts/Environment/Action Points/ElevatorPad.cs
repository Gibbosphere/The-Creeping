using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ElevatorPad : MonoBehaviour
{
    public GameObject player;
    public GameObject entireActionPoint;

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Entered elevator");

        // No reoccuring collisions causing weird stuff
        this.gameObject.GetComponent<Collider>().enabled = false;

        if (other.gameObject.CompareTag("Player"))
        {
            // Turn player around to face doors
            PlayerMove.isTurningCorner = true;
            player.GetComponent<PlayerMove>().targetRotation = Quaternion.LookRotation(Quaternion.Euler(0, 180, 0) * PlayerMove.fixedForwardDirection);

            // Player Movement while in the elevator
            StartCoroutine(StopWaitRun());
        }
    }

    private IEnumerator StopWaitRun()
    {
        // Wait until the player has turned to face the elevator doors before stopping them
        yield return new WaitUntil(() => !PlayerMove.isTurningCorner);
        PlayerMove.stopPlayer = true;

        // Wait for the elevator door to close before moving the player and elevator (entireActionPoint) down
        yield return new WaitForSeconds(8.5f);

        // Move the player and the entire elevator down over 26 seconds
        float duration = 18.0f;  // Duration of the descent
        float elapsedTime = 0.0f;

        // Store the initial and final positions for smooth movement
        Vector3 initialAPPosition = entireActionPoint.transform.position;
        Vector3 targetAPPosition = initialAPPosition + new Vector3(0, -10.0f, 0);  // Move down by 10 units 
        Vector3 initialPlayerPosition = player.transform.position;
        Vector3 targetPlayerPosition = initialPlayerPosition + new Vector3(0, -10.0f, 0);  // Move down by 10 units

        // Move the entireActionPoint (which includes the player) down
        while (elapsedTime < duration)
        {
            // Lerp both the player and elevator (entireActionPoint) downwards over time
            entireActionPoint.transform.position = Vector3.Lerp(initialAPPosition, targetAPPosition, elapsedTime / duration);
            player.transform.position = Vector3.Lerp(initialPlayerPosition, targetPlayerPosition, elapsedTime / duration);

            elapsedTime += Time.deltaTime;  // Increment elapsed time
            yield return null;  // Wait for the next frame
        }

        // Ensure the final position is reached (in case of any precision issues)
        entireActionPoint.transform.position = targetAPPosition;
        player.transform.position = targetPlayerPosition;

        // Wait for the elevator to open before allowing the player to move again
        yield return new WaitForSeconds(6);

        // Finally, let the player run onto the next floor
        PlayerMove.stopPlayer = false;
    }



}
