using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameFinishPad : MonoBehaviour
{

    // Start is called before the first frame update
    void Start()
    {
        this.gameObject.GetComponent<MeshRenderer>().enabled = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            // No recurring collisions causing weird behavior
            this.gameObject.GetComponent<BoxCollider>().enabled = false;
            StartCoroutine(FinishGame());
        }
    }

    private IEnumerator FinishGame()
    {
        PlayerMove.isColliding = true;
        yield return new WaitUntil(() => (!PlayerMove.isColliding));
        PlayerMove.stopPlayer = true;

        // Play end scene
        yield return new WaitForSeconds(3.0f);
        print("You survived! Starting end scene");
        SceneManager.LoadScene("MainMenu");   // Load end scene
    }

}
