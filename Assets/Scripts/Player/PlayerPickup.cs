using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class PlayerPickup : MonoBehaviour
{
    public Transform heldObjectPosition;  // The position where the object will be (in comparison to body)
    public static GameObject currentHeldObjectParent = null;  // the parent of the pickup object including the object and charge light
    private GameObject currentHeldObject = null;  // currently held object
    private GameObject chargeLightObject;  // charge light

    private Quaternion originalObjectRotation;
    private float originalObjectPosY;

    public float dropColliderDelay = 2.0f;  // Time in seconds before collider re-enables after dropping object (prevent immediate pickup again)

    public GameObject soundObject; // Reference to the GameObject holding the SoundManager script
    private PickupObjectSoundManager soundManager;


    private void OnTriggerEnter(Collider other)
    {
        // Check if the object has the tag "PickUp"
        if (other.CompareTag("PickUp"))
        {
            GameObject parentObject = other.transform.parent.gameObject;

            // If holding something already, drop it first
            if (currentHeldObject != null)
            {
                DropObject();
            }

            PickupObject(parentObject);
        }
    }

    private void PickupObject(GameObject parentObject)
    {
        // Set the parent, pickup object, and charge light as the newly held object
        currentHeldObjectParent = parentObject;
        currentHeldObject = parentObject.transform.GetChild(0).gameObject;
        chargeLightObject = parentObject.transform.GetChild(1).gameObject;

        // Store original position and rotation to restore when dropped again
        originalObjectRotation = currentHeldObjectParent.transform.rotation;
        originalObjectPosY = currentHeldObjectParent.transform.position.y;

        // Stop rotation of the held object
        //currentHeldObject.GetComponent<RotateObject>().enabled = false;

        // Play the pickup object sound
        currentHeldObject.GetComponent<PickupObjectSoundManager>().PlayPickUpSound();

        // Set its Rigidbody to Kinematic (so it doesn't fall or move)
        Rigidbody rb = currentHeldObject.GetComponent<Rigidbody>();
        if (rb != null)
            rb.isKinematic = true;

        // Disable the collider of the object (no longer interacts with the world)
        currentHeldObject.GetComponent<Collider>().enabled = false;

        // Deactivate the charge light
        chargeLightObject.SetActive(false);

        // Parent it to the HeldObjectPosition and reset object's position and rotation
        currentHeldObjectParent.transform.SetParent(heldObjectPosition);
        //currentHeldObject.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
        currentHeldObjectParent.transform.localPosition = Vector3.zero;

        // Fine-tune the local position
        //currentHeldObjectParent.transform.localPosition = new Vector3(-0.92f, -0.52f * (PlayerMove.fixedRotationY == 180 || PlayerMove.fixedRotationY == -90 ? -1 : 1), 0.8f);  // offset for positioning the object a bit forward
        currentHeldObjectParent.transform.localPosition = new Vector3(-0.72f, -0.52f, 0.8f);  // offset for positioning the object a bit forward
        currentHeldObjectParent.transform.localRotation = Quaternion.identity;  // Ensure the object faces a consistent direction
    }

    private void DropObject()
    {
        // Remove it from the parent (so it's no longer attached to the player)
        currentHeldObjectParent.transform.SetParent(null);

        // Reactivate physics and any rotation logic on the parent
        Rigidbody rb = currentHeldObject.GetComponent<Rigidbody>();
        if (rb != null)
            rb.isKinematic = false;
        //currentHeldObject.GetComponent<RotateObject>().enabled = true;

        // Drop it at the player's feet
        currentHeldObjectParent.transform.position = new Vector3(transform.position.x, originalObjectPosY, transform.position.z);
        currentHeldObjectParent.transform.rotation = originalObjectRotation;

        // Reactivate the charge light when parent is dropped
        if (chargeLightObject != null)
        {
            chargeLightObject.SetActive(true);
        }

        // Start the coroutine to enable the collider after a delay
        StartCoroutine(EnableColliderAfterDelay(currentHeldObject));

        // Clear the currentHeldObject references
        currentHeldObjectParent = null;
        currentHeldObject = null;
        chargeLightObject = null;
    }

    // Coroutine to enable the collider after a short delay
    private IEnumerator EnableColliderAfterDelay(GameObject droppedObject)
    {
        yield return new WaitForSeconds(dropColliderDelay);  // Wait for the specified delay

        // Enable the collider again
        Collider objectCollider = droppedObject.GetComponent<Collider>();
        if (objectCollider != null)
        {
            objectCollider.enabled = true;
        }
    }
}

