using UnityEngine;

public class SpiderLegStepper : MonoBehaviour
{
    [Header("References")]
    public Transform footTarget;
    public Transform spiderBody;
    public SpiderMovement spiderMovement;
    public SpiderLegPlacement legPlacement;

    [Header("Stepping")]
    public float stepDistance = 0.6f;
    public float stepLength = 0.3f;
    public float stepHeight = 0.5f;
    public float stepSpeed = 3.5f;

    [Header("Leg Reach")]
    public float minLegReach = 0.35f;
    public float maxLegReach = 1.5f;

    [Header("Body Clearance")]
    public float minBodyDistance = 0.7f;

    private bool isStepping;
    public bool IsStepping => isStepping;
    private Vector2 stepStart;
    private Vector2 stepEnd;
    private float stepProgress;

    private Vector2 homeOffset;
    private float outwardDirection;

    private Vector2 previousBodyPosition;

    public SpiderLegCoordinator coordinator;
    public int legNumber;

    void Start()
    {
        // Remember the foot's original position relative to the body
        homeOffset = (Vector2)footTarget.position -
                     (Vector2)spiderBody.position;

        // Determine which side of the body this leg belongs to
        outwardDirection = Mathf.Sign(homeOffset.x);

        if (outwardDirection == 0)
        {
            outwardDirection = Mathf.Sign(
                legPlacement.legBase.position.x - spiderBody.position.x
            );
        }

        previousBodyPosition = spiderBody.position;
    }


    void Update()
    {
        // Track how far the spider body moved this frame
        Vector2 bodyMovement =
            (Vector2)spiderBody.position - previousBodyPosition;

        // If the foot is planted, keep it in the same world position
        if (!isStepping)
        {
            footTarget.position -= (Vector3)bodyMovement;
        }
        else
        {
            // If the foot is stepping, move its path along with the body
            stepStart += bodyMovement;
            stepEnd += bodyMovement;
        }

        // Remember the body's current position
        previousBodyPosition = spiderBody.position;

        // Print a status message once per second
        if (Time.frameCount % 60 == 0)
        {
            Debug.Log(
                name +
                " | Script running" +
                " | Input: " + Input.GetAxisRaw("Horizontal") +
                " | Is Stepping: " + isStepping
            );
        }

        if (isStepping)
        {
            UpdateStep();
            return;
        }

        float movementInput = Input.GetAxisRaw("Horizontal");

        if (movementInput == 0)
            return;

        // Measure the distance between the leg base and planted foot
        float distanceFromBase = Vector2.Distance(
            legPlacement.legBase.position,
            footTarget.position
        );

        // Start a step when the leg is stretched too far
        if (distanceFromBase < stepDistance)
            return;

        Debug.Log(name + ": Attempting to start a step!");

        TryStartStep(movementInput);
    }


    void TryStartStep(float movementInput)
    {
        if (!coordinator.CanLegStep(legNumber))
            return;

        Vector2 floorPosition;

        if (!legPlacement.TryFindFloor(out floorPosition))
        {
            Debug.Log(name + ": Could not find floor!");
            return;
        }

        // Determine whether this leg is on the side we're walking toward
        bool isLeadingLeg = outwardDirection == Mathf.Sign(movementInput);

        // Leading leg takes a normal step
        // Opposite leg takes a shorter step
        float currentStepLength = isLeadingLeg
            ? stepLength
            : stepLength * 0.5f;

        // Calculate the new foot position
        float targetX = spiderBody.position.x +
                        homeOffset.x +
                        movementInput * currentStepLength;

        Vector2 proposedPosition = new Vector2(
            targetX,
            floorPosition.y
        );

        // Keep the foot outside the minimum body clearance
        proposedPosition = KeepFootAwayFromBody(proposedPosition);

        // Check whether the leg can physically reach this position
        float distanceFromBase = Vector2.Distance(
            legPlacement.legBase.position,
            proposedPosition
        );

        Debug.Log(name + ": Proposed foot distance = " + distanceFromBase);

        if (distanceFromBase < minLegReach ||
            distanceFromBase > maxLegReach)
        {
            Debug.Log(name + ": Step rejected because of leg reach!");
            return;
        }

        stepStart = footTarget.position;
        stepEnd = proposedPosition;

        isStepping = true;
        stepProgress = 0f;

        coordinator.SetLegStepping(legNumber, true);

        Debug.Log(name + ": Step started!");
    }

    void UpdateStep()
    {
        stepProgress += Time.deltaTime * stepSpeed;

        float progress = Mathf.Clamp01(stepProgress);

        Vector2 position = Vector2.Lerp(
            stepStart,
            stepEnd,
            progress
        );

        // Lift the foot in an arc
        position.y += Mathf.Sin(progress * Mathf.PI) * stepHeight;

        position = KeepFootAwayFromBody(position);

        footTarget.position = position;

        if (progress >= 1f)
        {
            Vector2 groundPosition;

            if (legPlacement.TryFindFloor(out groundPosition))
            {
                Debug.Log(
                    name +
                    " | Foot before correction: " + footTarget.position +
                    " | Ground detected: " + groundPosition +
                    " | Vertical difference: " +
                    Mathf.Abs(footTarget.position.y - groundPosition.y)
                );

                stepEnd = groundPosition;
            }

            stepEnd = KeepFootAwayFromBody(stepEnd);

            isStepping = false;
            coordinator.SetLegStepping(legNumber, false);
        }
    }

    Vector2 KeepFootAwayFromBody(Vector2 footPosition)
    {
        Vector2 bodyPosition = spiderBody.position;

        Vector2 direction = footPosition - bodyPosition;

        // Check whether the foot is too close to the body
        if (direction.magnitude < minBodyDistance)
        {
            // Make sure we have a valid direction
            if (direction.sqrMagnitude < 0.001f)
            {
                direction = new Vector2(outwardDirection, 0f);
            }

            // Push the foot out to the minimum distance
            footPosition = bodyPosition +
                           direction.normalized * minBodyDistance;
        }

        return footPosition;
    }
}

