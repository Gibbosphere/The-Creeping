using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class ActionPoint : MonoBehaviour
{
    public bool isOpen = false;
    public GameObject requiredObject;

    public void TryToEnter(Collider player, bool left)
    {
        //if (isOpen)
        //{
        //    // This won't be here if you dont want a player to go back up a level
        //    EnterActionPoint(player, left);
        //    return;
        //}
        isOpen = PlayerPickup.currentHeldObjectParent == requiredObject;

        if (isOpen)
        {
            PlayerPickup.currentHeldObjectParent.transform.SetParent(null);
            EnterActionPoint(player, left);
        }
        else
        {
            ViewActionPoint();
        }
    }

    private void EnterActionPoint(Collider player, bool left)
    {
        // Get the PlayerMove component and turn the player
        PlayerMove playerMove = player.gameObject.GetComponent<PlayerMove>();
        if (playerMove != null)
        {
            // Next level means increase player speed
            playerMove.forwardSpeed += 2;

            // Turn player 90 degrees to the left/right (relative to current forward)
            PlayerMove.isEnteringActionPoint = true;
            playerMove.targetRotation = Quaternion.LookRotation(Quaternion.Euler(0, left ? -90 : 90, 0) * PlayerMove.fixedForwardDirection);
        }

        // Open the action point
        IOpenable openable = GetComponent<IOpenable>();
        if (openable != null)
        {
            openable.Open(PlayerPickup.currentHeldObjectParent);
        }
    }

    private void ViewActionPoint()
    {
        // Make player look at action point for a moment
        //PlayerMove.ActionPointPos = transform.position;
        PlayerMove.ActionPointPos = transform.TransformPoint(Vector3.zero); // Assuming the door is at the parent's origin
        PlayerMove.isViewingActionPoint = true;
    }
}
