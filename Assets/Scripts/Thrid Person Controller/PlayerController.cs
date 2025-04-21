using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] float moveSpeed = 5f;
    [SerializeField] float rotationSpeed = 500f;
    [SerializeField] float jumpForce = 5f;
    [SerializeField] float airControlFactor = 0.7f;
    [SerializeField] float jumpCooldown = 0.1f;

    [Header("Ground Check Settings")]
    [SerializeField] float groundCheckRadius = 0.2f;
    [SerializeField] Vector3 groundCheckOffset;
    [SerializeField] LayerMask groundLayer;

    bool isGrounded;
    bool hasControl = true;
    bool canJump = true;

    public bool InAction { get; private set; }
    public bool IsHanging { get; set; }

    Vector3 desiredMoveDir;
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
    private void Awake()
    {
        cameraController = Camera.main.GetComponent<CameraController>();
        animator = GetComponent<Animator>();
        characterController = GetComponent<CharacterController>();
        environmentScanner = GetComponent<EnvironmentScanner>();
    }

    private void Update()
    {
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");

        float moveAmount = Mathf.Clamp01(Mathf.Abs(h) + Mathf.Abs(v));

        var moveInput = (new Vector3(h, 0, v)).normalized;

        desiredMoveDir = cameraController.PlanarRotation * moveInput;
        moveDir = desiredMoveDir;

        if (!hasControl) return;

        if (IsHanging) return;

        velocity = Vector3.zero;

        GroundCheck();
        animator.SetBool("isGrounded", isGrounded);
        
        // Handle jumping
        if (isGrounded && Input.GetButtonDown("Jump") && canJump && !InAction)
        {
            Jump();
        }
        
        if (isGrounded)
        {
            ySpeed = -0.5f;
            velocity = desiredMoveDir * moveSpeed;

            // Only check for ledge if we're not intentionally trying to go off the edge
            if (moveAmount > 0 && Physics.Raycast(transform.position + desiredMoveDir * 0.5f, Vector3.down, 2f, groundLayer))
            {
                IsOnLedge = environmentScanner.ObstacleLedgeCheck(desiredMoveDir, out LedgeData ledgeData);
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

            animator.SetFloat("moveAmount", velocity.magnitude / moveSpeed, 0.2f, Time.deltaTime);
        }
        else
        {
            // Apply gravity
            ySpeed += Physics.gravity.y * Time.deltaTime;
            
            // Apply air control but with reduced effect
            velocity = desiredMoveDir * moveSpeed * airControlFactor;
            
            // Set air animation blend
            animator.SetFloat("moveAmount", velocity.magnitude / moveSpeed, 0.2f, Time.deltaTime);
        }


        velocity.y = ySpeed;

        characterController.Move(velocity * Time.deltaTime);

        if (moveAmount > 0 && moveDir.magnitude > 0.2f)
        {
            targetRotation = Quaternion.LookRotation(moveDir);
        }

        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation,
            rotationSpeed * Time.deltaTime);


    }

    void Jump()
    {
        // Apply more initial force for a snappier jump
        ySpeed = jumpForce;
        
        // Ensure we're really leaving the ground
        transform.position += Vector3.up * 0.1f;
        
        // Trigger jump animation
        animator.SetTrigger("jump");
        
        // Start cooldown
        canJump = false;
        StartCoroutine(ResetJumpCooldown());
    }

    IEnumerator ResetJumpCooldown()
    {
        yield return new WaitForSeconds(jumpCooldown);
        canJump = true;
    }

    void GroundCheck()
    {
        // Previous ground state for transition detection
        bool wasGrounded = isGrounded;
        
        isGrounded = Physics.CheckSphere(transform.TransformPoint(groundCheckOffset), groundCheckRadius, groundLayer);
        
        // Make character fall immediately when stepping off a ledge
        if (!isGrounded && desiredMoveDir.magnitude > 0.05f)
        {
            ySpeed = -2f; // Increased negative value for faster falling
        }
        
        // If we just landed, apply a small downward force to "stick" to the ground
        if (!wasGrounded && isGrounded)
        {
            ySpeed = -1f;
        }
    }

    void LedgeMovement()
    {
        float signedAngle = Vector3.SignedAngle(LedgeData.surfaceHit.normal, desiredMoveDir, Vector3.up);
        float angle = Mathf.Abs(signedAngle);

        // Check if we're trying to walk off an edge intentionally (forward direction with no ground)
        bool wantToWalkOff = Vector3.Dot(desiredMoveDir, transform.forward) > 0.7f;
        bool noGroundAhead = !Physics.Raycast(transform.position + transform.forward * 1.0f, Vector3.down, 2f, groundLayer);
        
        // If player wants to walk off edge, allow it
        if (wantToWalkOff && noGroundAhead)
        {
            // Don't restrict movement
            return;
        }

        if (Vector3.Angle(desiredMoveDir, transform.forward) >= 80)
        {
            // Don't move, but rotate
            velocity = Vector3.zero;
            return;
        }

        if (angle < 60)
        {
            velocity = Vector3.zero;
            moveDir = Vector3.zero;
        }
        else if (angle < 90)
        {
            // Angle is b/w 60 and 90, so limit the velocity to horizontal direction

            var left = Vector3.Cross(Vector3.up, LedgeData.surfaceHit.normal);
            var dir = left * Mathf.Sign(signedAngle);

            velocity = velocity.magnitude * dir;
            moveDir = dir;
        }
    }

    public IEnumerator DoAction(string animName, MatchTargetParams matchParams = null,
        Quaternion targetRotation = new Quaternion(), bool rotate = false,
        float postDelay = 0f, bool mirror = false)
    {
        InAction = true;

        animator.SetBool("mirrorAction", mirror);
        animator.CrossFadeInFixedTime(animName, 0.2f);
        yield return null;

        var animState = animator.GetNextAnimatorStateInfo(0);
        if (!animState.IsName(animName))
            Debug.LogError("The parkour animation is wrong!");

        float rotateStartTime = (matchParams != null) ? matchParams.startTime : 0f;

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

    void MatchTarget(MatchTargetParams mp)
    {
        if (animator.isMatchingTarget) return;

        animator.MatchTarget(mp.pos, transform.rotation, mp.bodyPart, new MatchTargetWeightMask(mp.posWeight, 0),
            mp.startTime, mp.targetTime);
    }

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


    public bool HasControl {
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

public class MatchTargetParams
{
    public Vector3 pos;
    public AvatarTarget bodyPart;
    public Vector3 posWeight;
    public float startTime;
    public float targetTime;
}
