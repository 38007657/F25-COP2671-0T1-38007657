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
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        // Get input in Update 
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

        // Apply velocity in FixedUpdate for physics
        playerRb.linearVelocity = movementInput * moveSpeed;


    }
}