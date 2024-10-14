using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BatteryPickup : MonoBehaviour
{
    public float respawnDelay = 30f;
    public GameObject torch;
    public GameObject chargeLight; // lights up the battery

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            // No recurring collisions causing weird behavior
            this.gameObject.GetComponent<BoxCollider>().enabled = false;
            
            // Appear to remove battery (and chargeLight) for now - will revive later
            this.gameObject.GetComponent<MeshRenderer>().enabled = false;
            chargeLight.SetActive(false);

            // Play battery pickup sound 
            this.gameObject.GetComponent<AudioSource>().Play();

            // Increase the torch battery
            torch.GetComponent<TorchManagement>().IncreaseBattery();

            // Start count to revive/respawn battery
            StartCoroutine(RespawnBattery(respawnDelay));
        }
    }

    private IEnumerator RespawnBattery(float reviveDelay)
    {
        yield return new WaitForSeconds(reviveDelay);

        // Respawn the battery (and its charge light)
        this.gameObject.GetComponent<MeshRenderer>().enabled = true;
        this.gameObject.GetComponent<BoxCollider>().enabled = true;

        chargeLight.SetActive(true);
    }

}
