using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

// Very Importantly, not caught scares need to reset the player Move values that they manipulate when they're done
public class NotCaughtScares : MonoBehaviour
{
    // Monster for jump scares
    public GameObject monster;
    private GameObject spawnedMonster;

    // Player
    public GameObject player;
    public static int noOfScares = 6;

    // Audio management
    public AudioSource audioSource;  // The single AudioSource used for playing different sounds
    public AudioClip distantMonsterScreech;
    public AudioClip monsterLaugh1;
    public AudioClip monsterLaugh2;
    public AudioClip monsterRunningSound;

    public AudioClip impactSound1;
    public AudioClip impactSound2;
    public AudioClip impactSound3;
    public AudioClip impactSound4;
    public AudioClip impactSound5;

    private Dictionary<string, AudioClip> audioClips; // Dictionary to store different audio clips

    private void Start()
    {
        // Initialize audio clips
        audioClips = new Dictionary<string, AudioClip>
        {
            { "distant_screech", distantMonsterScreech },
            { "monster_laugh_1", monsterLaugh1 },
            { "monster_laugh_2", monsterLaugh2 },
            { "monster_running", monsterRunningSound },
       
            { "impact_1", impactSound1 },
            { "impact_2", impactSound2 },
            { "impact_3", impactSound3 },
            { "impact_4", impactSound4 },
            { "impact_5", impactSound5 }
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
                StartCoroutine(RunAcrossAdjacentPassage());
                break;
            case 1:
                StartCoroutine(HeadPopOut());
                break;
            case 2:
                Laugh();
                break;
            case 3:
                RunningFootsteps();
                break;
            case 4:
                StartCoroutine(Wheelchair());
                break;
            case 5:
                StartCoroutine(Balloon());
                break;
            default:
                Debug.Log("Invalid jumpscare number");
                break;
        }
    }


