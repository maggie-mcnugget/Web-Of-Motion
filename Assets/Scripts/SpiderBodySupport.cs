
using UnityEngine;

public class SpiderBodySupport : MonoBehaviour
{
    [Header("Leg References")]
    public SpiderLegStepper[] legs;

    [Header("Body Height")]
    public float bodyClearance = 0.9f;
    public float minimumHeight = 0.5f;
    public float maximumHeight = 4f;

    [Header("Stability")]
    public float positionStrength = 25f;
    public float damping = 10f;

    private Rigidbody2D rb;
    private SpiderMovement movement;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        movement = GetComponent<SpiderMovement>();
    }

    void FixedUpdate()
    {
        if (movement != null && !movement.isGrounded)
            return;

        if (legs == null || legs.Length == 0)
            return;

        float totalHeight = 0f;
        int plantedLegCount = 0;

        foreach (SpiderLegStepper leg in legs)
        {
            if (leg == null || leg.IsStepping || leg.footTarget == null)
                continue;

            totalHeight += leg.footTarget.position.y;
            plantedLegCount++;
        }

        if (plantedLegCount == 0)
            return;

        float averageFootHeight = totalHeight / plantedLegCount;

        float clearance = Mathf.Clamp(
            bodyClearance,
            minimumHeight,
            maximumHeight
        );

        float targetHeight = averageFootHeight + clearance;

        float heightDifference = targetHeight - rb.position.y;

        float acceleration =
            (heightDifference * positionStrength) -
            (rb.linearVelocity.y * damping);

        acceleration -= Physics2D.gravity.y * rb.gravityScale;

        rb.AddForce(Vector2.up * acceleration, ForceMode2D.Force);
    }
}


