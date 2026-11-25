using UnityEngine;

public class WebProjectile : MonoBehaviour
{
    [Header("Projectile Physics")]
    public float projectileRadius = 0.15f;
    public float lifetime = 4f;
    public LayerMask validSurfaceLayer;

    [Header("Climbing")]
    public float climbSpeed = 3f;

    [Header("Swinging")]
    public float swingAcceleration = 5f;
    public float swingDrag = 0.8f;
    public float maxSwingSpeed = 8f;

    private Transform playerTransform;
    private Rigidbody2D playerRb;

    private float originalGravityScale;
    private float gravityValue = 9.8f;

    private Vector2 playerVelocity;
    private Vector2 radialDirection;

    private Transform playerOrigin;
    private float webLength;
    private float currentDistance;

    private Vector2 startingPosition;
    private Vector2 startingVelocity;
    private float gravity;
    private float elapsedTime = 0f;

    private bool isAttached = false;

    private BoxCollider2D[] validSurfaces;
    private LineRenderer webLine;

    private Vector2 webDirection;

    void Start()
    {
        // find all box colliders in the scene
        BoxCollider2D[] allSurfaces =
            FindObjectsByType<BoxCollider2D>(FindObjectsSortMode.None);

        // store only colliders based on layer
        System.Collections.Generic.List<BoxCollider2D> surfaces =
            new System.Collections.Generic.List<BoxCollider2D>();

        foreach (BoxCollider2D surface in allSurfaces)
        {
            if ((validSurfaceLayer.value &
                (1 << surface.gameObject.layer)) != 0)
            {
                surfaces.Add(surface);
            }
        }

        validSurfaces = surfaces.ToArray();

        // get line renderer
        webLine = GetComponent<LineRenderer>();

        if (webLine != null)
        {
            webLine.positionCount = 2;
            webLine.enabled = false;
        }
    }

    public void Launch(Vector2 velocity, float gravityValue)
    {
        startingPosition = transform.position;
        startingVelocity = velocity;
        gravity = gravityValue;
    }

    void Update()
    {
        // if atttached, keep web
        if (isAttached)
        {
            return;
        }

        elapsedTime += Time.deltaTime;

        // destroy if not attached
        if (elapsedTime >= lifetime)
        {
            Destroy(gameObject);
            return;
        }

        // calculate projectiles new position
        float x = startingPosition.x
            + startingVelocity.x * elapsedTime;

        float y = startingPosition.y
            + startingVelocity.y * elapsedTime
            + 0.5f * gravity * elapsedTime * elapsedTime;

        Vector2 newPosition = new Vector2(x, y);

        // check path between previous and new position
        Vector2 oldPosition = transform.position;

        float distance = Vector2.Distance(oldPosition, newPosition);

        // reduces chance of passing through a surface by checking multiple points along the path
        int steps = Mathf.Max(
            1,
            Mathf.CeilToInt(distance / (projectileRadius * 0.5f))
        );

        for (int i = 1; i <= steps; i++)
        {
            float fraction = (float)i / steps;

            Vector2 checkPosition = Vector2.Lerp(
                oldPosition,
                newPosition,
                fraction
            );

            if (CheckForSurface(checkPosition))
            {
                return;
            }
        }

        // no valid surface found, update position
        transform.position = newPosition;
    }

    bool CheckForSurface(Vector2 position)
    {
        foreach (BoxCollider2D surface in validSurfaces)
        {
            if (surface == null || !surface.enabled ||
                !surface.gameObject.activeInHierarchy)
            {
                continue;
            }

            // gets the bounds of the surface
            Bounds bounds = surface.bounds;

            // closest point on surface to the projectile's position
            float closestX = Mathf.Clamp(
                position.x, bounds.min.x, bounds.max.x
            );

            float closestY = Mathf.Clamp(
                position.y, bounds.min.y, bounds.max.y
            );

            Vector2 closestPoint = new Vector2(closestX, closestY);

            // distance between the projectile and the closest point on the surface
            float distanceSquared =
                (position - closestPoint).sqrMagnitude;

            // check if projectile is close enough to attach to the surface
            if (distanceSquared <= projectileRadius * projectileRadius)
            {
                AttachToSurface(closestPoint);
                return true;
            }
        }

        return false;
    }

    void AttachToSurface(Vector2 anchorPoint)
    {
        transform.position = anchorPoint;
        isAttached = true;

        // find player
        GameObject player = GameObject.FindGameObjectWithTag("Player");

        if (player != null)
        {
            playerTransform = player.transform;
            playerOrigin = player.transform.Find("WebOrigin");
            playerRb = player.GetComponent<Rigidbody2D>();

            if (playerOrigin != null && playerRb != null)
            {
                // store og web length
                webLength = Vector2.Distance(
                    playerOrigin.position,
                    anchorPoint
                );

                currentDistance = webLength;

                // store og gravity scale
                originalGravityScale = playerRb.gravityScale;

                // disable unity gravity while attached
                playerRb.gravityScale = 0f;

                // tell spider script that player is on web
                SpiderMovement movement =
                    player.GetComponent<SpiderMovement>();

                if (movement != null)
                    movement.isOnWeb = true;

                // store initial velocity
                playerVelocity = playerRb.linearVelocity;

                // direction from player origin to anchor point
                radialDirection = (
                    (Vector2)playerOrigin.position - anchorPoint
                ).normalized;
            }
        }

        if (webLine != null)
        {
            webLine.enabled = true;
            UpdateWebLine();
        }
    }