    // Play sound effect for a given scare
    private System.Collections.IEnumerator PlayScareSound(string scareName, float delay)
    {
        yield return new WaitForSeconds(delay); // delay sound if need be

        if (audioClips.ContainsKey(scareName))
        {
            // Speed of running
            audioSource.pitch = scareName == "monster_running" ? 1.6f : 1f;

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
    // Randomizing impact sounds
    private void PlayImpactSound(float delay)
    {
        string randomImpactSound = "impact_" + new System.Random().Next(1, 6);
        StartCoroutine(PlayScareSound(randomImpactSound, delay));
    }


    // Monster runs across adjacent passage - when player looking back
    private Vector3 runFinalPos;
    private float runSpeed;
    private IEnumerator RunAcrossAdjacentPassage()
    {
        // Spawning of the monster
        PlayerMove.starebackTimer = 3;  // make player stare back longer

        // If passage goes left and right, make monster run across it
        if (PlayerPositioning.rightTurn && PlayerPositioning.leftTurn)
        {
            Vector3 spawnPosition = PlayerPositioning.previousTurnPosition - 7 * PlayerMove.fixedForwardDirection + 8 * (Quaternion.Euler(0, 90, 0) * PlayerMove.fixedForwardDirection);
            spawnPosition.y -= 1.4f;
            spawnedMonster = Instantiate(monster, spawnPosition, Quaternion.Euler(0, PlayerMove.fixedRotationY - 90, 0));
            runFinalPos = PlayerPositioning.previousTurnPosition - 7 * PlayerMove.fixedForwardDirection + 8 * (Quaternion.Euler(0, -90, 0) * PlayerMove.fixedForwardDirection); // makes monster run perpindicular to player
            runSpeed = 12;
        }
        // If passage only goes one way (right/left), start monster in the center of the passage the player is currently in, facing player
        else
        {
            Vector3 spawnPosition = PlayerPositioning.previousTurnPosition - 7 * PlayerMove.fixedForwardDirection;
            spawnPosition.y -= 1.4f;
            spawnedMonster = Instantiate(monster, spawnPosition, Quaternion.Euler(0, PlayerMove.fixedRotationY, 0));
            runSpeed = 5f;
            Debug.Log("Spawned monster");

            // if passage only goes right, make the monster run out that direction.
            if (PlayerPositioning.rightTurn)
            {
                runFinalPos = PlayerPositioning.previousTurnPosition - 7 * PlayerMove.fixedForwardDirection + 8 * (Quaternion.Euler(0, -90, 0) * PlayerMove.fixedForwardDirection);
            }
            else
            {
                runFinalPos = PlayerPositioning.previousTurnPosition - 7 * PlayerMove.fixedForwardDirection + 8 * (Quaternion.Euler(0, 90, 0) * PlayerMove.fixedForwardDirection);
            }
        }

        // Wait for player to face backwards, then make monster run
        yield return new WaitUntil(() => !PlayerMove.isTurningBackwards && !PlayerMove.isColliding);
        runFinalPos.y -= 1.4f;
        spawnedMonster.GetComponent<MonsterHandler>().Run(runFinalPos, runSpeed);

        PlayImpactSound(0);   // Play an impact sound

        // Remove monster once player facing forward again
        yield return new WaitUntil(() => !PlayerMove.isLookingAround);
        Destroy(spawnedMonster);
        PlayerMove.ResetStareBackTime();

        this.enabled = false;
    }

    // Monster head pops out from adjacent passage wall - when player looking back
    private IEnumerator HeadPopOut()
    {
        PlayerMove.starebackTimer = 3;  // make player stare back longer

        // Calculate monster's spawn position based on whether there is a right/left passage at the most recent turning point
        Vector3 spawnPosition;
        if (PlayerPositioning.rightTurn)
        {
            spawnPosition = PlayerPositioning.previousTurnPosition - 4 * PlayerMove.fixedForwardDirection + 6 * (Quaternion.Euler(0, -90, 0) * PlayerMove.fixedForwardDirection);
            spawnPosition.y -= 1f;
        }
        else
        {
            spawnPosition = PlayerPositioning.previousTurnPosition - 4 * PlayerMove.fixedForwardDirection + 6 * (Quaternion.Euler(0, 90, 0) * PlayerMove.fixedForwardDirection);
            spawnPosition.y -= 1f;
        }

        // Spawn monster
        spawnedMonster = Instantiate(monster, spawnPosition, Quaternion.Euler(0, PlayerMove.fixedRotationY, PlayerPositioning.rightTurn ? -60 : 60));

        // Make monster pop its head out - once player is looking back
        yield return new WaitUntil(() => PlayerMove.isStaringBackwards);
        spawnedMonster.GetComponent<MonsterHandler>().PopOutHead(PlayerPositioning.rightTurn ? true : false);
        PlayImpactSound(0.1f);   // Play an impact sound

        // Remove monster - once player back to normal
        yield return new WaitUntil(() => !PlayerMove.isLookingAround);
        Destroy(spawnedMonster);
        PlayerMove.ResetStareBackTime();

        this.enabled = false;
    }

    // Simply play the monster laughing/screeching audio
    void Laugh()
    {
        int randomSound = new System.Random().Next(1, 4);
        if (randomSound == 1)
        {
            StartCoroutine(PlayScareSound("distant_screech", new System.Random().Next(4, 9)));
        }
        else if (randomSound == 2)
        {
            StartCoroutine(PlayScareSound("monster_laugh_1", new System.Random().Next(4, 9)));
        }
        else if (randomSound == 3)
        {
            StartCoroutine(PlayScareSound("monster_laugh_2", 3.0f));
        }
    }

    // Simply play the running footsteps getting louder audio
    void RunningFootsteps()
    {
        StartCoroutine(PlayScareSound("monster_running", 2.0f));
    }

    // A wheelchair appears and rolls randomly behind you - when player looks back
    public GameObject wheelchair;
    private GameObject spawnedWheelchair;
    public float wheelchairDistanceBehindPlayer = 15;

    private float moveWheelchairTimer = PlayerMove.lookbackTimer - 0.5f; // when wheelchair starts moving
    private float removeWheelchairTimer = PlayerMove.starebackTimer + 3; // when wheelchair starts moving

    private IEnumerator Wheelchair()
    {
        // Calculate spawn position behind the player, and spawn wheelchair
        Vector3 spawnPosition = player.transform.position - PlayerMove.fixedForwardDirection * wheelchairDistanceBehindPlayer;
        spawnedWheelchair = Instantiate(wheelchair, spawnPosition, Quaternion.Euler(0, PlayerMove.fixedRotationY + 90, 0));

        // Timer to initiate wheelchair move
        yield return new WaitForSeconds(moveWheelchairTimer);
        spawnedWheelchair.GetComponent<Wheelchair>().Push(PlayerMove.fixedForwardDirection);   // push wheelchair

        // Timer to remove wheelchair
        yield return new WaitForSeconds(removeWheelchairTimer);
        Destroy(spawnedWheelchair);

        this.enabled = false;        
    }

    // A get well soon ballon appears in front of you - when player looks forward again
    public GameObject balloon;
    private GameObject spawnedBalloon;
    public float balloonDistanceBehindPlayer = 10;

    private float removeBalloonTimer = 6.8f; // when timer to pop balloon starts
    private float playerContinueToLookBackTimer = 2f;

    private IEnumerator Balloon()
    {
        PlayerMove.starebackTimer = 10;  // make player stare back longer

        // Calculate spawn position behind the player, and spawn balloon
        Vector3 spawnPosition = player.transform.position - PlayerMove.fixedForwardDirection * balloonDistanceBehindPlayer + Vector3.up * 0.9f;
        spawnedBalloon = Instantiate(balloon, spawnPosition, Quaternion.Euler(0, PlayerMove.fixedRotationY, 0));

        // Then wait until player turns around to push the balloon
        yield return new WaitUntil(() => PlayerMove.isStaringBackwards);
        PlayImpactSound(0);   // Play an impact sound
        spawnedBalloon.GetComponent<Balloon>().Push(PlayerMove.fixedForwardDirection);   // Push balloon towards player

        // Timer to pop balloon
        yield return new WaitForSeconds(removeBalloonTimer);
        spawnedBalloon.GetComponent<Balloon>().Pop();

        // Player continues to look back for a moment after the pop
        yield return new WaitForSeconds(playerContinueToLookBackTimer);

        // Finally continue the player on their way
        PlayerMove.ResetStareBackTime();

        // Give a little time until disabling scares to ensure balloon is properly popped and removed
        yield return new WaitUntil(() => !PlayerMove.isLookingAround);
        this.enabled = false;
    }
}
