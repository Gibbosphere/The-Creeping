using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LeftPad : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        this.gameObject.SetActive(false);
        this.gameObject.GetComponent<MeshRenderer>().enabled = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Activating Left Pad");

        // Capture the player's position at this turning point (helps jumpscares)
        PlayerPositioning.previousDecisionPosition = transform.position;
        PlayerPositioning.leftDecisionTurn = true;

        // No reoccuring collisions causing weird stuff
        this.gameObject.SetActive(false);

        if (other.gameObject.CompareTag("Player"))
        {
            // Get the PlayerMove component and turn the player
            PlayerMove playerMove = other.gameObject.GetComponent<PlayerMove>();
            if (playerMove != null)
            {
                // Turn player 90 degrees to the left (relative to current forward)
                PlayerMove.isTurningCorner = true;
                playerMove.targetRotation = Quaternion.LookRotation(Quaternion.Euler(0, -90, 0) * PlayerMove.fixedForwardDirection);
            }
        }
    }
}
