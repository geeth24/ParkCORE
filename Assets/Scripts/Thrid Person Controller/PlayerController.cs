using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    // Movement Speeds
    [SerializeField] float movementSpeed = 5f;
    [SerializeField] float rotationSpeed = 500f;
    [SerializeField] float airFactor = 0.7f;
    [SerializeField] float sprintMultiplier = 1.5f;
    [SerializeField] float jumpPower = 5f;
    [SerializeField] float jumpCooldown = 0.1f;

    // How close player is to touching ground
    [SerializeField] float groundCheckRadius = 0.2f;

    // Allows for picking which objects are apart of the "ground"
    [SerializeField] LayerMask groundLayer;

    // Position slightly below player's feet for pivot point
    [SerializeField] Vector3 groundCheckOffset;

    bool isGrounded;
    bool hasControl = true;
    bool canJump = true;

    public bool InAction { get; private set; }
    public bool IsHanging { get; set; }

    Vector3 desiredMoveDirection;
    Vector3 moveDir;
    Vector3 velocity;

    public bool IsOnLedge { get; set; }
    public LedgeData LedgeData { get; set; }

    float ySpeed;
    Quaternion targetRotation;

    CameraController cameraController;
    Animator animator;
    CharacterController characterController;
    EnvironmentScanner environmentScanner;

    // Components that make up a player when in use
    private void Awake()
    {
        cameraController = Camera.main.GetComponent<CameraController>();
        animator = GetComponent<Animator>();
        characterController = GetComponent<CharacterController>();
        environmentScanner = GetComponent<EnvironmentScanner>();
    }

    // Provides basic functionalities for when our player moves and operates throughout the game
    private void Update()
    {
        // Get x and y coordinates
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");
        bool clickRunButton = Input.GetMouseButton(0);

        // Calculate magnitude of movement
        float moveAmount = clickRunButton ? 1f : Mathf.Clamp01(Mathf.Abs(h) + Mathf.Abs(v));

        // Calculate direction of movement
        var moveInput = clickRunButton ? Vector3.forward : (new Vector3(h, 0, v)).normalized;
        desiredMoveDirection = cameraController.PlanarRotation * moveInput;
        moveDir = desiredMoveDirection;

        // Instances where the player cannot "update"
        if (!hasControl) return;
        if (IsHanging) return;

        velocity = Vector3.zero;

        GroundCheck();
        animator.SetBool("isGrounded", isGrounded);
        
        // Handles a jump by the user
        if (isGrounded && Input.GetButtonDown("Jump") && canJump && !InAction)
        {
            Jump();
        }
        
        // Components for if a player is currently on the ground
        if (isGrounded)
        {
            // Make sure character is firmly on ground
            ySpeed = -0.5f;

            float speed = movementSpeed * (clickRunButton ? sprintMultiplier : 1f);
            velocity = desiredMoveDirection * speed;

            // Check if player is walking into an obstacle or ledge
            if (moveAmount > 0 && Physics.Raycast(transform.position + desiredMoveDirection * 0.5f, Vector3.down, 2f, groundLayer))
            {
                // Checks for specific ledge and handles that movement
                IsOnLedge = environmentScanner.ObstacleLedgeCheck(desiredMoveDirection, out LedgeData ledgeData);

                if (IsOnLedge)
                {
                    LedgeData = ledgeData;
                    LedgeMovement();
                }
            }
            else
            {
                IsOnLedge = false;
            }

            animator.SetFloat("moveAmount", velocity.magnitude / movementSpeed, 0.2f, Time.deltaTime);
        }

        // If the character is not grounded then we account for gravity and other components
        else
        {
            // Apply gravity
            ySpeed += Physics.gravity.y * Time.deltaTime;
            float speedAir = movementSpeed * (clickRunButton ? sprintMultiplier : 1f);
            velocity = desiredMoveDirection * speedAir * airFactor;
            
            // Apply a different speed to our animation to account for falling
            animator.SetFloat("moveAmount", velocity.magnitude / movementSpeed, 0.2f, Time.deltaTime);
        }

        velocity.y = ySpeed;

        characterController.Move(velocity * Time.deltaTime);

        // Account for if there is a rotation
        if (moveAmount > 0 && moveDir.magnitude > 0.2f)
        {
            targetRotation = Quaternion.LookRotation(moveDir);
        }

        // Helps to apply a smooth rotation
        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
    }

    // Account for our player jumping
    void Jump()
    {
        // Apply an upwards thrust for a real jumping look
        ySpeed = jumpPower;
        isGrounded = false;

        
        transform.position += Vector3.up * 0.1f;

        // Apply our jumping animation
        animator.SetTrigger("jump");

        // Make cooldown until we can jump again
        canJump = false;
        StartCoroutine(ResetJumpCooldown());
    }

    // Cooldown for jumping
    IEnumerator ResetJumpCooldown()
    {
        yield return new WaitForSeconds(jumpCooldown);
        canJump = true;
    }

    // Function to check if player is currently grounded
    void GroundCheck()
    {
        bool wasGrounded = isGrounded;

        // Don't have to ground check if moving upward for smoothing purposes
        if (ySpeed > 0f)
        {
            isGrounded = false;
        }

        // Account for moving downward (-y)
        else
        {
            // Checks if sphere of player overlaps with sphere of another object to identify if grounded
            isGrounded = Physics.CheckSphere(transform.TransformPoint(groundCheckOffset), groundCheckRadius, groundLayer);
        }

        // Make player fall if jumping off ledge
        if (!isGrounded && desiredMoveDirection.magnitude > 0.05f)
        {
            ySpeed = -2f; 
        }

        // Applying a downward force after landing to stick
        if (!wasGrounded && isGrounded)
        {
            ySpeed = -1f;
        }
    }

    // For "shimmying" or movement on a ledge
    void LedgeMovement()
    {
        // Calculate angle relative to ledge
        float signedAngle = Vector3.SignedAngle(LedgeData.surfaceHit.normal, desiredMoveDirection, Vector3.up);
        float angle = Mathf.Abs(signedAngle);

        // Check if we want to fall of ledge intentionally
        bool wantToWalkOff = Vector3.Dot(desiredMoveDirection, transform.forward) > 0.7f;
        bool noGroundAhead = !Physics.Raycast(transform.position + transform.forward * 1.0f, Vector3.down, 2f, groundLayer);
        
        if (wantToWalkOff && noGroundAhead)
        {
            return;
        }

        // If player makes a sharp turn (80), make a rotation
        if (Vector3.Angle(desiredMoveDirection, transform.forward) >= 80)
        {
            velocity = Vector3.zero;
            return;
        }

        // If turning into a wall
        if (angle < 60)
        {
            velocity = Vector3.zero;
            moveDir = Vector3.zero;
        }

        // If moving along a wall
        else if (angle < 90)
        {
            // Slow speed of player
            var left = Vector3.Cross(Vector3.up, LedgeData.surfaceHit.normal);
            var dir = left * Mathf.Sign(signedAngle);

            velocity = velocity.magnitude * dir;
            moveDir = dir;
        }
    }

    // Function to handle multiple actions by our player
    public IEnumerator DoAction(string animName, MatchTargetParams matchParams = null,
        Quaternion targetRotation = new Quaternion(), bool rotate = false,
        float postDelay = 0f, bool mirror = false)
    {
        InAction = true;

        // To handle mirror animations
        animator.SetBool("mirrorAction", mirror);
        animator.CrossFadeInFixedTime(animName, 0.2f);
        yield return null;

        // Checks the current animation that is requested
        var animState = animator.GetNextAnimatorStateInfo(0);
        if (!animState.IsName(animName))
            Debug.LogError("The parkour animation is wrong!");

        float rotateStartTime = (matchParams != null) ? matchParams.startTime : 0f;

        // Duration of animation
        float timer = 0f;
        while (timer <= animState.length)
        {
            timer += Time.deltaTime;
            float normalizedTime = timer / animState.length;

            if (rotate && normalizedTime > rotateStartTime)
                transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);

            if (matchParams != null)
                MatchTarget(matchParams);

            if (animator.IsInTransition(0) && timer > 0.5f)
                break;

            yield return null;
        }

        yield return new WaitForSeconds(postDelay);

        InAction = false;
    }

    // To synchronize the animation
    void MatchTarget(MatchTargetParams mp)
    {
        if (animator.isMatchingTarget) return;

        animator.MatchTarget(mp.pos, transform.rotation, mp.bodyPart, new MatchTargetWeightMask(mp.posWeight, 0), mp.startTime, mp.targetTime);
    }

    // To set the user's control over character (For movement/animations) (Can't spam movements)
    public void SetControl(bool hasControl)
    {
        this.hasControl = hasControl;
        characterController.enabled = hasControl;

        if (!hasControl)
        {
            animator.SetFloat("moveAmount", 0f);
            targetRotation = transform.rotation;
        }
    }

    public void EnableCharacterController(bool enabled)
    {
        characterController.enabled = enabled;
    }

    public void ResetTargetRotation()
    {
        targetRotation = transform.rotation;
    }

    public bool HasControl 
    {
        get => hasControl;
        set => hasControl = value;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0, 1, 0, 0.5f);
        Gizmos.DrawSphere(transform.TransformPoint(groundCheckOffset), groundCheckRadius);
    }

    public float RotationSpeed => rotationSpeed;
}

// Target parameters for animations
public class MatchTargetParams
{
    public Vector3 pos;
    public AvatarTarget bodyPart;
    public Vector3 posWeight;
    public float startTime;
    public float targetTime;
}
