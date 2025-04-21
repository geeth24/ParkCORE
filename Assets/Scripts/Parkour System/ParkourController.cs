using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParkourController : MonoBehaviour
{
    [SerializeField] List<ParkourAction> parkourActions;
    [SerializeField] ParkourAction jumpDownAction;
    [SerializeField] float autoDropHeightLimit = 1f;

    EnvironmentScanner environmentScanner;
    Animator animator;
    PlayerController playerController;
    private void Awake()
    {
        environmentScanner = GetComponent<EnvironmentScanner>();
        animator = GetComponent<Animator>();
        playerController = GetComponent<PlayerController>();
    }

    private void Update()
    {
        var hitData = environmentScanner.ObstacleCheck();

        if (Input.GetButton("Jump") && !playerController.InAction && !playerController.IsHanging)
        {
            if (hitData.forwardHitFound)
            {
                foreach (var action in parkourActions)
                {
                    if (action.CheckIfPossible(hitData, transform))
                    {
                        StartCoroutine(DoParkourAction(action));
                        break;
                    }
                }
            }
        }

        if (playerController.IsOnLedge && !playerController.InAction && !hitData.forwardHitFound)
        {
            bool shouldJump = true;
            if (playerController.LedgeData.height > autoDropHeightLimit && !Input.GetButton("Jump"))
                shouldJump = false;

            if (shouldJump && playerController.LedgeData.angle <= 50)
            {
                playerController.IsOnLedge = false;
                StartCoroutine(DoParkourAction(jumpDownAction));
            }
        }
    }

    IEnumerator DoParkourAction(ParkourAction action)
    {
        playerController.SetControl(false);

        MatchTargetParams matchParams = null;
        if (action.EnableTargetMatching)
        {
            matchParams = new MatchTargetParams()
            {
                pos = action.MatchPos,
                bodyPart = action.MatchBodyPart,
                posWeight = action.MatchPosWeight,
                startTime = action.MatchStartTime,
                targetTime = action.MatchTargetTime
            };
        }

        yield return playerController.DoAction(action.AnimName, matchParams, action.TargetRotation, 
            action.RotateToObstacle, action.PostActionDelay, action.Mirror);

        playerController.SetControl(true);
    }

    void MatchTarget(ParkourAction action)
    {
        if (animator.isMatchingTarget) return;

        animator.MatchTarget(action.MatchPos, transform.rotation, action.MatchBodyPart, new MatchTargetWeightMask(action.MatchPosWeight, 0), 
            action.MatchStartTime, action.MatchTargetTime);
    }
}
