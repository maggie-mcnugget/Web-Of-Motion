using UnityEngine;

public class SpiderMovement : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f;
    public float jumpForce = 7f;
    public float acceleration = 20f;
    public float deceleration = 10f;

    [Header("Air Movement")]
    public float airAcceleration = 8f;
    public float airDeceleration = 1.5f;

    [Header("Ground Detection")]
    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;
    public LayerMask groundLayer;

    private Rigidbody2D rb;
    private float moveInput;
    public bool isGrounded;

    public bool isOnWeb = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        // horizontal input
        moveInput = Input.GetAxisRaw("Horizontal");

        // check if spider is grounded
        isGrounded = Physics2D.OverlapCircle(
            groundCheck.position,
            groundCheckRadius,
            groundLayer
        );

        // jump if spider is grounded and space key is pressed
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded && !isOnWeb)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        }
    }

    void FixedUpdate()
    {
        // allow normal movement when grounded even with web
        if (isOnWeb && !isGrounded)
        {
            return;
        }

        if (Time.frameCount % 20 == 0)
        {
            Debug.Log(
                $"Input: {moveInput} | " +
                $"Grounded: {isGrounded} | " +
                $"On Web: {isOnWeb} | " +
                $"Velocity X: {rb.linearVelocity.x}"
            );
        }

        // determine the target horizontal speed
        float targetSpeed = moveInput * moveSpeed;

        // choose movement settings based on whether the spider is grounded
        float speedChange;

        if (isGrounded)
        {
            speedChange = moveInput != 0 ? acceleration : deceleration;
        }
        else
        {
            speedChange = moveInput != 0 ? airAcceleration : airDeceleration;
        }

        // gradually move toward the target speed
        float newSpeed = Mathf.MoveTowards(
            rb.linearVelocity.x,
            targetSpeed,
            speedChange * Time.fixedDeltaTime
        );

        // apply horizontal movement while keeping vertical velocity
        rb.linearVelocity = new Vector2(
            newSpeed,
            rb.linearVelocity.y
        );
    }
}
