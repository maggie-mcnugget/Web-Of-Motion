
using UnityEngine;

public class SpiderLegPlacement : MonoBehaviour
{
    [Header("References")]
    public Transform footTarget;
    public Transform legBase;
    public Transform spiderBody;

    [Header("Surface Detection")]
    public LayerMask surfaceLayers;
    public float detectionDistance = 2f;
    public float footOffset = 0.05f;
    public float outwardDistance = 0.5f;

    public bool TryFindFloor(out Vector2 footPosition)
    {
        footPosition = footTarget.position;

        float outwardDirection = Mathf.Sign(
            legBase.position.x - spiderBody.position.x
        );

        if (outwardDirection == 0)
            outwardDirection = 1f;

        // Search slightly outward from the leg base
        Vector2 rayOrigin = new Vector2(
            legBase.position.x + outwardDirection * outwardDistance,
            legBase.position.y
        );

        Debug.DrawRay(
            rayOrigin,
            Vector2.down * detectionDistance,
            Color.red
        );

        RaycastHit2D hit = Physics2D.Raycast(
            rayOrigin,
            Vector2.down,
            detectionDistance,
            surfaceLayers
        );

        if (hit.collider == null)
            return false;

        footPosition = hit.point + hit.normal * footOffset;

        return true;
    }
}

