using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class ObstacleCollision : MonoBehaviour
{
    public GameObject handleJumpScares;
    public GameObject player;


    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            // No recurring collisions causing weird behavior
            this.gameObject.GetComponent<BoxCollider>().enabled = false; 

            PlayerMove.isColliding = true;

            // Sound effect of player colliding grunt
            player.GetComponent<PlayerCollisionGrunt>().PlayGrunt();

            // Remove a player life
            other.gameObject.GetComponent<PlayerHealth>().RemovePlayerLife();

            // Begin a jump scare
            if (other.gameObject.GetComponent<PlayerHealth>().GetPlayerLives() <= 0) 
            {
                // If the player has no lives left and is now going to be caught, activate a Caught jump scare
                if (!CaughtScares.runTowardsAtNextTurnActive)
                {
                    handleJumpScares.GetComponent<HandleJumpScares>().CaughtScare();
                }
            }
            else
            {
                handleJumpScares.GetComponent<HandleJumpScares>().NotCaughtScare();
            }

        // Start the disable and re-enable sequence using a coroutine
        StartCoroutine(DisableAndEnableObstacle(1.5f));
        }
    }

    private IEnumerator DisableAndEnableObstacle(float disableTime)
    {
        // Disable obstacle after a moment so not in the way of the jump scare
        yield return new WaitForSeconds(disableTime);
        this.gameObject.GetComponent<MeshRenderer>().enabled = false;  // Hide the object from view

        // Wait until the player stops looking around
        yield return new WaitUntil(() => !PlayerMove.isLookingAround);
        yield return new WaitForSeconds(3.0f);

        // Re-enable the obstacle to work again for possible future collisions
        this.gameObject.GetComponent<MeshRenderer>().enabled = true;
        this.gameObject.GetComponent<BoxCollider>().enabled = true;  
    }
}
