using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class StartPad : MonoBehaviour
{
    private Coroutine deactivateRoutine;

    public GameObject decisionPadLeft;
    public GameObject decisionPadRight;
    public GameObject decisionPadBack;

    private void Start()
    {
        this.gameObject.GetComponent<MeshRenderer>().enabled = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        // Activate all associated decisionPads
        ActivateDecisionPads();

        // Capture the player's position at this turning point (helps jumpscares)
        PlayerPositioning.previousTurnPosition = transform.position;
        PlayerPositioning.rightTurn = decisionPadRight != null;
        PlayerPositioning.leftTurn = decisionPadLeft != null;

        // Stop any ongoing deactivation process (if the player re-enters the pad)
        if (deactivateRoutine != null)
        {
            StopCoroutine(deactivateRoutine);
            deactivateRoutine = null;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        // Capture the player's position at this turning point (helps jumpscares)
        PlayerPositioning.previousTurnPosition = transform.position;
        PlayerPositioning.rightTurn = decisionPadRight != null;
        PlayerPositioning.leftTurn = decisionPadLeft != null;

        // Start the coroutine to deactivate the pads after a delay
        deactivateRoutine = StartCoroutine(DeactivateDecisionPadsAfterDelay(3.0f));
    }

    private void ActivateDecisionPads()
    {
        if (decisionPadLeft != null)
        {
            decisionPadLeft.gameObject.SetActive(true);
        }
        if (decisionPadRight != null)
        {
            decisionPadRight.gameObject.SetActive(true);
        }
        if (decisionPadBack != null)
        {
            decisionPadBack.gameObject.SetActive(true);
        }
    }

    private IEnumerator DeactivateDecisionPadsAfterDelay(float delay)
    {
        // Wait for the specified delay time
        yield return new WaitForSeconds(delay);

        // Deactivate the associated decision pads
        if (decisionPadLeft != null)
        {
            decisionPadLeft.SetActive(false);
        }
        if (decisionPadRight != null)
        {
            decisionPadRight.SetActive(false);
        }
        if (decisionPadBack != null)
        {
            decisionPadBack.SetActive(false);
        }

        // Clear the coroutine reference
        deactivateRoutine = null;
    }


}
