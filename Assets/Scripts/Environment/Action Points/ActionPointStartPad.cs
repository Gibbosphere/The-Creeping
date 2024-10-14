using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class ActionPointStartPad : MonoBehaviour
{
    private Coroutine deactivateRoutine;
    public GameObject decisionPad;

    private void Start()
    {
        this.gameObject.GetComponent<MeshRenderer>().enabled = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        // Activate associated decision pad
        ActivateDecisionPad();

        // Stop any ongoing deactivation process (if the player re-enters the pad)
        if (deactivateRoutine != null)
        {
            StopCoroutine(deactivateRoutine);
            deactivateRoutine = null;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        // Start the coroutine to deactivate the pads after a delay
        deactivateRoutine = StartCoroutine(DeactivateDecisionPadAfterDelay(8.0f));
    }

    private void ActivateDecisionPad()
    {
        if (decisionPad != null)
        {
            decisionPad.gameObject.SetActive(true);
        }
    }

    private IEnumerator DeactivateDecisionPadAfterDelay(float delay)
    {
        // Wait for the specified delay time
        yield return new WaitForSeconds(delay);

        // Deactivate the associated decision pads
        if (decisionPad != null)
        {
            decisionPad.gameObject.SetActive(false);
        }

        // Clear the coroutine reference
        deactivateRoutine = null;
    }


}
