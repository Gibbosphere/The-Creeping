using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using Unity.VisualScripting;
using UnityEngine;

public class MonsterHandler : MonoBehaviour
{
    public AudioSource audioSource;
    public AudioClip runningSound;
    public AudioClip screamSound1;
    public AudioClip screamSound2;
    public AudioClip screamSound3;

    private Dictionary<string, AudioClip> audioClips; // Dictionary to store different audio clips

    // Monster animations
    Animator animator;

    public static bool fromInFrontScare;

    private void Start()
    {
        animator = GetComponent<Animator>();

        audioSource = GetComponent<AudioSource>();

        // Initialize audio clips
        audioClips = new Dictionary<string, AudioClip>
        {
            { "runningSound", runningSound },
            { "scream_1", screamSound1 },
            { "scream_2", screamSound2 },
            { "scream_3", screamSound3 }
        };

        // Ensure the audio source is not playing initially
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        audioSource.loop = false;
    }

    private void Update()
    {
        if (running)
        {
            RunHelper();
        }
        if (runningTowardsPlayer)
        {
            RunTowardsPlayerHelper();
        }
        if (runningTowardsOnTurnPlayer)
        {
            RunTowardsPlayerOnTurnHelper();
        }
        if (poppingOutHead)
        {
            PopOutHeadHelper();
        }
        if (poppingUp)
        {
            PopUpHelper();
        }
    }

    public void PlayMonsterScream(string screamNo, float delay)
    {
        if (screamNo == "-1")
        {
            string randomScreamSound = "scream_" + new System.Random().Next(1, 4);
            StartCoroutine(PlayMonsterSound(randomScreamSound, delay));
        }
        else
        {
            StartCoroutine(PlayMonsterSound("scream_" + screamNo, delay));
        }
    }

    public System.Collections.IEnumerator PlayMonsterSound(string soundName, float delay)
    {
        if (audioClips.ContainsKey(soundName))
        {
            // fast footsteps
            audioSource.pitch = soundName == "runningSound" ? 1.4f : 1f;
            
            audioSource.clip = audioClips[soundName];

            yield return new WaitForSeconds(delay); // delay sound if need be
            audioSource.Play();
        }
        else
        {
            Debug.LogWarning($"Scare audio clip for {soundName} not found!");
        }


    }


    private bool running = false;
    private Vector3 finalRunningPos;
    private float runningSpeed;
    public void Run(Vector3 finalRunningPos, float speed)
    {
        this.finalRunningPos = finalRunningPos;
        this.runningSpeed = speed;
        this.running = true;

        AnimateCrawl();
    }
    private void RunHelper()
    {
        transform.position = Vector3.MoveTowards(transform.position, finalRunningPos, runningSpeed * Time.deltaTime);

        // Check if monster has reached destination
        if (Vector3.Distance(transform.position, finalRunningPos) < 0.1f)
        {
            running = false;
            Destroy(gameObject); // Monster is done now, remove it
        }
    }


    private bool runningTowardsPlayer = false;
    private Vector3 playerPos;
    private float runningTowardsSpeed;
    public void RunTowardsPlayer(Vector3 playerPos, float speed)
    {
        this.playerPos = playerPos;
        this.runningTowardsSpeed = speed;
        this.runningTowardsPlayer = true;

        AnimateRun();
    }
    private void RunTowardsPlayerHelper()
    {
        transform.position = Vector3.MoveTowards(transform.position, playerPos, runningTowardsSpeed * Time.deltaTime);

        // Check if monster has reached destination
        if (Vector3.Distance(transform.position, playerPos) < 0.1f)
        {
            if (fromInFrontScare)
            {
                transform.position = playerPos - (0.3f * PlayerMove.fixedForwardDirection) - (0.1f * Vector3.up); // snap monster to correct place
            }
            else 
            {
                transform.position = playerPos + (0.4f * PlayerMove.fixedForwardDirection) - (0.1f * Vector3.up); // snap monster to correct place
            }

            runningTowardsPlayer = false;
            PlayerMove.killPlayer = true; // Monster got to player, end it
            PlayerMove.stopPlayer = false;
            AnimateIdle();
        }
    }


    private GameObject playerOnTurn;
    private bool runningTowardsOnTurnPlayer = false;
    private float runningTowardsOnTurnSpeed;
    Transform playerTransform; // Cache the transform reference
    public void RunTowardsOnTurnPlayer(float speed)
    {
        this.runningTowardsOnTurnSpeed = speed;
        this.runningTowardsOnTurnPlayer = true;
        playerOnTurn = GameObject.FindGameObjectWithTag("Player");
        playerTransform = playerOnTurn.transform;

        AnimateRun();
    }
    private void RunTowardsPlayerOnTurnHelper()
    {
        // Move the monster faster sideways than forwards to line up the monster with the player asap ***** use this once monster orientation correct
        transform.position = Vector3.MoveTowards(transform.position, new Vector3(playerTransform.position.x, playerTransform.position.y - 1.5f, playerTransform.position.z), runningTowardsOnTurnSpeed * Time.deltaTime);

        // Check if monster has reached destination
        if (Vector3.Distance(transform.position, playerTransform.position) < 1.6f)
        {
            transform.position = playerTransform.position + (0.6f * PlayerMove.fixedForwardDirection) - (1.5f * Vector3.up); // snap monster to correct place
            runningTowardsOnTurnPlayer = false;
            PlayerMove.killPlayer = true; // Monster got to player, end it
            AnimateIdle();
        }

        if (Vector3.Distance(transform.position, playerOnTurn.transform.position) < 20.5f)
        {
            PlayerMove.stopPlayer = true;
        }

    }


    private bool poppingOutHead = false;
    private Vector3 monsterDest;

    public void PopOutHead(bool fromLeft)
    {
        AnimateIdle();
        poppingOutHead = true;
        if (fromLeft)
        {
            monsterDest = transform.position + 1.5f * (Quaternion.Euler(0, 90, 0) * PlayerMove.fixedForwardDirection);

        }
        else
        {
            monsterDest = transform.position + 1.5f * (Quaternion.Euler(0, -90, 0) * PlayerMove.fixedForwardDirection);
        }

    }
    private void PopOutHeadHelper()
    {
        transform.position = Vector3.MoveTowards(transform.position, monsterDest, 1 * Time.deltaTime);

        // Check if monster has reached destination
        if (Vector3.Distance(transform.position, monsterDest) < 0.1f)
        {
            poppingOutHead = false;
        }
    }

    private bool poppingUp = false;
    private Vector3 finalMonsterPos;
    public void PopUp(Vector3 finalMonsterPos)
    {
        poppingUp = true;
        this.finalMonsterPos = finalMonsterPos;
        AnimateIdle();
    }
    private void PopUpHelper()
    {
        transform.position = Vector3.MoveTowards(transform.position, finalMonsterPos, 7 * Time.deltaTime);

        // Check if monster has reached destination
        if (Vector3.Distance(transform.position, finalMonsterPos) < 0.1f)
        {
            poppingUp = false;
        }
    }


    private void AnimateIdle()
    {
        animator.SetBool("run", false);
        animator.SetBool("crawl", false);
    }

    private void AnimateRun()
    {
        animator.SetBool("run", true);
    }

    private void AnimateCrawl()
    {
        print("setting crawl");
        animator.SetBool("run", true);
        //animator.SetBool("crawl", true);
    }


}
