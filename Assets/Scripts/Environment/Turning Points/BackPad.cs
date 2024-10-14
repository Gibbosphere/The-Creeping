using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackPad : MonoBehaviour
{
    public GameObject leftPad;
    public GameObject rightPad;

    // Start is called before the first frame update
    void Start()
    {
        this.gameObject.SetActive(false);
        this.gameObject.GetComponent<MeshRenderer>().enabled = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Activating Back Pad");

        // Capture the player's position at this turning point (helps jumpscares)
        PlayerPositioning.previousDecisionPosition = transform.position;
        PlayerPositioning.leftDecisionTurn = true;

        // No reoccuring collisions causing weird stuff
        this.gameObject.SetActive(false);
        leftPad.gameObject.SetActive(false);
        rightPad.gameObject.SetActive(false);

        if (other.gameObject.CompareTag("Player"))
        {
            // Get the PlayerMove component and turn the player
            PlayerMove playerMove = other.gameObject.GetComponent<PlayerMove>();
            if (playerMove != null)
            {
                // Turn player 90 degrees to the left (relative to current forward)
                //Vector3 newDirection = Quaternion.Euler(0, -90, 0) * playerMove.fixedForwardDirection;
                //playerMove.TurnCorner(newDirection, true);
                PlayerMove.isTurningCorner = true;
                playerMove.targetRotation = Quaternion.LookRotation(Quaternion.Euler(0, -90, 0) * PlayerMove.fixedForwardDirection);
            }
        }
    }
}