using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RightPad : MonoBehaviour
{
    public GameObject backPad;

    // Start is called before the first frame update
    void Start()
    {
        this.gameObject.SetActive(false);
        this.gameObject.GetComponent<MeshRenderer>().enabled = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Activating Right Pad");

        // Capture the player's position at this turning point (helps jumpscares)
        PlayerPositioning.previousDecisionPosition = transform.position;
        PlayerPositioning.leftDecisionTurn = false;

        // No reoccuring collisions causing weird stuff
        this.gameObject.SetActive(false);
        if (backPad != null)
        {
            backPad.gameObject.SetActive(false);
        }

        if (other.gameObject.CompareTag("Player"))
        {
            // Get the PlayerMove component and turn the player
            PlayerMove playerMove = other.gameObject.GetComponent<PlayerMove>();
            if (playerMove != null)
            {
                // Turn player 90 degrees to the right (relative to current forward)
                PlayerMove.isTurningCorner = true;
                playerMove.targetRotation = Quaternion.LookRotation(Quaternion.Euler(0, 90, 0) * PlayerMove.fixedForwardDirection);
            }
        }
    }
}
