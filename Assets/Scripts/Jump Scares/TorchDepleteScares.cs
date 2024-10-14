using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class TorchDepleteScares : MonoBehaviour
{
    // Monster for jump scares
    public GameObject monster;
    private GameObject spawnedMonster;
    private float runSpeed;

    // Torch for jump scares
    public GameObject torch;

    // Player
    public GameObject player;
    private Vector3 playerPos;
    public static int noOfScares = 1;

    // Which jump scare is active
    private bool flickerAndRunTowardsActive = false;

    // Audio management
    public AudioSource audioSource;  // The single AudioSource used for playing different sounds

    public AudioClip finalScream1;
    public AudioClip finalScream2;
    public AudioClip finalScream3;

    private Dictionary<string, AudioClip> audioClips; // Dictionary to store different audio clips



    // Start is called before the first frame update
    void Start()
    {
        // Initialize audio clips
        audioClips = new Dictionary<string, AudioClip>
        {
            { "final_scream_1", finalScream1 },
            { "final_scream_2", finalScream2 },
            { "final_scream_3", finalScream3 },
        };

        // Ensure the audio source is not playing initially
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        audioSource.loop = false;

        // Only need to enable when obstacle collision happens
        this.enabled = false;
    }

    public void ChooseJumpScare(int jumpScareNo)
    {
        switch (jumpScareNo)
        {
            case 0:
                StartCoroutine(FlickerAndRunTowards());
                break;
            default:
                Debug.Log("Invalid jumpscare number");
                break;
        }
    }

    // Play sound effect for a given scare
    private IEnumerator PlayScareSound(string scareName)
    {
        if (audioClips.ContainsKey(scareName))
        {
            audioSource.clip = audioClips[scareName];
            audioSource.Play();

            yield return new WaitUntil(() => !audioSource.isPlaying);
            this.enabled = false;
        }
        else
        {
            Debug.LogWarning($"Scare audio clip for {scareName} not found!");
        }
    }
    // Randomizing which monster scream
    public void PlayMonsterScream(string screamNo)
    {
        if (screamNo == "-1")
        {
            string randomScreamSound = "final_scream_" + new System.Random().Next(1, 4);
            StartCoroutine(PlayScareSound(randomScreamSound));
        }
        else
        {
            StartCoroutine(PlayScareSound("final_scream_" + screamNo));
        }
    }


    private float lightOffTimer = 4f;

    private IEnumerator FlickerAndRunTowards()
    {
        MonsterHandler.fromInFrontScare = true;
        flickerAndRunTowardsActive = true;
        PlayerMove.isColliding = true; // stop player slowly

        // Wait until player has stopped colliding to stop them, then flicker torch + turn off lights
        yield return new WaitUntil(() => !PlayerMove.isColliding);
        // Stop player
        PlayerMove.stopPlayer = true;
        //torch.GetComponent<TorchManagement>().IncreaseBattery();
        StartCoroutine(torch.GetComponent<TorchManagement>().Flicker());
        yield return new WaitForSeconds(2.5f);
        torch.GetComponent<TorchManagement>().turnOff();

        // Keep dark for a few seconds 
        yield return new WaitForSeconds(lightOffTimer);

        // Spawn Monster
        Vector3 spawnPosition = PlayerPositioning.obstacleHitPosition + 10 * PlayerMove.fixedForwardDirection;
        spawnPosition.y -= 1.4f;
        spawnedMonster = Instantiate(monster, spawnPosition, Quaternion.Euler(0, 180 - PlayerMove.fixedRotationY, 0));
        runSpeed = 20f;
        playerPos = player.transform.position + 1 * PlayerMove.fixedForwardDirection;
        playerPos.y -= 1.4f;

        // Turn on the lights and send that monster
        yield return new WaitForSeconds(2);
        torch.GetComponent<TorchManagement>().IncreaseBattery();  
        torch.GetComponent<TorchManagement>().IncreaseBattery(); 
        torch.GetComponent<TorchManagement>().turnOn();   // Turn torch on 
        yield return new WaitForSeconds(0.5f);

        // Make monster run
        PlayMonsterScream("2");  // Start scream sound
        MonsterHandler monsterHandler = spawnedMonster.GetComponent<MonsterHandler>();
        if (monsterHandler != null)
        {
            StartCoroutine(WaitForInitializationThenRun(monsterHandler, playerPos, runSpeed));
        }
        flickerAndRunTowardsActive = false;

    }
    private IEnumerator WaitForInitializationThenRun(MonsterHandler handler, Vector3 targetPosition, float speed)
    {
        // Wait until the audioSource or other components are set up
        while (handler.audioSource == null)
        {
            yield return null; // Wait for the next frame
        }

        handler.RunTowardsPlayer(targetPosition, speed);
    }
}
