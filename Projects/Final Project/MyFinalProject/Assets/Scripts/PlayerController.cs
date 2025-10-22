using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 5.0f;
    private Rigidbody2D playerRb;
    private Vector2 movementInput;
    private Animator animator;

    void Start()
    {
        playerRb = GetComponent<Rigidbody2D>();
        // Ensure proper Rigidbody2D settings for pixel-perfect movement
        playerRb.gravityScale = 0f; // No gravity for top-down
        playerRb.freezeRotation = true; // Prevent rotation
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        // Get input in Update (best practice for input)
        movementInput.x = Input.GetAxisRaw("Horizontal");
        movementInput.y = Input.GetAxisRaw("Vertical");

        // Normalize diagonal movement to prevent speed boost
        if (movementInput.magnitude > 1f)
        {
            movementInput.Normalize();
        }
    }

    void FixedUpdate()
    {
        animator.SetBool("isWalking", true);

        if (movementInput.x == 0 && movementInput.y == 0)
        {
            animator.SetBool("isWalking", false);
            animator.SetFloat("LastInputX", movementInput.x);
            animator.SetFloat("LastInputY", movementInput.y);
        }


        // Apply velocity in FixedUpdate for physics
        playerRb.linearVelocity = movementInput * moveSpeed;

        animator.SetFloat("InputX", movementInput.x);
        animator.SetFloat("InputY", movementInput.y);
    }
}