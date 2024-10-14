using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.PlayerLoop;

public class CaughtScares : MonoBehaviour
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
    public static int noOfScares = 6 + 1; // case 2 and 3 both call a next turn scare for higher probability

    // Which jump scare is active
    private bool runTowardsFromBehindActive = false;
    private bool runTowardsFromInFrontActive = false;
    public static bool runTowardsAtNextTurnActive = false;
    private bool balloonActive = false;
    private bool immediatePopupActive = false;
    private bool appearBehindWithFlickerActive = false;

    public static bool fromInFrontScare;

    // Specifically used by next turn jump scare to first call a non caught scare
    public GameObject notCaughtScares;

    // Audio management
    public AudioSource audioSource;  // The single AudioSource used for playing different sounds

    public AudioClip finalScream1;
    public AudioClip finalScream2;
    public AudioClip finalScream3;

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
            { "final_scream_1", finalScream1 },
            { "final_scream_2", finalScream2 },
            { "final_scream_3", finalScream3 },
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
                StartCoroutine(RunTowardsFromBehind());
                break;
            case 1:
                StartCoroutine(RunTowardsFromInFront());
                break;
            case 2:
                StartCoroutine(RunTowardsAtNextTurn());
                break;
            case 3:
                StartCoroutine(RunTowardsAtNextTurn());
                break;
            case 4:
                StartCoroutine(AppearBehindWithFlicker());
                break;
            case 5:
                StartCoroutine(Balloon());
                break;
            case 6:
                StartCoroutine(ImmediatePopup());
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
    // Randomizing impact sounds
    private void PlayImpactSound()
    {
        string randomImpactSound = "impact_" + new System.Random().Next(1, 6);
        StartCoroutine(PlayScareSound(randomImpactSound));
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


    // The monster runs rapidly directly towards the player - when looking back
    private IEnumerator RunTowardsFromBehind()
    {
        MonsterHandler.fromInFrontScare = false;
        runTowardsFromBehindActive = true;

        // Wait for player to start turning back before spawning monster
        yield return new WaitUntil(() => PlayerMove.isTurningBackwards);

        Vector3 spawnPosition = PlayerPositioning.obstacleHitPosition - (45 * PlayerMove.fixedForwardDirection);
        spawnPosition.y -= 1.4f; // Lower the position by 1.4 units on the Y-axis
        spawnedMonster = Instantiate(monster, spawnPosition, Quaternion.Euler(0, PlayerMove.fixedRotationY, 0));
        runSpeed = 40f;
        playerPos = PlayerPositioning.obstacleHitPosition - 1 * PlayerMove.fixedForwardDirection;
        playerPos.y -= 1.4f;


        // Once player is looking back - start jump scare
        yield return new WaitUntil(() => PlayerMove.isStaringBackwards);

        PlayMonsterScream("3");   // Start scream sound
        PlayerMove.stopPlayer = true;   // Stop player
        spawnedMonster.GetComponent<MonsterHandler>().RunTowardsPlayer(playerPos, runSpeed);   // Make monster run

        runTowardsFromBehindActive = false;
        yield return new WaitForSeconds(5f);
        this.enabled = false;
    }

    // The monster runs rapidly directly towards the player - after looking forward again
    private IEnumerator RunTowardsFromInFront()
    {
        MonsterHandler.fromInFrontScare = true;
        runTowardsFromInFrontActive = true;

        // Spawn the monster only once player looks back
        yield return new WaitUntil(() => PlayerMove.isStaringBackwards);

        Vector3 spawnPosition = PlayerPositioning.obstacleHitPosition + 80 * PlayerMove.fixedForwardDirection;
        spawnPosition.y -= 1.4f;
        spawnedMonster = Instantiate(monster, spawnPosition, Quaternion.Euler(0, PlayerMove.fixedRotationY + 180, 0)); // monster facing the player
        runSpeed = 35f;
        playerPos = PlayerPositioning.obstacleHitPosition + 1 * PlayerMove.fixedForwardDirection;
        playerPos.y -= 1.4f;

        // Make monster run once player begins to turn forward again
        yield return new WaitUntil(() => !PlayerMove.isStaringBackwards);
        PlayMonsterScream("1");   // Start scream sound
        spawnedMonster.GetComponent<MonsterHandler>().RunTowardsPlayer(playerPos, runSpeed);

        // Stop player once they have fully turned backwards
        yield return new WaitUntil(() => !PlayerMove.isLookingAround);
        PlayerMove.stopPlayer = true;
        yield return new WaitForSeconds(4f);
        runTowardsFromInFrontActive = false;
    }

    // The monster runs rapidly directly towards the player - at the next turn they take
    private Vector3 originalPreviousDecisionPoint;
    private int randomCornerCount;
    private IEnumerator RunTowardsAtNextTurn()
    {
        // Set a random number of corners to turn before getting the jump scare (currently 1 to 2 corners)
        randomCornerCount = new System.Random().Next(1, 3);

        originalPreviousDecisionPoint = PlayerPositioning.previousDecisionPosition;
        runTowardsAtNextTurnActive = true;

        // Call a not caught scare first (deceiving the player)
        notCaughtScares.GetComponent<NotCaughtScares>().ChooseJumpScare(new System.Random().Next(0, NotCaughtScares.noOfScares));

        while (randomCornerCount > 0)
        {
            if (originalPreviousDecisionPoint != PlayerPositioning.previousDecisionPosition)
            {
                randomCornerCount--;
                originalPreviousDecisionPoint = PlayerPositioning.previousDecisionPosition;
            }
            yield return null;   // Wait for the next frame
        }

        // Spawn the monster
        PlayMonsterScream("3");   // Start scream sound
        Vector3 spawnPosition;   // Spawning monster down the passageway
        float monsterRotation;
        if (PlayerPositioning.leftDecisionTurn)
        {
            spawnPosition = PlayerPositioning.previousDecisionPosition + 60 * (Quaternion.Euler(0, -90, 0) * PlayerMove.fixedForwardDirection);
            monsterRotation = PlayerMove.fixedRotationY + 90;
        }
        else
        {
            spawnPosition = PlayerPositioning.previousDecisionPosition + 60 * (Quaternion.Euler(0, 90, 0) * PlayerMove.fixedForwardDirection);
            monsterRotation = PlayerMove.fixedRotationY - 90;
        }
        spawnPosition.y -= 1.4f; // Lower the position by 1.4 units on the Y-axis

        spawnedMonster = Instantiate(monster, spawnPosition, Quaternion.Euler(0, monsterRotation, 0)); // monster facing the player
        runSpeed = 35f;

        MonsterHandler monsterHandler = spawnedMonster.GetComponent<MonsterHandler>();
        StartCoroutine(WaitForInitializationThenRunOnTurn(monsterHandler, runSpeed));
    }
    private IEnumerator WaitForInitializationThenRunOnTurn(MonsterHandler handler, float speed)
    {
        // Wait until the audioSource or other components are set up
        while (handler.audioSource == null)
        {
            yield return null; // Wait for the next frame
        }

        handler.RunTowardsOnTurnPlayer(speed);
    }



    private float lightOffTimer = 3f;
    private IEnumerator AppearBehindWithFlicker()
    {
        MonsterHandler.fromInFrontScare = false;
        appearBehindWithFlickerActive = true;

        yield return new WaitUntil(() => PlayerMove.isStaringBackwards && !PlayerMove.stopPlayer);

        // Stop player
        PlayerMove.stopPlayer = true;

        // Turn off the lights completely
        StartCoroutine(torch.GetComponent<TorchManagement>().Flicker());
        yield return new WaitForSeconds(2.5f);
        torch.GetComponent<TorchManagement>().turnOff();

        // Keep dark for a few seconds 
        yield return new WaitForSeconds(lightOffTimer);

        // Spawn Monster
        Vector3 spawnPosition = PlayerPositioning.obstacleHitPosition - 10 * PlayerMove.fixedForwardDirection;
        spawnPosition.y -= 1.4f; // Lower the position by 1.4 units on the Y-axis
        spawnedMonster = Instantiate(monster, spawnPosition, Quaternion.Euler(0, PlayerMove.fixedRotationY, 0));
        runSpeed = 20f;
        playerPos = PlayerPositioning.obstacleHitPosition - 1 * PlayerMove.fixedForwardDirection;
        playerPos.y -= 1.4f; // Lower the position by 1.4 units on the Y-axis

        // Turn on the lights and send that monster
        yield return new WaitForSeconds(2);
        torch.GetComponent<TorchManagement>().turnOn();   // Turn torch on 
        yield return new WaitForSeconds(0.5f);

        // Make monster run
        PlayMonsterScream("2");  // Start scream sound
        MonsterHandler monsterHandler = spawnedMonster.GetComponent<MonsterHandler>();
        if (monsterHandler != null)
        {
            StartCoroutine(WaitForInitializationThenRun(monsterHandler, playerPos, runSpeed));
        }

        appearBehindWithFlickerActive = false;

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


    // A get well soon ballon appears in front of you - when player looks forward again
    public GameObject balloon;
    private GameObject spawnedBalloon;
    public float balloonDistanceBehindPlayer = 10;

    private bool movedBalloon = false;

    private float removeBalloonTimer = 6.8f; // when timer to pop balloon starts
    private float removeBalloonTimeElapsed = 0f;
    private bool ballonPopped = false;

    private bool balloonMonsterSpawned = false;

    private IEnumerator Balloon()
    {
        MonsterHandler.fromInFrontScare = false;
        balloonActive = true;

        PlayerMove.starebackTimer = 10;  // make player stare back longer

        // Calculate spawn position behind the player, and spawn balloon
        Vector3 spawnPosition = player.transform.position - PlayerMove.fixedForwardDirection * balloonDistanceBehindPlayer + Vector3.up * 0.9f;
        spawnedBalloon = Instantiate(balloon, spawnPosition, Quaternion.Euler(0, PlayerMove.fixedRotationY, 0));

        // Wait until player is looking back to push balloon
        yield return new WaitUntil(() => PlayerMove.isStaringBackwards);
        spawnedBalloon.GetComponent<Balloon>().Push(PlayerMove.fixedForwardDirection);


        // Spawn the monster behind the balloon
        yield return new WaitForSeconds(removeBalloonTimer / 2);

        Vector3 monsterSpawnPosition = PlayerPositioning.obstacleHitPosition - 30 * PlayerMove.fixedForwardDirection;
        monsterSpawnPosition.y -= 1.4f; // Lower the position by 1.4 units on the Y-axis
        spawnedMonster = Instantiate(monster, monsterSpawnPosition, Quaternion.Euler(0, PlayerMove.fixedRotationY, 0));
        runSpeed = 30f;
        playerPos = PlayerPositioning.obstacleHitPosition - 1 * PlayerMove.fixedForwardDirection;
        playerPos.y -= 1.4f; // Lower the position by 1.4 units on the Y-axis


        // Pop Balloon and make monster run
        yield return new WaitForSeconds(removeBalloonTimer / 2);
        spawnedBalloon.GetComponent<Balloon>().Pop();
        PlayMonsterScream("3");   // Start scream sound
        spawnedMonster.GetComponent<MonsterHandler>().RunTowardsPlayer(playerPos, runSpeed);

        // Stop player movement
        PlayerMove.stopPlayer = true;
        balloonActive = false;
    }

    private IEnumerator ImmediatePopup()
    {
        immediatePopupActive = true;

        yield return new WaitForSeconds(0.1f);
        yield return new WaitUntil(() => !PlayerMove.isColliding);

        // Spawn monster in front of player, under the ground
        Vector3 spawnPosition = PlayerPositioning.obstacleHitPosition + 0.6f * PlayerMove.fixedForwardDirection - 3 * Vector3.up;
        spawnedMonster = Instantiate(monster, spawnPosition, Quaternion.Euler(0, PlayerMove.fixedRotationY + 180, 0)); // monster facing the player

        // Start scream sound
        PlayMonsterScream("2");

        // Make monster popup (after a small delay to give it initialization time)
        MonsterHandler monsterHandler = spawnedMonster.GetComponent<MonsterHandler>();
        if (monsterHandler != null)
        {
            StartCoroutine(WaitForInitializationThenPopup(monsterHandler, spawnPosition + 1.6f * Vector3.up));
        }

        immediatePopupActive = false;
        PlayerMove.killPlayer = true;
    }
    private IEnumerator WaitForInitializationThenPopup(MonsterHandler handler, Vector3 finalMonsterPos)
    {
        // Wait until the audioSource or other components are set up
        while (handler.audioSource == null)
        {
            yield return null; // Wait for the next frame
        }

        handler.PopUp(finalMonsterPos);
    }
}