    void UpdateWebLine()
    {
        if (webLine == null)
            return;

        // connect the web line to the player and the projectile
        GameObject player = GameObject.FindGameObjectWithTag("Player");

        if (player == null)
            return;

        Transform origin = player.transform.Find("WebOrigin");

        if (origin == null)
            return;

        webLine.SetPosition(0, origin.position);
        webLine.SetPosition(1, transform.position);
    }


    void FixedUpdate()
    {
        if (!isAttached || playerRb == null || playerOrigin == null)
            return;

        // check if spider is on the ground
        SpiderMovement movement =
            playerTransform.GetComponent<SpiderMovement>();

        bool isGrounded = movement != null && movement.isGrounded;

        float dt = Time.fixedDeltaTime;

        // W/S for climbing
        float climbInput = 0f;

        if (Input.GetKey(KeyCode.W))
            climbInput = 1f;

        if (Input.GetKey(KeyCode.S))
            climbInput = -1f;

        // change web length when climbing
        currentDistance -= climbInput * climbSpeed * dt;

        // stops web from being too short or too long
        currentDistance = Mathf.Clamp(currentDistance, 0.5f, webLength);

        // if grounded, climb along the web
        if (isGrounded)
        {
            if (climbInput != 0)
            {
                // Get direction from the spider toward the anchor
                Vector2 directionToAnchor =
                    ((Vector2)transform.position - (Vector2)playerOrigin.position).normalized;

                // W moves toward anchor, S moves away
                Vector2 climbMovement =
                    directionToAnchor * climbInput * climbSpeed * dt;

                playerRb.MovePosition(playerRb.position + climbMovement);

                playerVelocity = climbMovement / dt;

                currentDistance = Vector2.Distance(
                    playerOrigin.position,
                    transform.position
                );
            }
            else
            {
                // Keep the stored velocity in sync with normal ground movement
                playerVelocity = playerRb.linearVelocity;
            }

            // Keep the radial direction updated while walking on the ground
            radialDirection = (
                (Vector2)playerOrigin.position - (Vector2)transform.position
            ).normalized;

            return;
        }

        // a/d for swinging
        float swingInput = Input.GetAxisRaw("Horizontal");

        // calculate direction from anchor to player origin
        Vector2 tangent = new Vector2(
            -radialDirection.y,
            radialDirection.x
        );

        // gravity applied manually
        playerVelocity += Vector2.down * gravityValue * dt;

        // apply swinging input to the player velocity
        playerVelocity += tangent * swingInput * swingAcceleration * dt;

        // apply air resistance to gradually reduce momentum
        playerVelocity *= Mathf.Exp(-swingDrag * dt);

        // prevent swinging speed from becoming too high
        playerVelocity = Vector2.ClampMagnitude(
            playerVelocity,
            maxSwingSpeed
        );

        // remember the offset from the player origin to the player position
        Vector2 originOffset =
            (Vector2)playerOrigin.position - playerRb.position;

        // predict player movement based on current velocity
        Vector2 predictedPosition =
            playerRb.position + playerVelocity * dt;

        // predict webOrigin position
        Vector2 predictedOrigin = predictedPosition + originOffset;

        // calculate direction from the anchor to the predicted player origin
        radialDirection = (predictedOrigin - (Vector2)transform.position).normalized;

        // keep spider correct distance from the anchor point
        Vector2 correctedOrigin =
            (Vector2)transform.position + radialDirection * currentDistance;

        // calculate correct player position
        Vector2 correctedPosition = correctedOrigin - originOffset;

        // calculate the velocity needed to move the player to the corrected position
        playerVelocity = (correctedPosition - playerRb.position) / dt;

        // remove velocity that would move the player away from the anchor point
        playerVelocity -= Vector2.Dot(playerVelocity, radialDirection) * radialDirection;

        // apply calculate movement to the player
        playerRb.MovePosition(correctedPosition);
        playerRb.linearVelocity = playerVelocity;
    }

    void LateUpdate()
    {
        if (isAttached)
        {
            UpdateWebLine();
        }
    }

    public void ReleaseWeb()
    {
        if (!isAttached)
            return;

        isAttached = false;

        if (playerRb != null)
        {
            // Restore normal gravity
            playerRb.gravityScale = originalGravityScale;

            // Keep the velocity built up while swinging
            playerRb.linearVelocity = playerVelocity;
        }

        // Allow normal player movement again
        SpiderMovement movement =
            playerTransform.GetComponent<SpiderMovement>();

        if (movement != null)
            movement.isOnWeb = false;
    }

}
