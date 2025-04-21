using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClimbController : MonoBehaviour
{
    ClimbPoint currentPoint;

    PlayerController playerController;
    EnvironmentScanner envScanner;
    private void Awake()
    {
        playerController = GetComponent<PlayerController>();
        envScanner = GetComponent<EnvironmentScanner>();
    }

    private void Update()
    {
        if (!playerController.IsHanging)
        {
            if (Input.GetButton("Jump") && !playerController.InAction)
            {
                if (envScanner.ClimbLedgeCheck(transform.forward, out RaycastHit ledgeHit))
                {
                    currentPoint = GetNearestClimbPoint(ledgeHit.transform, ledgeHit.point);

                    playerController.SetControl(false);
                    StartCoroutine(JumpToLedge("IdleToHang", currentPoint.transform, 0.41f, 0.54f));
                }
            }

            if (Input.GetButtonDown("Drop") && !playerController.InAction)
            {
                if (envScanner.DropLedgeCheck(out RaycastHit ledgeHit))
                {
                    currentPoint = GetNearestClimbPoint(ledgeHit.transform, ledgeHit.point);

                    playerController.SetControl(false);
                    StartCoroutine(JumpToLedge("DropToHang", currentPoint.transform, 0.30f, 0.45f, handOffset: new Vector3(0.25f, 0.2f, -0.2f)));
                }
            }
        }
        else
        {
            if (Input.GetButton("Drop") && !playerController.InAction)
            {
                StartCoroutine(JumpFromHang());
            }

            float h = Mathf.Round(Input.GetAxisRaw("Horizontal"));
            float v = Mathf.Round(Input.GetAxisRaw("Vertical"));
            var inputDir = new Vector2(h, v);

            if (playerController.InAction || inputDir == Vector2.zero) return;

            // Mount from the hanging state
            if (currentPoint.MountPoint && inputDir.y == 1)
            {
                StartCoroutine(MountFromHang());
                return;
            }

            // Ledge to Ledge Jump
            var neighbour = currentPoint.GetNeighbour(inputDir);
            if (neighbour == null) return;

            if (neighbour.connectionType == ConnectionType.Jump && Input.GetButton("Jump"))
            {
                currentPoint = neighbour.point;

                if (neighbour.direction.y == 1)
                    StartCoroutine(JumpToLedge("HangHopUp", currentPoint.transform, 0.35f, 0.65f, handOffset: new Vector3(0.25f, 0.08f, 0.15f)));
                else if (neighbour.direction.y == -1)
                    StartCoroutine(JumpToLedge("HangHopDown", currentPoint.transform, 0.31f, 0.65f, handOffset: new Vector3(0.25f, 0.1f, 0.13f)));
                else if (neighbour.direction.x == 1)
                    StartCoroutine(JumpToLedge("HangHopRight", currentPoint.transform, 0.20f, 0.50f));
                else if (neighbour.direction.x == -1)
                    StartCoroutine(JumpToLedge("HangHopLeft", currentPoint.transform, 0.20f, 0.50f));
            }
            else if (neighbour.connectionType == ConnectionType.Move)
            {
                currentPoint = neighbour.point;

                if (neighbour.direction.x == 1)
                    StartCoroutine(JumpToLedge("ShimmyRight", currentPoint.transform, 0f, 0.38f, handOffset: new Vector3(0.25f, 0.05f, 0.1f)));
                else if (neighbour.direction.x == -1)
                    StartCoroutine(JumpToLedge("ShimmyLeft", currentPoint.transform, 0f, 0.38f, AvatarTarget.LeftHand, handOffset: new Vector3(0.25f, 0.05f, 0.1f)));
            }
        }
    }

    IEnumerator JumpToLedge(string anim, Transform ledge, float matchStartTime, float matchTargetTime,
        AvatarTarget hand=AvatarTarget.RightHand,
        Vector3? handOffset=null)
    {
        var matchParams = new MatchTargetParams()
        {
            pos = GetHandPos(ledge, hand, handOffset),
            bodyPart = hand,
            startTime = matchStartTime,
            targetTime = matchTargetTime,
            posWeight = Vector3.one
        };

        var targetRot = Quaternion.LookRotation(-ledge.forward);

        yield return playerController.DoAction(anim, matchParams, targetRot, true);

        playerController.IsHanging = true;
    }

    Vector3 GetHandPos(Transform ledge, AvatarTarget hand, Vector3? handOffset)
    {
        var offVal = (handOffset != null) ? handOffset.Value : new Vector3(0.25f, 0.1f, 0.1f);

        var hDir = (hand == AvatarTarget.RightHand) ? ledge.right : -ledge.right;
        return ledge.position + ledge.forward * offVal.z + Vector3.up * offVal.y - hDir * offVal.x;
    }

    IEnumerator JumpFromHang()
    {
        playerController.IsHanging = false;
        yield return playerController.DoAction("JumpFromHang");

        playerController.ResetTargetRotation();
        playerController.SetControl(true);
    }

    IEnumerator MountFromHang()
    {
        playerController.IsHanging = false;
        yield return playerController.DoAction("MountFromHang");

        playerController.EnableCharacterController(true);

        yield return new WaitForSeconds(0.5f);

        playerController.ResetTargetRotation();
        playerController.SetControl(true);
    }

    ClimbPoint GetNearestClimbPoint(Transform ledge, Vector3 hitPoint)
    {
        var points = ledge.GetComponentsInChildren<ClimbPoint>();

        ClimbPoint nearestPoint = null;
        float nearestPointDistance = Mathf.Infinity;

        foreach (var point in points)
        {
            float distance = Vector3.Distance(point.transform.position, hitPoint);

            if (distance < nearestPointDistance)
            {
                nearestPoint = point;
                nearestPointDistance = distance;
            }
        }

        return nearestPoint;
    }
}
