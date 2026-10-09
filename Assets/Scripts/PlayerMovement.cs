using UnityEngine;

// Moves the player with a Rigidbody: 
// A/D or Left/Right arrows to ROTATE, W/S or Up/Down to move Forward/Backward, Space to jump.
[RequireComponent(typeof(Rigidbody))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 7f;      // units per second
    [SerializeField] private float turnSpeed = 150f;    // degrees per second
    [SerializeField] private float jumpForce = 5f;      // strength of the jump
    [SerializeField] private float doubleJumpForce = 4f;

    [Header("Ground Check Settings")]
    [SerializeField] private float groundDistance = 0.5f; // ray length
    [SerializeField] private LayerMask groundMask;        // ground layers

    public float sensitivityX = 3.0f;
    private Rigidbody rb;               // the physics body we move
    private Vector3 moveDirection;      // worked out from input in Update()
    private float turnInput;            // horizontal input for rotation
    private bool isGrounded;            // true when standing on the ground
    private bool jumpRequested = false; // set in Update(), used later
    private bool doubleJumpRequested = false; // set in TryDoubleJump()
    private bool hasDoubleJumped = false;     // one per jump


    public bool IsGrounded => isGrounded;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.constraints = RigidbodyConstraints.FreezeRotation;
    }

    private void Update()
    {
        // 1. Ground Check
        isGrounded = Physics.Raycast(transform.position + transform.up*groundDistance/2, -transform.up, groundDistance, groundMask);

        // 2. Read Inputs
        float moveX = Input.GetAxisRaw("Horizontal"); // A/D or Left/Right turn input
        float moveZ = Input.GetAxisRaw("Vertical");  // W/S or Up/Down forward/backward

        Vector3 forward = Camera.main.transform.forward;
        Vector3 right = Camera.main.transform.right;

        // Ignore Y-axis movement to prevent unwanted tilting
        forward.y = 0f;
        right.y = 0f;

        //transform.forward = forward;
        // Calculate forward movement relative to current facing direction
        moveDirection = forward * moveZ + right * moveX;

        if (moveDirection.magnitude > 0f)
            transform.forward = moveDirection.normalized;

        // 3. Handle Jump Input
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            jumpRequested = true;
        }
        // Back on the ground: the double jump is available again
        if (isGrounded)
        {
            hasDoubleJumped = false;
        }

    }

    private void FixedUpdate()
    {
        MovePlayer();

        if (jumpRequested)
        {
            Jump(jumpForce);
            jumpRequested = false;
        }

        // NEW: the second jump, in mid-air
        if (doubleJumpRequested)
        {
            Jump(doubleJumpForce);
            doubleJumpRequested = false;
        }
    }
 
    private void Jump(float force)
    {
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f,
                                        rb.linearVelocity.z);
        rb.AddForce(Vector3.up * force, ForceMode.VelocityChange);
    }

    private void MovePlayer()
    {
        // Calculate velocity based on current forward vector
        Vector3 targetVelocity = moveDirection * moveSpeed;

        // Apply movement while preserving vertical velocity (gravity/jumping)
        rb.linearVelocity = new Vector3(targetVelocity.x, rb.linearVelocity.y, targetVelocity.z);
    }

 
    // Tries to jump a second time in mid-air.
    // Returns true if the double jump really happened.
    public bool TryDoubleJump()
    {
        // Not allowed on the ground, or if it was already used
        if (isGrounded || hasDoubleJumped) return false;
 
        doubleJumpRequested = true;
        hasDoubleJumped = true;
        return true;
    }

}