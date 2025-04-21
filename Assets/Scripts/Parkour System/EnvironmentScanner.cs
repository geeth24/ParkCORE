using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EnvironmentScanner : MonoBehaviour
{
    [SerializeField] Vector3 forwardRayOffset = new Vector3(0, 2.5f, 0);
    [SerializeField] float forwardRayLength = 0.8f;
    [SerializeField] float heightRayLength = 5;
    [SerializeField] float ledgeRayLength = 10;
    [SerializeField] float climbLedgeRayLength = 1.5f;
    [SerializeField] LayerMask obstacleLayer;
    [SerializeField] LayerMask climbLedgeLayer;
    [SerializeField] float ledgeHeightThreshold = 0.75f;

    public ObstacleHitData ObstacleCheck()
    {
        var hitData = new ObstacleHitData();

        var forwardOrigin = transform.position + forwardRayOffset;
        hitData.forwardHitFound = Physics.Raycast(forwardOrigin, transform.forward, 
            out hitData.forwardHit, forwardRayLength, obstacleLayer);

        // Debug.DrawRay(forwardOrigin, transform.forward * forwardRayLength, (hitData.forwardHitFound) ? Color.red : Color.white);

        if (hitData.forwardHitFound)
        {
            var heightOrigin = hitData.forwardHit.point + Vector3.up * heightRayLength;
            hitData.heightHitFound = Physics.Raycast(heightOrigin, Vector3.down,
                out hitData.heightHit, heightRayLength, obstacleLayer);

            Debug.DrawRay(heightOrigin, Vector3.down * heightRayLength, (hitData.heightHitFound) ? Color.red : Color.white);
        }
        
        return hitData;
    }

    public bool ClimbLedgeCheck(Vector3 dir, out RaycastHit ledgeHit)
    {
        ledgeHit = new RaycastHit();

        if (dir == Vector3.zero)
            return false;

        var origin = transform.position + Vector3.up * 1.5f;
        var offset = new Vector3(0, 0.18f, 0);

        for (int i = 0; i < 10; i++)
        {
            Debug.DrawRay(origin + offset * i, dir);
            if (Physics.Raycast(origin + offset * i, dir, out RaycastHit hit, climbLedgeRayLength, climbLedgeLayer))
            {
                ledgeHit = hit;
                return true;
            }
        }

        return false;
    }

    public bool DropLedgeCheck(out RaycastHit ledgeHit)
    {
        ledgeHit = new RaycastHit();

        var origin = transform.position + Vector3.down * 0.1f + transform.forward * 2f;

        if (Physics.Raycast(origin, -transform.forward, out RaycastHit hit, 3, climbLedgeLayer))
        {
            ledgeHit = hit;
            return true;
        }

        return false;
    }

    public bool ObstacleLedgeCheck(Vector3 moveDir, out LedgeData ledgeData)
    {
        ledgeData = new LedgeData();

        if (moveDir == Vector3.zero)
            return false;

        float originOffset = 0.5f;
        var origin = transform.position + moveDir * originOffset + Vector3.up;

        if (PhysicsUtil.ThreeRaycasts(origin, Vector3.down, 0.25f, transform, 
            out List<RaycastHit> hits, ledgeRayLength, obstacleLayer, true))
        {
            var validHits = hits.Where(h => transform.position.y - h.point.y > ledgeHeightThreshold).ToList();

            if (validHits.Count > 0)
            {
                var surfaceRayOrigin = validHits[0].point;
                surfaceRayOrigin.y = transform.position.y - 0.1f;

                if (Physics.Raycast(surfaceRayOrigin, transform.position - surfaceRayOrigin, out RaycastHit surfaceHit, 2, obstacleLayer))
                {
                    Debug.DrawLine(surfaceRayOrigin, transform.position, Color.cyan);

                    float height = transform.position.y - validHits[0].point.y;

                    ledgeData.angle = Vector3.Angle(transform.forward, surfaceHit.normal);
                    ledgeData.height = height;
                    ledgeData.surfaceHit = surfaceHit;

                    return true;
                }
            }
        }

        return false;
    }
}

public struct ObstacleHitData
{
    public bool forwardHitFound;
    public bool heightHitFound;
    public RaycastHit forwardHit;
    public RaycastHit heightHit;
}

public struct LedgeData
{
    public float height;
    public float angle;
    public RaycastHit surfaceHit;
}
