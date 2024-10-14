using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class PlayerMove : MonoBehaviour
{
    // To Handle Physics - gravity + boundaries
    CharacterController controller;
    public float gravity = -20.0f;
    private Vector3 playerVelocity;      // Track vertical velocity

    // Moving
    public float forwardSpeed = 8.0f;
    public float leftRightSpeed = 4.0f;
    public static Vector3 fixedForwardDirection;     // Store a fixed forward direction for movement

    // Looking
    public float lookSpeed = 5.0f;
    public float lookXLimit = 70.0f;   // Vertical look limit
    public float lookYLimit = 30.0f;   // Horizontal look limit (Restricting Yaw)

    float rotationX = 0;               // The current vertical rotation angle
    float rotationY = 0;               // The current horizontal rotation angle
    public static float fixedRotationY = 0;          // The angle that is straight ahead in reference to the moving forward direction

    public Camera playerCamera;

    // States
    public static bool isColliding = false;
    public static bool isLookingAround = false;
    public static bool isTurningCorner = false;
    public static bool isViewingActionPoint = false;
    public static bool isEnteringActionPoint = false;
    public static bool stopPlayer = false;
    public static bool killPlayer = false;
    private static bool playerKilled = false;

    // Obstacle Collision
    private bool hasCollisionStarted = false;
    public static float collisionSlowDownSpeed = 2.0f;
    public float collisionDistance = 2f;
    private Vector3 finalPosAfterCollision;

    // Lookback
    public static float lookbackTimer = 4.0f;    // Duration for how long turning back takes (in seconds)
    public static float starebackTimer = 2.0f;    // Duration for how long to stare back for
    public static float originalStareBackTimer;  // Allows resetting the stareBackTimer (after jump scares manipulate it)
    public static float lookforwardTimer = 2.0f;   // Duration for how long turning forward again takes (in seconds)
    private float lookbackElapsed = 0.0f;  // Timer to keep track of the lookback duration
    private float starebackElapsed = 0.0f;
    private float lookforwardElapsed = 0.0f;
    public static bool isTurningBackwards = false;
    public static bool isStaringBackwards = false;

    Quaternion lookbackRotation; // Turn by 180 degrees
    Quaternion lookforwardRotation;

    // Turning Corners
    public Quaternion targetRotation;

    // Viewing Action Points
    private bool hasSlowingDownStarted;
    private bool readingActionPoint;
    private bool hasSpeedingUpStarted;
    private bool turningFromActionPoint;
    private bool startedSpeedUpSound;

    private float viewActionPointTimer = 5.0f;   // Duration for how long turning forward again takes (in seconds)
    private float viewActionPointElapsed = 0.0f;  // Timer to keep track of the lookback duration

    private Vector3 finalConstantSpeedPos;
    private Vector3 finalResumeSpeedPos;

    public float slowDownDistance = 2f;
    public static Vector3 ActionPointPos;

    // Entering Action Points
    public bool isFacingActionPoint = false;
    public static float watchActionPointTimer = 6.0f;   // Duration for how long to watch action point open
    private float watchActionPointElapsed = 0.0f;

    // End the level
    public GameObject levelEnder;

    void Start()
    {
        // For physics - gravity and boundaries
        controller = GetComponent<CharacterController>();

        // Save the initial forward direction of the player
        fixedForwardDirection = transform.forward;
        // Initialize rotationY for the camera to match the initial player rotation
        fixedRotationY = Quaternion.LookRotation(fixedForwardDirection).eulerAngles.y;
        rotationY = fixedRotationY;

        // Lock the cursor for better control
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // Save the original stare back duration
        originalStareBackTimer = starebackTimer;
    }

    void Update()
    {

        if (killPlayer)
        {
            if (!playerKilled)
            {
                playerKilled = true;
                StartCoroutine(levelEnder.GetComponent<LevelEnder>().EndLevel(false));
            }
        }

        else if (stopPlayer)
        {
            //Debug.Log("Player stopped and possibly about to be killed - stopping all player movement");
        }

        else if (isTurningCorner)
        {
            // Automatic player turning when stepping on activation pad
            TurnCorner();
        }

        else if (isViewingActionPoint)
        {
            ViewActionPoint();
        }

        else if (isEnteringActionPoint)
        {
            EnterActionPoint();
        }

        else if (!isColliding && !isLookingAround)
        {
            // Constant forward movement using the fixed direction
            ForwardMovement();  

            // Side movement using the fixed direction
            SidewardsMovement();

            // Camera rotation for restricted looking around
            PlayerLooking();

            // Set player breathing to soft
            StartCoroutine(this.GetComponent<PlayerBreathing>().SetBreathingVolume("low"));
        }

        else if (isColliding)
        {
            // Momentum carries the player for a little before stopping completely 
            SlowDown();

            // Can continue to look around for this small moment
            PlayerLooking();
        }

        else if (isLookingAround)
        {
            PlayerLookAround();
        }

        // Apply gravity and check if character is grounded
        if (!controller.isGrounded)
        {
            playerVelocity.y += gravity * Time.deltaTime; // Apply gravity
            // Manually apply gravity to character controller using Move
            controller.Move(playerVelocity * Time.deltaTime);
        }
        else if (playerVelocity.y < 0)
        {
            playerVelocity.y = 0f; // Reset velocity when grounded
        }
    }

    public static void ResetStareBackTime()
    {
        starebackTimer = PlayerMove.originalStareBackTimer;
    }

    void ForwardMovement()
    {
        // Calculate forward movement using fixed direction
        Vector3 moveDirection = forwardSpeed * fixedForwardDirection * Time.deltaTime;

        // Use the CharacterController to move the player
        controller.Move(moveDirection);
    }

    void SidewardsMovement()
    {
        Vector3 left = Vector3.Cross(fixedForwardDirection, Vector3.up); // Get the left direction based on fixed forward direction
        Vector3 moveDirection = Vector3.zero; // Reset movement direction for each frame

        // Check for left movement
        if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A))
        {
            moveDirection = leftRightSpeed * left * Time.deltaTime;
        }
        // Check for right movement
        else if (Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D))
        {
            moveDirection = leftRightSpeed * -left * Time.deltaTime;
        }

        // Use CharacterController to move
        controller.Move(moveDirection);

    }

    void PlayerLooking()
    {
        // Calculate rotation values based on mouse input
        rotationX += -Input.GetAxis("Mouse Y") * lookSpeed;
        rotationY += Input.GetAxis("Mouse X") * lookSpeed;

        // Clamp the vertical rotation
        rotationX = Mathf.Clamp(rotationX, -lookXLimit, lookXLimit);

        // Clamp the horizontal rotation
        rotationY = Mathf.Clamp(rotationY, (fixedRotationY - lookYLimit), (fixedRotationY + lookYLimit));

        // Apply the rotations to the camera for looking up/down
        playerCamera.transform.localRotation = Quaternion.Euler(rotationX, 0, 0);

        // Apply the clamped horizontal rotation to the player (left/right looking)
        transform.localRotation = Quaternion.Euler(0, rotationY, 0);
    }

    void EnterActionPoint()
    {
        if (!isFacingActionPoint)
        {
            // Continue to move the player forward, but slightly slower than usual
            //transform.position += (forwardSpeed / 3) * Time.deltaTime * fixedForwardDirection;
            Vector3 moveDirection = (forwardSpeed / 3) * fixedForwardDirection * Time.deltaTime;
            controller.Move(moveDirection);

            // Rotate player towards the target rotation smoothly
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, 250 * Time.deltaTime);
            
            // Check if the rotation is complete
            if (Quaternion.Angle(transform.rotation, targetRotation) < 0.1f)
            {
                isFacingActionPoint = true;

                // Set the new forward direction
                fixedForwardDirection = new Vector3(Mathf.Round(transform.forward.x), transform.forward.y, Mathf.Round(transform.forward.z)); // Update the fixed forward direction
                fixedRotationY = Quaternion.LookRotation(fixedForwardDirection).eulerAngles.y;
                rotationY = fixedRotationY;
            }

        }
        else
        {
            // Once action point has opened, run into it
            watchActionPointElapsed += Time.deltaTime;
            if (watchActionPointElapsed >= watchActionPointTimer)
            {
                isFacingActionPoint = false;
                isEnteringActionPoint = false;
                watchActionPointElapsed = 0.0f;
            }
        }
    }


    Quaternion initialPlayerRotation; // Store the original player rotation
    Quaternion initialCameraRotation; // Store the original camera rotation

    void ViewActionPoint()
    {
        // The first moment the player passes the action point
        if (!hasSlowingDownStarted)
        {
            // Sound effect of player slowing down
            this.GetComponent<PlayerSlowmo>().PlaySlowDown();

            // Increase sound of player breathing
            StartCoroutine(this.GetComponent<PlayerBreathing>().SetBreathingVolume("high"));

            // Set the final position after the collision (for deceleration)
            finalConstantSpeedPos = transform.position + (fixedForwardDirection * 1);
            initialPlayerRotation = Quaternion.Euler(0, fixedRotationY, 0);        // Store the initial player rotation
            initialCameraRotation = Quaternion.Euler(0, fixedRotationY, 0); // Store the initial camera rotation
            hasSlowingDownStarted = true;
        }
        else if (!readingActionPoint && !hasSpeedingUpStarted && !turningFromActionPoint)
        {
            // Calculate the distance to the final position
            float remainingDistance = Vector3.Distance(transform.position, finalConstantSpeedPos);
            // Calculate the speed reduction factor based on the remaining distance - for smooth deceleration
            float smoothSpeed = Mathf.Lerp(0.5f, forwardSpeed, remainingDistance / slowDownDistance);
            // Move the player towards the constant slow motion speed point
            Vector3 moveDirection = (finalConstantSpeedPos - transform.position).normalized * smoothSpeed * Time.deltaTime;
            controller.Move(moveDirection);

            // Check if the player has reached the slowing down point, then we'll keep them moving at a slow constant speed
            if (remainingDistance < 0.1f)
            {
                readingActionPoint = true;

                Debug.Log("Starting To view action point for 6 seconds");
            }
        }
        else if (readingActionPoint)
        {
            // Move player forward at a very slow constant speed
            controller.Move(0.5f * fixedForwardDirection * Time.deltaTime);

            // Calculate the direction vector towards the action point
            Vector3 directionToActionPoint = (ActionPointPos - transform.position).normalized;

            // Smoothly rotate player towards the action point to constantly face it
            Quaternion targetRotation = Quaternion.LookRotation(directionToActionPoint);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 2.0f * Time.deltaTime);

            // If the camera is tilted significantly downwards, adjust it
            if (Mathf.Abs(rotationX) > 2f)
            {
                // Gradually lift the camera to be horizontal
                rotationX = Mathf.Lerp(rotationX, 0, Time.deltaTime * 2.0f); // Smoothly bring to 0 (horizontal)
                playerCamera.transform.localRotation = Quaternion.Euler(rotationX, 0, 0);
            }

            // How long the player is viewing the action point
            viewActionPointElapsed += Time.deltaTime;

            // Start player speed up sound 1 second early
            if (!startedSpeedUpSound && viewActionPointElapsed >= viewActionPointTimer - 0.5f)
            {
                startedSpeedUpSound = true;
                this.GetComponent<PlayerSlowmo>().PlaySpeedUp();  // Play speed up sound
                StartCoroutine(this.GetComponent<PlayerBreathing>().SetBreathingVolume("low"));  // Decrease sound of player breathing again
            }

            // Once viewing time is complete, proceed to speed player up again
            if (viewActionPointElapsed >= viewActionPointTimer)
            {
                viewActionPointElapsed = 0f;

                // Reset the state and proceed to the next step
                startedSpeedUpSound = false;
                readingActionPoint = false;
                hasSpeedingUpStarted = true;
            }
        }
        else if (hasSpeedingUpStarted)
        {
            // Set the position for the speed-up transition
            hasSpeedingUpStarted = false;
            finalResumeSpeedPos = transform.position + (fixedForwardDirection * 2f);
            turningFromActionPoint = true;
        }
        else if (turningFromActionPoint)
        {
            // Move the player back towards the normal speed point
            float remainingDistance = Vector3.Distance(transform.position, finalResumeSpeedPos);
            float smoothSpeed = Mathf.Lerp(forwardSpeed, 0.5f, remainingDistance / slowDownDistance);
            Vector3 moveDirection = (finalResumeSpeedPos - transform.position).normalized * smoothSpeed * Time.deltaTime;
            controller.Move(moveDirection);

            // Smoothly rotate the player back to the exact original rotation
            transform.rotation = Quaternion.RotateTowards(transform.rotation, initialPlayerRotation, 180f * Time.deltaTime);

            // Smoothly return the camera's vertical rotation to its original value
            playerCamera.transform.localRotation = Quaternion.RotateTowards(playerCamera.transform.localRotation, initialCameraRotation, 180f * Time.deltaTime);

            // Check if the player is back to the original orientation and ready to continue running normally
            if (Quaternion.Angle(transform.rotation, initialPlayerRotation) < 1.0f && Quaternion.Angle(playerCamera.transform.localRotation, initialCameraRotation) < 1.0f)
            {
                rotationX = 0;
                rotationY = fixedRotationY;
                hasSlowingDownStarted = false;
                turningFromActionPoint = false;
                isViewingActionPoint = false;
            }
        }
    }


    void SlowDown()
    {
        // The first moment the player collides
        if (!hasCollisionStarted)
        {
            // Increase player breathing
            StartCoroutine(this.GetComponent<PlayerBreathing>().SetBreathingVolume("medium"));

            // Camera shake

            // set the final position after the collision
            finalPosAfterCollision = transform.position + (fixedForwardDirection * collisionDistance);
            hasCollisionStarted = true;
        }

        // Calculate the distance to the final position
        float remainingDistance = Vector3.Distance(transform.position, finalPosAfterCollision);
        // Calculate the speed reduction factor based on the remaining distance - for smooth deceleration
        float smoothSpeed = Mathf.Lerp(0, forwardSpeed, remainingDistance / collisionDistance);

        // Move the player towards the ending collision point
        Vector3 moveDirection = (finalPosAfterCollision - transform.position).normalized * smoothSpeed * Time.deltaTime;
        controller.Move(moveDirection);

        // Check if the player has reached the ending collision point
        if (remainingDistance < 0.1f)
        {
            isLookingAround = true;
            isTurningBackwards = true;
            isColliding = false;
            hasCollisionStarted = false;
            PlayerPositioning.obstacleHitPosition = transform.position; // Set position of the stopped player for jump scares
        }
    }

    bool isTurningBackwardsStarted = false;
    float extraRotation;
    float turnAmount;
    void PlayerLookAround() 
    {
        // Lookback logic
        if (isTurningBackwards)
        {
            if (!isTurningBackwardsStarted) {
                // Set player breathing to high
                StartCoroutine(this.GetComponent<PlayerBreathing>().SetBreathingVolume("high"));
                isTurningBackwardsStarted = true;

                extraRotation = fixedRotationY - rotationY;
                turnAmount = 180 + extraRotation;
            }
            else {
                // Start by turning 180 degrees around the Y-axis
                lookbackRotation = Quaternion.Euler(0, turnAmount / lookbackTimer, 0);
                transform.rotation = Quaternion.Slerp(transform.rotation, lookbackRotation * transform.rotation, 1.0f * Time.deltaTime);

                // If the camera is tilted significantly downwards, adjust it
                if (Mathf.Abs(rotationX) > 2f)
                {
                    // Gradually lift the camera to be horizontal
                    rotationX = Mathf.Lerp(rotationX, 0, Time.deltaTime * 2.0f); // Smoothly bring to 0 (horizontal)
                    playerCamera.transform.localRotation = Quaternion.Euler(rotationX, 0, 0);
                }

                // Start the lookback timer
                lookbackElapsed += Time.deltaTime;

                // Once the lookback is complete, start holding the backward view
                if (lookbackElapsed >= lookbackTimer)
                {
                    isTurningBackwards = false;
                    isTurningBackwardsStarted = false;
                    isStaringBackwards = true;
                    lookbackElapsed = 0.0f; // Reset the timer
                }
            }
        }
        else if (isStaringBackwards) 
        {
            // Turn breathing off for suspensful silence
            StartCoroutine(this.GetComponent<PlayerBreathing>().StopBreathing());

            // Start the stareback timer
            starebackElapsed += Time.deltaTime;

            // Once the stareback is complete, start holding the backward view
            if (starebackElapsed >= starebackTimer)
            {
                isStaringBackwards = false;
                starebackElapsed = 0.0f; // Reset the timer
                // Bring back player breathing
                StartCoroutine(this.GetComponent<PlayerBreathing>().SetBreathingVolume("medium"));
            }
        }
        else 
        {
            // Turn back to the original forward direction
            lookforwardRotation = Quaternion.Euler(0, -180 / lookforwardTimer, 0);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookforwardRotation * transform.rotation, 1.0f * Time.deltaTime);

            // Once we're done looking around, reset running
            lookforwardElapsed += Time.deltaTime;
            if (lookforwardElapsed >= lookforwardTimer)
            {
                rotationX = 0;
                rotationY = fixedRotationY;
                isLookingAround = false;
                lookforwardElapsed = 0.0f;
            }
        }
    }

    void TurnCorner()
    {
        // Rotate player towards the target rotation smoothly
        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, 250 * Time.deltaTime);

        // Check if the rotation is complete
        if (Quaternion.Angle(transform.rotation, targetRotation) < 0.1f)
        {
            isTurningCorner = false;

            // Set the new forward direction
            fixedForwardDirection = new Vector3(Mathf.Round(transform.forward.x), transform.forward.y, Mathf.Round(transform.forward.z));
            fixedRotationY = Quaternion.LookRotation(fixedForwardDirection).eulerAngles.y;
            rotationY = fixedRotationY;
        }

        // Continue to move the player forward, but slightly slower than usual
        Vector3 moveDirection = (forwardSpeed / 2) * fixedForwardDirection * Time.deltaTime;
        controller.Move(moveDirection);  // Use CharacterController to move
    }
}
