using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClimbController : MonoBehaviour
{
    ClimbPoint currentPoint;

    // Uses the playerController 
    PlayerController playerController;
    EnvironmentScanner envScanner;

    private void Awake()
    {
        playerController = GetComponent<PlayerController>();
        envScanner = GetComponent<EnvironmentScanner>();
    }

    private void Update()
    {
        // If the player is currently not hanging on an object
        if (!playerController.IsHanging)
        {
            // If user wants to climb/jump onto something
            if (Input.GetButton("Jump") && !playerController.InAction)
            {
                // We check if there actually is something in front of them
                if (envScanner.ClimbLedgeCheck(transform.forward, out RaycastHit ledgeHit))
                {
                    // Passes the ledge and world coordinate of the specific part of ledge
                    currentPoint = GetNearestClimbPoint(ledgeHit.transform, ledgeHit.point);

                    // Don't allow player to move
                    playerController.SetControl(false);

                    // Then we perform our animation and movement using a coroutine (unity function)
                    StartCoroutine(JumpToLedge("IdleToHang", currentPoint.transform, 0.41f, 0.54f));
                }
            }

            // If the user wants to drop down to a ledge down below
            if (Input.GetButtonDown("Drop") && !playerController.InAction)
            {
                // Checks if there actually is a ledge
                if (envScanner.DropLedgeCheck(out RaycastHit ledgeHit))
                {
                    // Again, passes the ledge and world coordiante of that specific part of ledge
                    currentPoint = GetNearestClimbPoint(ledgeHit.transform, ledgeHit.point);

                    // Don't allow player to move
                    playerController.SetControl(false);

                    // Perform our animation to drop and adjust coordinates
                    StartCoroutine(JumpToLedge("DropToHang", currentPoint.transform, 0.30f, 0.45f, handOffset: new Vector3(0.25f, 0.2f, -0.2f)));
                }
            }
        }

        // If the player is currently hanging on to a ledge
        else
        {
            // If user wants to drop down from ledge
            if (Input.GetButton("Drop") && !playerController.InAction)
            {
                // Start function for jumping from a hanging ledge
                StartCoroutine(JumpFromHang());
            }

            // Gets the coordinates of current hanging position
            float h = Mathf.Round(Input.GetAxisRaw("Horizontal"));
            float v = Mathf.Round(Input.GetAxisRaw("Vertical"));
            var inputDir = new Vector2(h, v);

            // If we are performing an action, we can't do another simultaneously
            if (playerController.InAction || inputDir == Vector2.zero) return;

            // Check if the player can mount up to something
            if (currentPoint.MountPoint && inputDir.y == 1)
            {
                // Start function for mounting from a hang
                StartCoroutine(MountFromHang());
                return;
            }

            // If the player is hanging from a ledge and wants to jump to another ledge

            // Gets neighbor point if valid
            var neighbour = currentPoint.GetNeighbour(inputDir);
            if (neighbour == null) return;

            // Ensures there is a valid jump and that the player is holding the jump button
            if (neighbour.connectionType == ConnectionType.Jump && Input.GetButton("Jump"))
            {
                // Update the player's coordinates
                currentPoint = neighbour.point;

                // Handle depending on what button user clicks
                if (neighbour.direction.y == 1)
                    StartCoroutine(JumpToLedge("HangHopUp", currentPoint.transform, 0.35f, 0.65f, handOffset: new Vector3(0.25f, 0.08f, 0.15f)));

                else if (neighbour.direction.y == -1)
                    StartCoroutine(JumpToLedge("HangHopDown", currentPoint.transform, 0.31f, 0.65f, handOffset: new Vector3(0.25f, 0.1f, 0.13f)));

                else if (neighbour.direction.x == 1)
                    StartCoroutine(JumpToLedge("HangHopRight", currentPoint.transform, 0.20f, 0.50f));

                else if (neighbour.direction.x == -1)
                    StartCoroutine(JumpToLedge("HangHopLeft", currentPoint.transform, 0.20f, 0.50f));
            }

            // Checks if the user can "shimmy" along the ledge
            else if (neighbour.connectionType == ConnectionType.Move)
            {
                // Update player coordinates
                currentPoint = neighbour.point;

                // If the user is holding the "right" button then we perform our animation
                if (neighbour.direction.x == 1)
                    StartCoroutine(JumpToLedge("ShimmyRight", currentPoint.transform, 0f, 0.38f, handOffset: new Vector3(0.25f, 0.05f, 0.1f)));

                // If the user is holding the "left" button then we perform our animation
                else if (neighbour.direction.x == -1)
                    StartCoroutine(JumpToLedge("ShimmyLeft", currentPoint.transform, 0f, 0.38f, AvatarTarget.LeftHand, handOffset: new Vector3(0.25f, 0.05f, 0.1f)));
            }
        }
    }

    // Function to perform a jump to a ledge
    IEnumerator JumpToLedge(string anim, Transform ledge, float matchStartTime, float matchTargetTime, AvatarTarget hand=AvatarTarget.RightHand, Vector3? handOffset=null)
    {
        var matchParams = new MatchTargetParams()
        {
            pos = GetHandPos(ledge, hand, handOffset),
            bodyPart = hand,
            startTime = matchStartTime,
            targetTime = matchTargetTime,
            posWeight = Vector3.one
        };

        // Find the direction the user should be looking
        var targetRot = Quaternion.LookRotation(-ledge.forward);

        yield return playerController.DoAction(anim, matchParams, targetRot, true);

        playerController.IsHanging = true;
    }

    // Function to get hand position/coordinates
    Vector3 GetHandPos(Transform ledge, AvatarTarget hand, Vector3? handOffset)
    {
        var offsetValue = (handOffset != null) ? handOffset.Value : new Vector3(0.25f, 0.1f, 0.1f);

        var hDir = (hand == AvatarTarget.RightHand) ? ledge.right : -ledge.right;
        return ledge.position + ledge.forward * offsetValue.z + Vector3.up * offsetValue.y - hDir * offsetValue.x;
    }

    // Function to perform a jump from hanging onto a ledge
    IEnumerator JumpFromHang()
    {
        playerController.IsHanging = false;
        yield return playerController.DoAction("JumpFromHang");

        playerController.ResetTargetRotation();
        playerController.SetControl(true);
    }

    // Function for climbing up from hanging onto a ledge
    IEnumerator MountFromHang()
    {
        playerController.IsHanging = false;
        yield return playerController.DoAction("MountFromHang");

        playerController.EnableCharacterController(true);

        yield return new WaitForSeconds(0.5f);

        playerController.ResetTargetRotation();
        playerController.SetControl(true);
    }

    // Function to get the nearest climb point, for either climbing up to a ledge or dropping down to a ledge
    ClimbPoint GetNearestClimbPoint(Transform ledge, Vector3 hitPoint)
    {
        // Get all points of possible ledges in scene
        var points = ledge.GetComponentsInChildren<ClimbPoint>();

        ClimbPoint nearestPoint = null;
        float nearestPointDistance = Mathf.Infinity;

        // Loop through all possible ledges and check which distance is the shortest to player
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
